#
# Copyright (c) University of Illinois and Bangladesh University of Engineering and Technology. All rights reserved.
# Unauthorized copying of this file, via any medium is strictly prohibited
# Proprietary and confidential. No warranties, express or implied.
#
import copy
from progress.bar import Bar, ShadyBar

from Animator import *
import numpy as np
import json


class ShadyBar1(ShadyBar):
    @property
    def dot(self):
        return '.' + '.' * (int(self.percent) % 3) + ' ' * (2 - int(self.percent) % 3)

    @property
    def end(self):
        if self.percent < 100:
            return str(int(self.percent)) + '% - ' + str(int(self.index)) + '/' + str(int(self.max))
        else:
            return 'Simulation Finished!'


class Simulator:
    def __init__(self, map, animals, time_step=2000):
        self.map = map
        self.animals = animals

        for animal in animals:
            animal.all_animal = animals

        self.time_step = time_step
        self.curr_time = 0

        self.bar = None

        self.animal_data = {self.curr_time: []}
        self.animator = Animator(map, self.animal_data)

    def run(self):
        fig, ax = self.animator.initialize_map()
        self.bar = ShadyBar1(message="Running Simulation %(dot)s", max=time_step, suffix='%(end)s')

        while self.curr_time < self.time_step:
            self.run_once()

            self.animator.get_time_step(self.curr_time-1, fig, ax)
            self.bar.next()

        self.animator.get_gif()
        np.save("simulation.npy", self.animal_data)

        print("\n")

    def run_once(self):
        self.animal_data[self.curr_time] = []

        for each_animal in self.animals:
            each_animal()

            animal_dict = {}
            for key in list(each_animal.__dict__.keys())[:9]:
                animal_dict[key] = copy.copy(each_animal.__dict__[key])

            self.animal_data[self.curr_time].append(animal_dict)

        self.curr_time += 1

    @property
    def get_json(self):
        self.run_once()
        animal_data_to_vr = self.animal_data[self.curr_time-1]
        return json.dumps(animal_data_to_vr)








