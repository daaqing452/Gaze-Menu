import numpy as np
import matplotlib.pyplot as plt

m = np.array([[2.5, 3.625], [3.125, 3.375], [2.875, 4], [2.625, 4.25]])
s = np.array([[0.53, 1.06], [0.83, 1.3], [0.83, 0.53], [0.52, 0.89]])

w = 0.3
ax = plt.subplot(111)
x = np.arange(4)
b0 = ax.bar(x+w*0, m[:,1], yerr=s[:,1], width=w)
b1 = ax.bar(x+w*1, m[:,0], yerr=s[:,0], width=w)
ax.legend((b0, b1), ['GazeDock', 'Dwelling'], loc='lower right', fontsize=14)
ax.set_xticks(x+w*0.5)
ax.set_xticklabels(['Effort', 'Fatigue', 'Performance', 'Preference'], size=12)
plt.show()