import json
import math
import os
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.patches as patches
import matplotlib.path as path

polar_bins = 12
polar_width = 360 / polar_bins

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

g = open('gaze_per.csv', 'w')

for u in range(1, 13):
	gaze = []
	zs = []
	polar = []
	for i in range(polar_bins):
		polar.append([])

	ddir = '../study 1/' + str(u) + '/'
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
			x /= length
			y /= length
			z /= length

			theta = math.acos(z) * 180 / math.pi
			if theta > 50: continue
			phi = math.atan2(y, x) * 180 / math.pi
			if phi < 0: phi += 360
			polar[int(phi // polar_width)].append(theta)
		f.close()

	b990 = []
	b997 = []
	b999 = []
	for i in range(polar_bins):
		polar[i] = np.array(polar[i])
		polar[i].sort()
		length = polar[i].shape[0]
		b990.append([(i + 0.5) * polar_width / 180 * math.pi, polar[i][int(length * 0.990)]])
		b997.append([(i + 0.5) * polar_width / 180 * math.pi, polar[i][int(length * 0.997)]])
		b999.append([(i + 0.5) * polar_width / 180 * math.pi, polar[i][int(length * 0.999)]])
	b990 = smooth(b990)
	b997 = smooth(b997)
	b999 = smooth(b999)

	g.write(str(u))
	for i in [2, 0, 1, 3]:
		g.write(',' + str(b990[polar_bins//4*i][1]))
	for i in [2, 0, 1, 3]:
		g.write(',' + str(b997[polar_bins//4*i][1]))
	for i in [2, 0, 1, 3]:
		g.write(',' + str(b999[polar_bins//4*i][1]))
	g.write('\n')

g.close()