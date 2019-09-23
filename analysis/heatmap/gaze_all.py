import json
import math
import os
import numpy as np
import pickle
import matplotlib.pyplot as plt
import matplotlib.patches as patches
import matplotlib.path as path

gaze = []
gsum = gshow = 0
zs = []
polar = []

polar_bins = 36
polar_width = 360 / polar_bins

for i in range(polar_bins):
	polar.append([])

for i in range(1, 13):
	ddir = '../study 1/' + str(i) + '/'
	filenames = os.listdir(ddir)
	for filename in filenames:
		if filename == '.DS_Store':
			continue
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

			if z < 0: continue
			length = math.sqrt(x*x+y*y+z*z)
			if length < 1e-5: continue
			gsum += 1
			x /= length
			y /= length
			z /= length

			theta = math.acos(z) * 180 / math.pi
			if theta > 50: continue
			gshow += 1

			phi = math.atan2(y, x) * 180 / math.pi
			# dis = math.sqrt(x*x+y*y)
			dis = math.acos(z) * 180 / math.pi
			x = int(x*100)/100
			y = int(y*100)/100
			gaze.append((x, y))
			zs.append(theta)
			
			if phi < 0: phi += 360
			polar[int(phi // polar_width)].append(dis)
		f.close()
		# break
	# break

jgaze = json.dumps(gaze)
f = open('heatmap/gaze.json', 'w')
f.write(jgaze)
f.close()

if False:
	gaze = np.array(gaze)
	print(gshow, gsum, 1.0 * gshow / gsum)
	print('x:', gaze[:, 0].min(), gaze[:, 0].max())
	print('y:', gaze[:, 1].min(), gaze[:, 1].max())
	plt.figure()
	plt.scatter(gaze[:, 0], gaze[:, 1])

if True:
	zs = np.array(zs)
	print('z:', zs.min(), zs.max())
	n, bins = np.histogram(zs, 50)
	left = np.array(bins[:-1])
	right = np.array(bins[1:])
	bottom = np.zeros((left.shape[0]))
	top = bottom + n
	XY = np.array([[left, left, right, right], [bottom, top, top, bottom]]).T
	barpath = path.Path.make_compound_path_from_polys(XY)
	patch = patches.PathPatch(barpath)
	patch.set_facecolor('#aaffaa')
	fig, ax = plt.subplots(1, 2)
	ax[0].add_patch(patch)
	ax[0].set_xlim(left[0], right[-1])
	ax[0].set_ylim(bottom.min(), top.max() * 1.1)
	ax[0].set_yticks([])
	ax[0].set_xticklabels(['0°', '10°', '20°', '30°', '40°', '50°'])
	zs.sort()
	print(len(np.nonzero(zs <= 35)[0]) / zs.shape[0])
	print(len(np.nonzero(zs <= 40)[0]) / zs.shape[0])
	x = np.arange(0, 1, 1 / zs.shape[0])
	ax[1].plot(zs, x)
	ax[1].set_xticklabels(['xx', '0°', '10°', '20°', '30°', '40°', '50°'])
	# plt.hist(zs, bins=50, density=True)

def smooth(b):
	c = []
	n = len(b)
	for i in range(n):
		p0 = b[i][1]
		p1 = b[i-1][1] if i > 0   else b[n-1][1]
		p2 = b[i+1][1] if i < n-1 else b[  0][1]
		c.append([b[i][0], (p0+p1+p2)/3])
	c.append(c[0])
	c = np.array(c)
	return c

if True:
	b500 = []
	b900 = []
	b990 = []
	b997 = []
	b999 = []
	for i in range(polar_bins):
		polar[i] = np.array(polar[i])
		polar[i].sort()
		length = polar[i].shape[0]
		b500.append([(i + 0.5) * polar_width / 180 * math.pi, polar[i][int(length * 0.500)]])
		b900.append([(i + 0.5) * polar_width / 180 * math.pi, polar[i][int(length * 0.900)]])
		b990.append([(i + 0.5) * polar_width / 180 * math.pi, polar[i][int(length * 0.990)]])
		b997.append([(i + 0.5) * polar_width / 180 * math.pi, polar[i][int(length * 0.997)]])
		b999.append([(i + 0.5) * polar_width / 180 * math.pi, polar[i][int(length * 0.999)]])
	b500 = smooth(b500)
	b900 = smooth(b900)
	b990 = smooth(b990)
	b997 = smooth(b997)
	b999 = smooth(b999)
	print('99.0%: ', b990[polar_bins//4*0][1], b990[polar_bins//4*1][1], b990[polar_bins//4*2][1], b990[polar_bins//4*3][1])
	print('99.7%: ', b997[polar_bins//4*0][1], b997[polar_bins//4*1][1], b997[polar_bins//4*2][1], b997[polar_bins//4*3][1])
	print('99.9%: ', b999[polar_bins//4*0][1], b999[polar_bins//4*1][1], b999[polar_bins//4*2][1], b999[polar_bins//4*3][1])
	plt.figure()
	ax = plt.subplot(111, projection='polar')
	ax.set_yticklabels(['10°', '20°', '30°', '40°', '50°'], {'fontsize': 10})
	a0, = ax.plot(b500[:,0], b500[:,1])
	# a1, = ax.plot(b900[:,0], b900[:,1])
	a2, = ax.plot(b990[:,0], b990[:,1])
	a3, = ax.plot(b997[:,0], b997[:,1])
	a4, = ax.plot(b999[:,0], b999[:,1])
	ax.legend([a0, a2, a3, a4], ['50.0%', '99.0%', '99.7%', '99.9%'], loc=(0.95, 0.9))

# pickle.dump( np.array([np.array(b990)[:-1, 1], np.array(b997)[:-1, 1], np.array(b999)[:-1, 1]]), open('study 1 border.pkl', 'wb') )
plt.show()