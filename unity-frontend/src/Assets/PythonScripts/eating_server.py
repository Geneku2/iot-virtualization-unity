import socket
import json
import random
import signal, os
import numpy as np
import time

def handler(signum, frame):
	print('signal %d catch' % signum)
	quit


def MoveOneStepMultiple(initial_positions, max_x, max_y, obstacles):
	new_positions = []
	for i in range(len(initial_positions)):
		x, y = initial_positions[i]
		# new_x = max(0, min(max_x, x + random.uniform(-1, 2)/40))
		#new_y = max(0, min(max_y , y + random.uniform(1, 2)/20))
		#new_x = max(0, min(max_y , y + random.uniform(1, 2)/20))
		new_x = min (max_x , x + 1 / 40.0)
		new_y = min (max_y, y + i / 40.0)
		new_positions.append([new_x , new_y])

	return new_positions

def going_in_angle(initial_positions, angles):
	new_positions = []
	for i in range(len(initial_positions)):
		x, y = initial_positions[i]
		dx = np.cos(angles[i]) * ((i+1)**0.5 / 25)
		dy = np.sin(angles[i]) * ((i+1)**0.5 / 25)
		new_positions.append([x+dx, y+dy])
		if random.random() < 0.01:
			da = random.uniform(-np.pi/3, np.pi/3)
			angles[i] += da
	return new_positions


if __name__ == "__main__":
	HOST = '127.0.0.1'
	PORT = 8080
	# initial_positions = [[2,2], [4, 4], [3, 3], [4, 4], [5, 5], [6, 6], [7, 7], [8, 8], [9, 9], [10, 10]] #10
	# initial_positions = [[4*i,4*i] for i in range(1,11)]

	reply = b'Start'

	s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
	s.bind((HOST, PORT))
	print("server initiating")
	ready = 0
	timer = 0
	eat_trigger = False
	eat_timer = time.time()
	initial_positions = []

	while True:
		request, address = s.recvfrom(1024)
		if request == b"START":
			s.sendto(b"Ready", address)
			request, address = s.recvfrom(1024)
			config = json.loads(request)
			ele_cnt = config["animalConfig"][0]
			lion_cnt = config["animalConfig"][1]
			zebra_cnt = config["animalConfig"][2]
			initial_positions = [[90*(i+1),90*(i+1)] for i in range(1, ele_cnt + lion_cnt + zebra_cnt + 1)]
			angles = [np.pi/2 for i in range(1, ele_cnt + lion_cnt + zebra_cnt + 1)]
			print(f"created {ele_cnt} elephants, {lion_cnt} lions, {zebra_cnt} zebras")

			initialArr = []
			
			for i in range(len(initial_positions)):
				if(i<ele_cnt):
					initialArr.append({"id":i, "type":0, "state":0, "position":{"x": initial_positions[i][0], "z": initial_positions[i][1]}, "velocity":{}})
				elif(i<lion_cnt+ele_cnt):
					initialArr.append({"id":i, "type":1, "state":0, "position":{"x": initial_positions[i][0], "z": initial_positions[i][1]}, "velocity":{}})
				else:
					initialArr.append({"id":i, "type":2, "state":0, "position":{"x": initial_positions[i][0], "z": initial_positions[i][1]}, "velocity":{}})

			weather = [{"type":0, "center":{"x":50, "z":50}, "radius":10},{"type":0, "center":{"x":20, "z":20}, "radius":5}]
			update = {"animals":initialArr, "weather": weather}
			s.sendto(json.dumps(update, sort_keys=True, indent=4).encode(), address)

		# print('address is: ', address)
		# if (ready == 0):
		# 	n = s.sendto(reply, address)
		# 	ready = 1
		else:
			#update = json.dumps({"position1":5,"position2":7}, sort_keys=True)
			if not eat_trigger:
				if random.random() < 0.01:
					eat_trigger = True
					eat_timer = time.time()
			else:
				if (time.time()- eat_timer > 5):
					eat_trigger = False
			
			if (timer == 0):
				pos = MoveOneStepMultiple(initial_positions, 20000, 20000, [])
				timer = 1
				print("initial position sent")
			else:
				# pos = MoveOneStepMultiple(pos, 2000, 2000, [])
				if not eat_trigger:
					pos = MoveOneStepMultiple(pos, 20000, 20000, [])
					# pos = going_in_angle(pos, angles)
			# return_pos = []
			
			animalArr = []
			for i in range(len(pos)):
				if(i<ele_cnt):
					animalArr.append({"id":i, "type":0, "state":int(eat_trigger), "position":{"x": pos[i][0], "z": pos[i][1]}, "velocity":{}})
				elif(i<lion_cnt+ele_cnt):
					animalArr.append({"id":i, "type":1, "state":int(eat_trigger), "position":{"x": pos[i][0], "z": pos[i][1]}, "velocity":{}})
				else:
					animalArr.append({"id":i, "type":2, "state":int(eat_trigger), "position":{"x": pos[i][0], "z": pos[i][1]}, "velocity":{}})
				# return_pos.append({"x": pos[i][0], "z": pos[i][1]})
			# print(animalArr)

			weather = [{"type":0, "center":{"x":50, "z":50}, "radius":10},{"type":0, "center":{"x":20, "z":20}, "radius":5}]
			update = {"animals":animalArr, "animalTypes":[ele_cnt,lion_cnt,zebra_cnt]}



			# return_aniTypes = [5,3,2] #elephant, lion, zebra
			# update = {"animalTypes":return_aniTypes,"positions": return_pos}
			s.sendto(json.dumps(update, sort_keys=True, indent=4).encode(), address)
