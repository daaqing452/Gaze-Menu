import numpy as np
import matplotlib.pyplot as plt
from matplotlib.patches import Ellipse
import math
import os
import pickle
from scipy import optimize
import pylab
import ellipses as el


def read_study1():
	gazes = []
	minus_one = False
	for i in range(1, 13):
		ddir = '../study 1/' + str(i) + '/'
		filenames = os.listdir(ddir)
		gaze = []
		for filename in filenames:
			if filename[:2] == 'el': continue
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
						gaze.append([-1, -1, -1, -1])
						minus_one = True
					continue
				minus_one = False
				x /= length
				y /= length
				z /= length

				theta = math.acos(z) * 180 / math.pi
				phi = math.atan2(y, x) * 180 / math.pi
				gaze.append([x, y, theta, phi])
			f.close()
		gazes.append(gaze)
	pickle.dump(gazes, open('../study 1/peruser.pkl', 'wb'))

def smooth(a):
	n = len(a)
	c = []
	for i in range(0, n):
		p0 = a[i-1] if i > 0   else a[-1]
		p1 = a[i]
		p2 = a[i+1] if i < n-1 else a[0]
		c.append((p0 + p1 + p2) / 3)
	return c

def get_ellipse(u, b, l, r, edgecolor):
	data = []
	for i in range(l, r):
		phi = (i+0.5) * BIN / 180 * math.pi
		r = b[i]
		x = r * math.cos(phi)
		y = r * math.sin(phi)
		data.append((x, y))
	data = np.array(data).T

	lsqe = el.LSqEllipse()
	lsqe.fit(data)
	center, width, height, phi = lsqe.parameters()
	pickle.dump((center, width, height), open('../study 1/' + str(u+1) + '/el990.pkl', 'wb'))

	# f = open('tmp.csv', 'a')
	# f.write(str(center[0]) + ',' + str(center[1]) + ',' + str(width) + ',' + str(height) + '\n')
	# f.close()
	
	ellipse = Ellipse(xy=center, width=2*width, height=2*height, angle=np.rad2deg(phi),
               edgecolor=edgecolor, 
               fc='None', lw=4, label='Fit', zorder = 2)
	return ellipse, data

def geterr(data, c, w, h):
	data = data.T
	n = data.shape[0]
	for i in range(n):
		pass

# read_study1()

gazes = pickle.load(open('../study 1/peruser.pkl', 'rb'))
gazes.append([])
for i in range(12):
	gazes[-1].extend(gazes[i])

N_BIN = 36
BIN = 360 // N_BIN
bs = []
es = []
edgecolor = ['#1f77b4', '#ff7f0e', '#2ca02c', '#d62728', '#9467bd', '#8c564b',
	'#e377c2', '#7f7f7f', '#bcbd22', '#17becf', 'b', 'k', 'r']
for i in range(13):
	print('user: ', i)
	gaze = gazes[i]
	bin = [[] for i in range(N_BIN)]
	for j in range(len(gaze)):
		if gaze[j][2] < 0: continue
		if gaze[j][2] > 50: continue
		bin[int(gaze[j][3] / BIN)].append(gaze[j][2])
	b = []
	for j in range(N_BIN):
		bin[j].sort()
		if len(bin[j]) == 0:
			b.append(b[-1])
		else:
			b.append(bin[j][int(len(bin[j])*0.990)])
	b = smooth(b)
	bs.append(b)

	e, data = get_ellipse(i, b, 0, len(b), edgecolor[i])
	es.append(e)

	if i == 12:
		fig = plt.figure(figsize=(6,6))
		ax = fig.add_subplot(111)
		print(data.shape, data[:, 0].shape)
		data = np.concatenate([data, data[:, 0].reshape(2,1)], axis=1)
		print(data.shape)
		ax.plot(data[0], data[1], linewidth=4)
		ax.add_patch(e)
		# plt.show()

plt.figure()
ax = plt.subplot(111, projection='polar')
ax.set_yticklabels(['10°', '20°', '30°', '40°', '50°'], {'fontsize': 10})
ax.set_ylim(0, 50)
for b in bs[-1:]:
	b.append(b[0])
	c = np.array(range(BIN//2, 360+BIN, BIN)) / 180 * math.pi
	ax.plot(c, b)

# fig = plt.figure(figsize=(6,6))
# ax = fig.add_subplot(111)
# for e in es[:-1]:
# 	ax.add_patch(e)
# ax.plot((0),(0), 'bo')

plt.show()