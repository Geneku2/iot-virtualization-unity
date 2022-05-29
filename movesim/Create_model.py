#
# Copyright (c) University of Illinois and Bangladesh University of Engineering and Technology. All rights reserved.
# Unauthorized copying of this file, via any medium is strictly prohibited
# Proprietary and confidential. No warranties, express or implied.
#
import argparse
import numpy as np
import pandas as pd


def create_samples(argument, num_sample=100):
    map_height = argument.mh
    map_width = argument.mw

    all_samples = []

    for each in range(num_sample):
        x_range = np.random.randint(0, map_width-1)
        y_range = np.random.randint(0, map_height-1)

        surrounding_obstacle = np.random.randint(0, 2)
        closest_prey_x = np.random.randint(0, map_width-1)
        closest_prey_y = np.random.randint(0, map_width-1)

        while x_range == closest_prey_x and y_range == closest_prey_y:
            closest_prey_x = np.random.randint(0, map_width - 1)
            closest_prey_y = np.random.randint(0, map_width - 1)

        hunger = np.random.randint(0, 100)
        thirst = np.random.randint(0, 100)
        sleep = np.random.randint(0, 100)
        health = np.random.randint(0, 100)

        sample = [x_range, y_range, surrounding_obstacle, closest_prey_x, closest_prey_x, hunger, thirst, sleep, health, 0]
        all_samples.append(sample)

    return all_samples


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Provide Map Parameters')

    parser.add_argument("--mh", type=int, default="400", help="Map Height")
    parser.add_argument("--mw", type=int, default="400", help="Map Width")

    opt = parser.parse_args()
    samples = pd.DataFrame(create_samples(opt, 100))
    samples.columns = ["x", "y", "obstacles", "prey x", "prey y", "hunger", "thirst", "sleep", "health", "action"]
    samples.to_csv("sample.csv")
