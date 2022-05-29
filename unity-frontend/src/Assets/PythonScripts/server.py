import socket
import json
import random
import time


def move_one_step(animalInfo, max_x, max_z):
    for i, info in enumerate(animalInfo.values()):
        pos = info['pos']
        pos['x'] = min(max_x, pos['x'] + 1 / 40.0)
        pos['z'] = min(max_z, pos['z'] + i / 40.0)


ELEPHANT_NAMES = []
LION_NAMES = ["Simba", "Nala",]
ZEBRA_NAMES = ["Joe", "Emily",]

if __name__ == "__main__":
    HOST = '127.0.0.1'
    PORT = 8080
    START = b'START'
    NEXT = b'NEXT'

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind((HOST, PORT))
    print("Server initiating")
    ready = 0
    timer = 0
    eat_trigger = False
    eat_timer = time.time()
    initial_positions = []

    # Max map size
    max_x = 0
    max_z = 0

    animalInfo = {}
    while True:
        request, address = sock.recvfrom(1024)
        if request == START:
            sock.sendto(START, address)
            request, address = sock.recvfrom(1024)
            config = json.loads(request)
            max_x = config['map'][0][0]
            max_z = config['map'][0][1]

            # Type is based on the AnimalType.cs enum
            # TODO: Figure out how have one file for animal types
            # TODO: Better way to init animal info
            initialAnimalInfo = []
            count = -1
            for name in ELEPHANT_NAMES:
                count += 1
                position_x = 10 * count
                position_z = 90 * count
                position = {"x": position_x, "z": position_z}
                info = {"id": count, "name": name, "type": 0, "state": 0, "pos": position}
                initialAnimalInfo.append(info)
                animalInfo[count] = info

            for name in LION_NAMES:
                count += 1
                position_x = 10 * count
                position_z = 90 * count
                position = {"x": position_x, "z": position_z}
                info = {"id": count, "name": name, "type": 1, "state": 0, "pos": position}
                initialAnimalInfo.append(info)
                animalInfo[count] = info

            for name in ZEBRA_NAMES:
                count += 1
                position_x = 10 * count
                position_z = 90 * count
                position = {"x": position_x, "z": position_z}
                info = {"id": count, "name": name, "type": 2, "state": 0, "pos": position}
                initialAnimalInfo.append(info)
                animalInfo[count] = info

            print(f'Created {len(ELEPHANT_NAMES)} elephants, {len(LION_NAMES)} lions, {len(ZEBRA_NAMES)} zebras')

            rain_locations = [{"x": 50, "z": 50, "radius": 10, "intensity": 1.0},
                              {"x": 20, "z": 20, "radius": 5, "intensity": 1.0}]
            update = {"animals": initialAnimalInfo, "rainLocations": rain_locations}
            sock.sendto(json.dumps(update, sort_keys=True, indent=4).encode(), address)
        elif request == NEXT:
            if not eat_trigger:
                if random.random() < 0.001:
                    eat_trigger = True
                    eat_timer = time.time()
                else:
                    move_one_step(animalInfo, max_x, max_z)
            else:
                if time.time() - eat_timer > 5:
                    eat_trigger = False

            for info in animalInfo.values():
                if info['state'] != 2:
                    info['state'] = int(eat_trigger)
                    if random.random() < 0:
                        info['state'] = 2

            update = {"animals": [x for x in animalInfo.values()]}

            sock.sendto(json.dumps(update, sort_keys=True, indent=4).encode(), address)
