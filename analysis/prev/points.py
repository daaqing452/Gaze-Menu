import matplotlib.pyplot as plt
import math
import numpy as np

# files = ['comfortable/20190624105607_1_comfortable.csv', 'comfortable/20190624105625_2_comfortable.csv',
#         'comfortable/20190624105639_3_comfortable.csv', 'comfortable/20190624105652_4_comfortable.csv',
#         'comfortable/20190624105705_5_comfortable.csv', 'comfortable/20190624105717_6_comfortable.csv',
#         'comfortable/20190624105729_7_comfortable.csv', 'comfortable/20190624105742_8_comfortable.csv',]

edge_files = ['edge/%d_edge.csv'% i for i in range(1, 9)]

cut_front = 50
cut_back = 50
confidence_threshold = 0.6
cut = False

avg_eyes = []


for filename in edge_files:
  eye_0_timestamp = []
  eye_1_timestamp = []
  eye_0_pos_2d = [[],[]]
  eye_1_pos_2d = [[],[]]
  eye_timestamp = []
  eye_pos_2d = [[], []]

  cur_eye_0 = [-1, -1]
  cur_eye_1 = [-1, -1]
  with open(filename, 'r') as f:
    line = f.readline()
    for line in f:
      data = line.strip().split(',')
      timestamp = float(data[0])
      eye_id = int(data[1])
      confidence = float(data[2])
      x = float(data[3])
      y = float(data[4])
      if confidence > confidence_threshold:
        if eye_id == 0:
          eye_0_pos_2d[0].append(x)
          eye_0_pos_2d[1].append(y)
          eye_0_timestamp.append(timestamp)
          cur_eye_0[0] = x
          cur_eye_0[1] = y
          if cur_eye_1[0] != -1 and cur_eye_1[1] != -1:
            eye_timestamp.append(timestamp)
            eye_pos_2d[0].append((cur_eye_0[0] + cur_eye_1[0])/2)
            eye_pos_2d[1].append((cur_eye_0[1] + cur_eye_1[1])/2)
        else:
          eye_1_pos_2d[0].append(x)
          eye_1_pos_2d[1].append(y)
          eye_1_timestamp.append(timestamp)
          cur_eye_1[0] = x
          cur_eye_1[1] = y
          if cur_eye_0[0] != -1 and cur_eye_0[1] != -1:
            eye_timestamp.append(timestamp)
            eye_pos_2d[0].append((cur_eye_0[0] + cur_eye_1[0])/2)
            eye_pos_2d[1].append((cur_eye_0[1] + cur_eye_1[1])/2)

  if cut:
    eye_0_pos_2d[0] = eye_0_pos_2d[0][cut_front:-cut_back]
    eye_0_pos_2d[1] = eye_0_pos_2d[1][cut_front:-cut_back]
    eye_0_timestamp = eye_0_timestamp[cut_front:-cut_back]
    eye_1_pos_2d[0] = eye_1_pos_2d[0][cut_front:-cut_back]
    eye_1_pos_2d[1] = eye_1_pos_2d[1][cut_front:-cut_back]
    eye_1_timestamp = eye_1_timestamp[cut_front:-cut_back]
    eye_pos_2d[0] = eye_pos_2d[0][cut_front:-cut_back]
    eye_pos_2d[1] = eye_pos_2d[1][cut_front:-cut_back]
    eye_timestamp = eye_timestamp[cut_front:-cut_back]

  avg_eyes.append(eye_pos_2d)

  eye_pos_x = np.array(eye_0_pos_2d[0], dtype=np.float32)
  eye_pos_y = np.array(eye_0_pos_2d[1], dtype=np.float32)
  plt.gca().set_xlim([0.0, 1.0])
  plt.gca().set_ylim([0.0, 1.0])
  plt.scatter(eye_pos_x, eye_pos_y)
  plt.savefig(filename+ '_eye_0.png')
  plt.clf()
  plt.cla()
  eye_pos_x = np.array(eye_1_pos_2d[0], dtype=np.float32)
  eye_pos_y = np.array(eye_1_pos_2d[1], dtype=np.float32)
  plt.gca().set_xlim([0.0, 1.0])
  plt.gca().set_ylim([0.0, 1.0])
  plt.scatter(eye_pos_x, eye_pos_y)
  plt.savefig(filename+ '_eye_1.png')
  plt.clf()
  plt.cla()
  eye_pos_x = np.array(eye_pos_2d[0], dtype=np.float32)
  eye_pos_y = np.array(eye_pos_2d[1], dtype=np.float32)
  plt.gca().set_xlim([0.0, 1.0])
  plt.gca().set_ylim([0.0, 1.0])
  plt.scatter(eye_pos_x, eye_pos_y)
  plt.savefig(filename+ '_avg.png')
  plt.clf()
  plt.cla()
  plt.gca().set_xlim([0.0, 1.0])
  plt.gca().set_ylim([0.0, 1.0])
  eye_pos_x = np.array(eye_0_pos_2d[0], dtype=np.float32)
  eye_pos_y = np.array(eye_0_pos_2d[1], dtype=np.float32)
  plt.scatter(eye_pos_x, eye_pos_y)
  eye_pos_x = np.array(eye_1_pos_2d[0], dtype=np.float32)
  eye_pos_y = np.array(eye_1_pos_2d[1], dtype=np.float32)
  plt.scatter(eye_pos_x, eye_pos_y)
  plt.savefig(filename+ '_both_eyes.png')
  plt.clf()
  plt.cla()

  timestamp_0 = np.array(eye_0_timestamp, dtype=np.float32)
  eye_0 = np.array(eye_0_pos_2d[0], dtype=np.float32)
  timestamp_1 = np.array(eye_1_timestamp, dtype=np.float32)
  eye_1 = np.array(eye_1_pos_2d[0], dtype=np.float32)
  plt.plot(timestamp_0, eye_0, label='eye_0')
  plt.plot(timestamp_1, eye_1, label='eye_1')
  plt.legend()
  plt.savefig(filename+ 'time_x.png')
  plt.clf()
  plt.cla()

  timestamp_0 = np.array(eye_0_timestamp, dtype=np.float32)
  eye_0 = np.array(eye_0_pos_2d[1], dtype=np.float32)
  timestamp_1 = np.array(eye_1_timestamp, dtype=np.float32)
  eye_1 = np.array(eye_1_pos_2d[1], dtype=np.float32)
  plt.plot(timestamp_0, eye_0, label='eye_0')
  plt.plot(timestamp_1, eye_1, label='eye_1')
  plt.legend()
  plt.savefig(filename+ 'time_y.png')
  plt.clf()
  plt.cla()

plt.gca().set_xlim([0.0, 1.0])
plt.gca().set_ylim([0.0, 1.0])
cnt = 0
for eye_pos_2d in avg_eyes:
  cnt += 1
  eye_pos_x = np.array(eye_pos_2d[0], dtype=np.float32)
  eye_pos_y = np.array(eye_pos_2d[1], dtype=np.float32)
  plt.gca().set_xlim([0.0, 1.0])
  plt.gca().set_ylim([0.0, 1.0])
  plt.scatter(eye_pos_x, eye_pos_y, label=str(cnt))

plt.legend()
plt.savefig('edge.png')
plt.clf()
plt.cla()
