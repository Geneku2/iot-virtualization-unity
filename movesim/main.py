import os
import argparse

from Simulator import *
from Map_generator import *
from Animal import *


if __name__ == "__main__":
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
    time_step = opt.time_step

    sim = Simulator(new_map, [4, 10], time_step)
    sim.run()

