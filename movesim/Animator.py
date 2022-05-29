#
# Copyright (c) University of Illinois and Bangladesh University of Engineering and Technology. All rights reserved.
# Unauthorized copying of this file, via any medium is strictly prohibited
# Proprietary and confidential. No warranties, express or implied.
#
from PIL import Image
import matplotlib.pyplot as plt
import matplotlib.patches as patches


class Animator:
    def __init__(self, map, animal_data):
        self.map = map
        self.animal_data = animal_data

        self.curr_points = []
        self.frames = []
        self.gif = None

    def initialize_map(self):
        """
        This Function initializes a map consisting of mountains and lakes. Coordinates
        Coordinates are stored respective class variables.

            @return: fig, ax - matplotlib object for display
        """
        # Display a map (non-blocking)
        fig, ax = plt.subplots(1)
        plt.axis([0, self.map.dimensions[0], 0, self.map.dimensions[1]])

        ax.add_patch(patches.Rectangle((0, 0), self.map.dimensions[0], self.map.dimensions[1], linewidth=1,
                     edgecolor='lawngreen', facecolor='lawngreen'))

        for lake in self.map.lakes:
            ax.add_patch(patches.Circle((lake[0], lake[1]), radius=lake[2], linewidth=1,
                                           edgecolor='skyblue', facecolor='skyblue'))

        return fig, ax

    def get_gif(self, filename="simulation.gif"):
        """
        This function draws a gif for the entire simulation

            @param filename: str - name of the generated gif
        """

        self.frames[0].save(filename, format='GIF',
                            append_images=self.frames[1:],
                            save_all=True,
                            duration=10, loop=0)

    def get_time_step(self, time_step: int, fig: object, ax: object):
        """
        This function draws a particular time step

            @param time_step: int - a specific time step of the simulation
            @param fig: matplotlib figure object
            @param ax: matplotlib axis object
        """
        for each in self.animal_data[time_step]:
            if each["type"] == "zebra":
                color = 'bo'
            if each["type"] == "lion":
                color = 'ro'
            if each["isDead"]:
                color = "k"

            if not each["isDead"]:
                p, = ax.plot(each["location"][0], each["location"][1], color)

                state = each["state"]
                state = str(int(each["state"]["hunger"])) + "," + \
                        str(int(each["state"]["thirst"])) + "," + \
                        str(int(each["state"]["sleep"])) + "," + \
                        str(int(each["health"])) + ","+each["action"]

                an = ax.annotate(state, (each["location"][0], each["location"][1]), fontsize=8)
                self.curr_points.append(an)
                self.curr_points.append(p)
            else:
                p, = ax.plot(each["location"][0], each["location"][1], color)

                respawn = "respawn"
                if each["type"] == "zebra" and each["predator"] is not None:
                    respawn = "eaten"

                state = each["state"]
                state = str(int(each["state"]["hunger"])) + "," + \
                        str(int(each["state"]["thirst"])) + "," + \
                        str(int(each["state"]["sleep"])) + "," + \
                        str(int(each["health"])) + "," + respawn

                an = ax.annotate(state, (each["location"][0], each["location"][1]), fontsize=12)
                self.curr_points.append(an)
                self.curr_points.append(p)

        ax.set_title("Time Step: "+str(time_step))
        fig.savefig('output/plot' + str(time_step) + '.png')
        self.clear_points()

        new_frame = Image.open("output/plot" + str(time_step) + ".png")
        self.frames.append(new_frame)

    def clear_points(self):
        for p in self.curr_points:
            p.remove()
        self.curr_points = []
