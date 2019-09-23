import random

toys = ['Flask_11', 'Instrument_Tray_12', 'Brain_Jar_13', 'Notebook_14', 'Pipette_15', 'Trash_Can_16',
	'Magic_Book_21', 'Cristal_Flask_22', 'Candle_23', 'Mortar_24', 'AirTotem_25', 'EarthTotem_26',
	'Ball_31', 'Chair_32', 'Speaker_33', 'CellPhone_34', 'House_Plant_35', 'Laptop_36']

a = list(range(0, len(toys)))
random.shuffle(a)
f = open('order.txt', 'w')
for i in range(len(a)):
	f.write(toys[a[i]] + '\n')
f.close()