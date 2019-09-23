import numpy as np
import os
import math
import pickle
import matplotlib.pyplot as plt

def read_study1():
	gaze = []
	minus_one = False
	for i in range(1, 13):
		ddir = 'study 1/' + str(i) + '/'
		filenames = os.listdir(ddir)
		for filename in filenames:
			if filename == '.DS_Store': continue
			f = open(ddir + filename)
			while True:
				line = f.readline()
				if len(line) == 0: break
				arr = line[:-1].split(' ')
				try:
					x = float(arr[0])
					y = float(arr[1])
					z = float(arr[2])
				except:
					continue

				length = math.sqrt(x*x+y*y+z*z)
				if z < 0 or length < 1e-5:
					if not minus_one:
						gaze.append([-1, -1, -1])
						minus_one = True
					continue
				minus_one = False
				x /= length
				y /= length
				z /= length

				theta = math.acos(z) * 180 / math.pi
				phi = math.atan2(y, x) * 180 / math.pi
				gaze.append([theta, phi, math.sqrt(x*x+y*y)])
			f.close()
	gaze = np.array(gaze)
	print(gaze.shape)
	pickle.dump(gaze, open('study 1/study 1g.pkl', 'wb'))
	print(gaze.shape)

def read_study1_inten():
	gaze = []
	minus_one = False
	ddir = 'study 1/inten/'
	filenames = os.listdir(ddir)
	for filename in filenames:
		if filename == '.DS_Store': continue
		f = open(ddir + filename)
		while True:
			line = f.readline()
			if len(line) == 0: break
			arr = line[:-1].split(' ')
			try:
				x = float(arr[0])
				y = float(arr[1])
				z = float(arr[2])
			except:
				continue

			length = math.sqrt(x*x+y*y+z*z)
			if z < 0 or length < 1e-5:
				if not minus_one:
					gaze.append([-1, -1])
					minus_one = True
				continue
			minus_one = False
			x /= length
			y /= length
			z /= length

			theta = math.acos(z) * 180 / math.pi
			phi = math.atan2(y, x) * 180 / math.pi
			gaze.append([theta, phi])
		f.close()
	gaze = np.array(gaze)
	pickle.dump(gaze, open('study 1 inten.pkl', 'wb'))
	print(gaze.shape)

def read_study2():
	bs = ['990', '997', '999']
	selections = []
	minus_one = False
	blink_cnt = 0
	prev_l = -1
	for i in range(len(bs)):
		selection = []
		ddir = 'study 2/' + bs[i] + '/'
		filenames = os.listdir(ddir)
		for filename in filenames:
			if filename == '.DS_Store': continue
			f = open(ddir + filename, 'r')
			while True:
				line = f.readline()
				if len(line) == 0: break
				arr = line[:-1].split(' ')
				l = int(arr[0])
				x = float(arr[1])
				y = float(arr[2])
				z = float(arr[3])

				length = math.sqrt(x*x+y*y+z*z)
				if z < 0 or length < 1e-5:
					if not minus_one:
						gaze.append([-1, -1])
						blink_cnt += 1
						minus_one = True
					continue
				minus_one = False
				x /= length
				y /= length
				z /= length

				theta = math.acos(z) * 180 / math.pi
				phi = math.atan2(y, x) * 180 / math.pi
				if l != prev_l: selection.append([-1, -1])
				prev_l = l
				selection.append([theta, phi])
			f.close()
		selections.append(np.array(selection))
	return selections

def smooth(a):
	for i in range(1, len(a)-1):
		if a[i, 0] < 0:
			a[i] = (a[i-1] + a[i+1]) / 2
	return a

def get_fp(g, b):
	WIN = 200
	SPAN = 100
	n = g.shape[0]
	i = SPAN
	cnt = 0
	fp = []
	while i < n-SPAN:
		if g[i][0] < 50 and g[i][0] > b[int(g[i][1])]:
			minus = False
			for j in range(i-WIN//2, i+WIN//2):
				if g[j][0] < 0:
					minus = True
			if minus == False:
				fp.append((i-100,i+100))
				cnt += 1
				i += WIN
		i += 1
	return fp

def get_tp(g, b):
	tp = []
	i = 1
	n = g.shape[0]
	while i < n:
		j = i + 1
		while j < n and g[j, 0] >= 0: j += 1
		tp.append((i, j))
		i = j + 1
	return tp

unint = pickle.load(open('study 1/study 1.pkl', 'rb'))
unint = np.array([[c[0], c[1]] for c in unint])
inten = pickle.load(open('study 1/study 1 inten.pkl', 'rb'))
b = pickle.load(open('study 1/border.pkl', 'rb'))
# study2 = pickle.load(open('study 2/study 2.pkl', 'rb'))

def plot_detail(a, b, print_lr=False):
	n = len(a)
	for i in range(24, n, 24):
		for j in range(min(24, n-i)):
			l, r = a[i+j]
			if print_lr: print(l % 1000000, r % 1000000)
			# plt.subplot(4, 6, j+1)
			# plt.plot(list(range(l,r)), b[l:r,0])
			# plt.plot(list(range(l,r)), b[l:r,1])

			plt.ylim(0, 40)
			l += 25
			r += 25
			plt.plot(list(range(l,r)), b[l:r,0])
			plt.show()
		plt.show()


UP_WIN = 48
MID_WIN = 48
UP_START_THRES = 11
UP_PHI_STD = 15
MID_PHI_STD = 7
ANGLE_THRES = 10
# print(UP_WIN, UP_START_THRES)

def sstd(phis, printed=False):
	phis = np.array(phis)
	if printed: print(phis)
	if phis.min() < -160 and phis.max() > 160:
		phis = (phis < 0) * (phis + 360) + (phis >= 0) * phis
	if printed: print(phis)
	if printed: print(phis.std())
	return phis.std()

def detect(g, b):
	state = 0
	phis = []
	detected = []
	n = g.shape[0]
	show = list(range(11147, 281))
	for i in range(1, n):
		if g[i, 0] < 0:
			state = 0
			continue
		if state == 0:
			if g[i, 0] > g[i-1, 0] or len(phis) > 0 and g[i, 0] > g[i-1, 0] - UP_DROP_THRES:
				phis.append(g[i, 1])
				start = i
			else:
				# 超过一定时间，极角稳定，眼动超过阈值
				if len(phis) > UP_WIN and steady(phis, UP_PHI_STD) and g[i, 0] > b[int(g[i, 1])] - ANGLE_THRES:
					state = 1
					phis = [g[i, 1]]
				else:
					state = 0
					phis = []
		elif state == 1:
			if g[i, 0] > b[int(g[i, 1])] - ANGLE_THRES:
				phis.append(g[i, 1])
			else:
				# 超过一定时间，极角稳定
				if len(phis) > MID_WIN and steady(phis, MID_PHI_STD):
					detected.append((max(start-10, 0), min(i+10, n)))
				state = 0
				phis = []

		if i in show:
			print(i, state, len(phis), np.array(phis).std(), b[int(g[i, 1])])
	return detected

def detect2(g, b, printed=False):
	n = g.shape[0]
	i = UP_WIN + MID_WIN + 1
	detected = []
	show = list(range(1682351, 682481))
	while i < n:
		if g[i-UP_WIN-MID_WIN, 0] > UP_START_THRES or min(g[i-MID_WIN, 0], g[i, 0]) < b[int(g[i, 1])] - ANGLE_THRES:
			i += 1
			continue
		u = g[i-UP_WIN-MID_WIN : i-MID_WIN]
		m = g[i-MID_WIN : i]
		if i in show and printed:
			print(i, u[0, 0], sstd(u[:,1]), sstd(m[:,1]), [g[i, 0], b[int(g[i, 1])] - ANGLE_THRES] )
		if u[:,0].min() < 0:
			i += u[:,0].argmin() + 1
			continue
		if m[:,0].min() < 0:
			i += m[:,0].argmin() + UP_WIN + 1
			continue
		# if g[i-UP_WIN-MID_WIN-10:i-UP_WIN-MID_WIN, 0].max() > 8:
		# 	i += 1
		# 	continue
		if sstd(u[:,1]) < UP_PHI_STD and sstd(m[:,1]) < MID_PHI_STD:
			if i in show and printed:
				print('bingo', i)
			# print(g[i, 2])
			detected.append([ max(i-UP_WIN-MID_WIN-10, 0), min(i+20, n) ])
			i += UP_WIN + MID_WIN
		i += 1
	return detected


gazeb = 0

fp = get_fp(unint, b[gazeb])
print('false positive:', len(fp))
plot_detail(fp, unint)

# tp = get_fp(inten, b[gazeb])
# print('false positive:', len(tp))
# plot_detail(tp, unint)

# tp = get_tp(study2[gazeb], b[gazeb])
# print('true positive:', len(tp))
# plot_detail(tp, inten)

# tpd = detect2(study2[gazeb], b[gazeb])
# tpd = detect2(inten, b[gazeb])
# print('tpd: ', len(tpd))

# fpd = detect2(unint, b[gazeb], True)
# print('fpd: ', len(fpd))
# plot_detail(fpd, unint, True)

# fpd = detect2(unten, b[2])
# print('tpd', len())

# not_detect = []
# for l, r in tp:
# 	flag = False
# 	for x, y in d:
# 		if not (r <= x or y <= l):
# 			flag = True
# 			break
# 	if not flag:
# 		not_detect.append((l, r))
# print(len(not_detect))
# plot_detail(not_detect, inten, True)