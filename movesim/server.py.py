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


async def handle_client(reader, writer):
    parser = argparse.ArgumentParser()
    parser.add_argument("--time_step", type=int,
                        default=1000, help="length of simulation")
    parser.add_argument("--filename", type=str, default="simulation.gif",
                        help="the name of the animation gif")

    opt = parser.parse_args()
    if not os.path.exists("output"):
        os.mkdir("output")

    dimensions = [1000, 1000]
    mountains = []
    lakes = [[300, 300, 200], [50, 100, 40], [600, 300, 40],
             [800, 700, 80], [387, 892, 80], [862, 651, 160]]

    new_map = Map(dimensions, mountains, lakes)
    lion = Lion(np.random.randint(0, 1000, 2), 0, new_map)
    lion1 = Lion(np.random.randint(0, 1000, 2), 3, new_map)
    zebra1 = Zebra(np.random.randint(0, 1000, 2), 1, new_map)
    zebra2 = Zebra(np.random.randint(0, 1000, 2), 2, new_map)
    all_animal = [lion, lion1, zebra1, zebra2]
    time_step = opt.time_step
    sim = Simulator(new_map, all_animal, time_step)

    addr = writer.get_extra_info('peername')

    while True:
        data = sim.get_json
        await asyncio.sleep(0.00001)
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

        writer.write(sendAbleJson.encode())
        await writer.drain()

    print("Close the connection")
    writer.close()


async def main():

    server = await asyncio.start_server(
        handle_client, '0.0.0.0', 8888)

    addr = server.sockets[0].getsockname()
    print(f'Serving on {addr}')

    async with server:
        await server.serve_forever()


if __name__ == "__main__":
    loop = asyncio.get_event_loop()
    loop.run_until_complete(main())
