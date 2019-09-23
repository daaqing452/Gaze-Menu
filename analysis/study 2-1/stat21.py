import numpy as np
import matplotlib.pyplot as plt
import pickle
import os
import math
import matplotlib.path as path
import matplotlib.patches as patches

def read_study2():
	bs = ['1-1', '1-2', '1-3']
	minus_one = False
	blink_cnt = 0
	selections = []
	for i in range(len(bs)):
		selection = []
		ddir = bs[i] + '/'
		filenames = os.listdir(ddir)
		cnt = 0
		for filename in filenames:
			cnt += 1
			# if cnt != 2: continue
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
				if z < 0 or length < 1e-5: continue
				minus_one = False
				x /= length
				y /= length
				z /= length

				theta = math.acos(z) * 180 / math.pi
				phi = math.atan2(y, x) * 180 / math.pi
				selection.append([l, theta, phi])
			f.close()
		selections.append(np.array(selection))
	return selections

def smooth(b):
	c = []
	n = len(b)
	for i in range(n):
		p0 = b[i]
		p1 = b[i-1] if i > 0   else b[n-1]
		p2 = b[i+1] if i < n-1 else b[  0]
		c.append((p0+p1+p2)/3)
	c.append(c[0])
	c = np.array(c)
	return c

study2 = read_study2()
N_BINS = 36
BIN = 360 / N_BINS
phierr = []
polar = [[] for i in range(N_BINS)]
for i in range(0, 3):
	# phierr = []
	# polar = [[] for i in range(N_BINS)]
	a = study2[i]
	n = a.shape[0]
	l = 0
	while l < n:
		r = l+1
		while r < n and a[l, 0] == a[r, 0]: r += 1
		idx = l
		for j in range(l, r):
			if a[j, 1] > a[idx, 1]:
				idx = j
		pe = (a[idx, 2] - a[idx, 0]) % 360
		if pe > 180: pe -= 360
		if pe < -180: pe += 360
		if pe > 30 or pe < -30:
			pass
		else:
			phierr.append(pe)
			polar[int(a[idx, 0] / BIN)].append(pe)
		l = r

print(len(phierr))
zs = np.array(phierr)
n, bins = np.histogram(zs, 50)
left = np.array(bins[:-1])
right = np.array(bins[1:])
bottom = np.zeros((left.shape[0]))
top = bottom + n
XY = np.array([[left, left, right, right], [bottom, top, top, bottom]]).T
barpath = path.Path.make_compound_path_from_polys(XY)
patch = patches.PathPatch(barpath)
patch.set_facecolor('#aaffaa')
fig, ax = plt.subplots()
ax.add_patch(patch)
ax.set_xlim(left[0], right[-1])
ax.set_ylim(bottom.min(), top.max() * 1.1)
ax.set_yticks([])
ax.set_xlim(-30, 30)
ax.set_xticklabels(['-30°', '-20°', '-10°', '0', '10°', '20°', '30°'])
pctl = np.percentile(zs, (0.5, 2.5, 5, 95, 97.5, 99.5))
print(pctl)

# for i in range(N_BINS):
# 	print(polar[i])
# 	print(np.percentile(polar[i], (25, 50, 75)))

plt.figure()
ax = plt.subplot(111, projection='polar')
ppp = np.array([np.percentile(p, (25, 50, 75)) for p in polar])
ppm = np.array([np.mean(p) for p in polar])
pps = np.array([np.std(p) for p in polar])
ax.set_ylim(-8, 8)
x = np.arange(BIN / 2 / 180 * math.pi, 2 * math.pi + BIN / 180 * math.pi, BIN / 180 * math.pi)
ax.set_yticklabels(['-6°', '-4°', '-2°', '0', '2°', '4°', '6°', '8°'], size=9)
ax.plot(x, smooth(ppm))
ax.plot(x, smooth(pps))

plt.show()
