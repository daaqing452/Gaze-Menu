import numpy as np
import matplotlib.pyplot as plt
import os
import sys
import math

errs = {'4': [], '6':[], '8':[], '12':[], '16':[]}
tims = {'4': [], '6':[], '8':[], '12':[], '16':[]}

for u in range(1,11):
	basedir = '2-' + str(u) + '/'
	filenames = os.listdir(basedir)
	for filename in filenames:
		if filename == '.DS_Store': continue;
		if filename[:2] == 'No': continue
		rtype = filename.split('-')[1]
		prev_op = 'out'
		f = open(basedir + filename)
		tim_u = []
		while True:
			line = f.readline()
			if len(line) == 0: break
			arr = line.split(' ')
			op = arr[0]
			t = float(arr[1])
			if op == 'finished':
				err = int(arr[6])
				break
			if op == 'in' and prev_op == 'out':
				start_t = t
				err = int(arr[6])
			if op == 'out' and prev_op == 'in':
				tim_u.append( t - start_t )
				err = int(arr[6])
			prev_op = op
		f.close()
		errs[rtype].append(err)
		tims[rtype].append(tim_u)

qm = []
qs = []
tims_raw = tims
f = open('study22.csv', 'w')
f.write('name,res,err,time,throughput\n')
for rtype in errs:
	err = np.array(errs[rtype])
	tim = tims[rtype]
	thr = np.zeros(err.shape[0])
	for i in range(err.shape[0]):
		thr[i] = math.log(int(rtype)) / math.log(2) * (1 - err[i] / 24) / np.array(tim[i]).mean()
	tflat = np.array([t1 for t0 in tim for t1 in t0])
	tim = np.array([np.array(t0).mean() for t0 in tim])
	throughput = math.log(int(rtype)) / math.log(2) * (1 - err.mean() / 24) / tflat.mean()
	print(rtype, err.mean() / 24, tim.mean(), thr.mean())
	qm.append([err.mean() / 24 * 100, tim.mean(), thr.mean()])
	qs.append([err.std() / 24 * 100, tim.std(), thr.std()])
	for i in range(err.shape[0]):
		f.write(str(i)+','+rtype+','+str(err[i]/24*100)+','+str(tim[i])+','+str(thr[i])+'\n')
f.close()

if True:
	qm = np.array(qm)
	qs = np.array(qs)
	x = np.arange(0, 5)

	ax0 = plt.subplot(311)
	ax0.set_xticks([])
	ax0.set_ylabel('Accuracy', size=12)
	ax0.yaxis.tick_right()
	ax0.bar(x, qm[:, 0], yerr=qs[:, 0])
	ax0.grid(True)

	ax1 = plt.subplot(312)
	ax1.set_xticks([])
	ax1.set_ylabel('Time', size=12)
	ax1.yaxis.tick_right()
	ax1.bar(x, qm[:, 1], yerr=qs[:, 1])
	ax1.grid(True)

	ax2 = plt.subplot(313)
	ax2.yaxis.tick_right()
	ax2.yaxis.grid(True)
	ax2.set_ylabel('Throughput', size=12)
	ax2.set_xticklabels(['x', '4', '6', '8', '12', '16'], size=15)
	ax2.bar(x, qm[:, 2], yerr=qs[:, 2])

	plt.show()

if False:
	n_b = 6
	ax = plt.subplot()
	for rtype in errs:
		tim = tims_raw[rtype]
		b = [[] for i in range(n_b)]
		for ti in tim:
			n = len(ti)
			for i in range(n_b):
				l = int(n/n_b*i)
				r = int(n/n_b*(i+1))
				b[i].extend(ti[l:r])
		b = [np.array(bi).mean() for bi in b]
		ax.plot(b)
	plt.show()