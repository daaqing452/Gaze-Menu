import numpy as np
import os
import math
import pickle
import matplotlib.pyplot as plt

def read_study1():
	gazes = []
	minus_one = False
	for i in range(1, 13):
		ddir = 'study 1/' + str(i) + '/'
		filenames = os.listdir(ddir)
		gaze = []
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
		gazes.append(np.array(gaze))
	pickle.dump(gazes, open('study 1/gazeperuser', 'wb'))
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

def outside(g, el):
	theta = g[2]
	x = theta * math.cos(g[3] / 180 * math.pi)
	y = theta * math.sin(g[3] / 180 * math.pi)
	q = math.pow(x - el[0], 2) / el[2] / el[2] + math.pow(y - el[1], 2) / el[3] / el[3]
	return 1 if q > 1 else -1

def detect3(g, el):
	n = g.shape[0]
	i = UP_WIN + MID_WIN + 1
	detected = []
	show = list(range(1682351, 682481))
	while i < n:
		if g[i-UP_WIN-MID_WIN, 0] > UP_START_THRES or outside(g[i-MID_WIN], el) < 0 or outside(g[i], el) < 0:
			i += 1
			continue
		u = g[i-UP_WIN-MID_WIN : i-MID_WIN]
		m = g[i-MID_WIN : i]
		if u[:,0].min() < 0:
			i += u[:,0].argmin() + 1
			continue
		if m[:,0].min() < 0:
			i += m[:,0].argmin() + UP_WIN + 1
			continue
		# if sstd(u[:,1]) < UP_PHI_STD and sstd(m[:,1]) < MID_PHI_STD:
		if True:
			# if i in show and printed:
			# 	print('bingo', i)
			# print(g[i, 2])
			detected.append([ max(i-UP_WIN-MID_WIN-10, 0), min(i+20, n) ])
			i += UP_WIN + MID_WIN
		i += 1
	return detected

gazes = pickle.load(open('study 1/peruser.pkl', 'rb'))
for i in range(len(gazes)):
	el = pickle.load(open('study 1/' + str(i+1) + '/el990.pkl', 'rb'))
	el = [el[0][0], el[0][1], el[1], el[2]]
	gaze = np.array(gazes[i])
	detected = detect3(gaze, el)
	print('user:' + str(i) + ' ' + str(len(detected)))