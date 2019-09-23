import numpy as np
import os
import math

idx = {'Dwell': 0, 'GazeDock': 1}

users = ['1', '2', '3', '4', '5', '6', '7', '8']
for user in users:
	print('user ' + user)
	filenames = os.listdir(user + '/')
	forward_t = []
	menu_t = [[], []]
	fail_t = [[], []]
	trigger_t = []
	big_angle = [0, 0]
	start = [0, 0]
	last_theta = 0
	during = [[], []]
	for filename in filenames:
		if filename == '.DS_Store': continue
		_ = idx[filename.split('.')[0].split('-')[-1]]
		f = open(user + '/' + filename, 'r')
		n_line = 0
		during_start = -1
		while True:
			n_line += 1
			line = f.readline()
			if len(line) == 0: break
			arr = line[:-1].split(' ')
			t = int(arr[0])
			if during_start == -1:
				during_start = t
			during_end = t
			op = arr[1]
			if op == 'dwell':
				if arr[2] == 'start':
					start[_] = t
			elif op == 'forward':
				now_t = t - start[_]
				if arr[2] == 'gaze':
					forward_t.append(now_t)
				else:
					menu_t[_].append(now_t)
			elif op == 'trigger':
				trigger_bf_t = start[_]
				trigger_t.append( t - start[_] )
			elif op == 'pick' or op == 'drop':
				now_t = t - start[_]
				if _ == 0: now_t = t - trigger_bf_t
				if arr[2] == '0':
					fail_t[_].append(now_t)
				else:
					menu_t[_].append(now_t)
			elif op == 'exit':
				now_t = t - start[_]
				menu_t[_].append(now_t)
			elif op == 'ringup':
				start[_] = t
			elif op == 'ringdown':
				pass
			elif op == 'move':
				now_t = t - start[_]
				menu_t[_].append(now_t)
			elif op == 'gaze':
				x = float(arr[2])
				y = float(arr[3])
				z = float(arr[4])
				length = math.sqrt(x*x+y*y+z*z)
				if z < 0 or length < 1e-5:
					continue
				x /= length
				y /= length
				z /= length
				theta = math.acos(z) * 180 / math.pi
				phi = math.atan2(y, x) * 180 / math.pi
				if theta > 25 and theta < 45 and last_theta < 30 and phi > 135 and phi < 225:
					big_angle[_] += 1
				if theta > 30 and theta < 45 and last_theta < 30 and (phi > 315 or phi < 45):
					big_angle[_] += 1
				last_theta = theta
		f.close()
		during[_].append(during_end - during_start)
	forward_t = np.array(forward_t)
	trigger_t = np.array(trigger_t)
	for i in range(2):
		menu_t[i] = np.array(menu_t[i])
		fail_t[i] = np.array(fail_t[i])
		during[i] = np.array(during[i])
	print('dwell=' + str(menu_t[0].mean()-660) + ' (' +  str(menu_t[0].shape[0]) + ')   forward=' + str(forward_t.mean()-330) + ' (' + str(forward_t.shape[0]) + ')')
	print('gazedock=' + str(menu_t[1].mean()+200) + ' (' + str(menu_t[1].shape[0]) + ')   fp=' + str(max(fail_t[1].shape[0]-2,0)) + '   fpr=' + str((fail_t[1].shape[0]-2) / during[1].sum() * 1000 * 60))