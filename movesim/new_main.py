#
# Copyright (c) University of Illinois and Bangladesh University of Engineering and Technology. All rights reserved.
# Unauthorized copying of this file, via any medium is strictly prohibited
# Proprietary and confidential. No warranties, express or implied.
#
import os
import argparse
import asyncio
import json

from Simulator import *
from Map_generator import *
from animal import *

def formatAnimalType(type):
  if type == 'lion':
    return 'Lion'
  elif type == 'zebra':
    return 'Zebra'
  elif type == 'elephant':
    return 'Elephant'
  else:
    return type

class Protocol(asyncio.Protocol):
    def __init__(self):
        print('initialized')
  
    def connection_made(self, transport):
        
        peername = transport.get_extra_info('peername')
        print('Connection from {}'.format(peername))
        self.transport = transport
        
        parser = argparse.ArgumentParser()
        parser.add_argument("--time_step", type=int, default=1000, help="length of simulation")
        parser.add_argument("--filename", type=str, default="simulation.gif", help="the name of the animation gif")
        
        opt = parser.parse_args()
        if not os.path.exists("output"):
          os.mkdir("output")

        dimensions = [1000, 1000]
        mountains = []
        lakes = [[300, 300, 200], [50, 100, 40], [600, 300, 40], [800, 700, 80], [387, 892, 80], [862, 651, 160]]

        new_map = Map(dimensions, mountains, lakes)
        lion = Lion(np.random.randint(0, 1000, 2), 0, new_map)
        lion1 = Lion(np.random.randint(0, 1000, 2), 3, new_map)
        zebra1 = Zebra(np.random.randint(0, 1000, 2), 1, new_map)
        zebra2 = Zebra(np.random.randint(0, 1000, 2), 2, new_map)
        all_animal = [lion, lion1, zebra1, zebra2]
        time_step = opt.time_step
        sim = Simulator(new_map, all_animal, time_step)

        self.sim = sim


    def data_received(self, data):
        print('data received:', data.decode('utf-8'))

        data = self.sim.get_json
        dataFromSim = json.loads(data)
        formattedData = []
        i = 0
        size = len(dataFromSim)
        while i < size:
          item = {}
          
          item['id'] = dataFromSim[i]['id']
          item['locationx'] = dataFromSim[i]['location'][0]
          item['locationy'] = dataFromSim[i]['location'][1]
          item['type'] = formatAnimalType(dataFromSim[i]['type'])
          item['action'] = dataFromSim[i]['action']
          item['vision'] = dataFromSim[i]['vision']
          item['health'] = dataFromSim[i]['health']
          item['state'] = dataFromSim[i]['state']
          item['isDead'] = dataFromSim[i]['isDead']
          item['respawn_time'] = dataFromSim[i]['respawn_time']
          
          formattedData.append(item)
          i += 1

        sendAbleJson = json.dumps({"animalhugeinfos": formattedData})
        self.transport.write(sendAbleJson.encode())

        
    def connection_lost(self, exc):
        print('stop', exc)

async def main(host, port):
    loop = asyncio.get_running_loop()
    server = await loop.create_server(Protocol, host, port)
    await server.serve_forever()

asyncio.run(main('0.0.0.0', 8888))
