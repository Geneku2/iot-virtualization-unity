import utilities as utils
import Animal

import matplotlib.pyplot as plt
import matplotlib.patches as patches

import numpy as np

from shapely.geometry import Point, box, LineString
from shapely.ops import nearest_points


class Grass:
    def __init__(self, location):
        self.location = location

        self.alive = True
        self.respawn_timer = 20

    def respawn(self):
        """

        @return:
        """

        if self.respawn_timer > 0:
            self.respawn_timer -= 1
        else:
            self.alive = True
            self.respawn_timer = 20


class Lake:
    def __init__(self):
        pass


class Map:
    def __init__(self, dimensions: list, mountains: list, lakes: list,
                       temperature: float = 25, humidity: float = 40, air_quality: float = 400,
                       temperature_dev: float = 1.0, humidity_dev: float = 5.0, air_quality_dev: float = 20):
        """
            @param dimensions: (M, N) array_like - Dimension of the Map
            @param mountains: (N, 4) array_like - Corner coordinates for N mountains in array form
            @param lakes: (N, 4) array_like - Corner coordinates for N lakes in array form

            @param temperature: float - Initial temperature
            @param humidity: float - Initial humidity
            @param air_quality: float - Initial air quality

            @param temperature_dev: float - Initial std for temperature
            @param humidity_dev: float - Initial std for humidity
            @param air_quality_dev: float - Initial std for air quality
        """

        # These indicate where patches of grass are
        self.grass_patch1 = [100, 600, 300, 200]
        self.grass_patch2 = [700, 100, 250, 300]
        self.grass_patch3 = [500, 550, 150, 350]

        self.grass_patch = [self.grass_patch1, self.grass_patch2, self.grass_patch3]
        self.eaten_grass = []

        self.dimensions = dimensions
        self.mountains = mountains

        self.shapely_mountains = []
        for mountain in self.mountains:
            self.shapely_mountains.append(
                box(mountain[0], mountain[1], mountain[0] + mountain[2], mountain[1] + mountain[3]))

        self.lakes = lakes
        self.shapely_lakes = []
        for lake in self.lakes:
            self.shapely_lakes.append(Point(lake[0], lake[1]).buffer(lake[2]))

        self.grass = None
        self.initialize_grass()

        # TODO Add features for different seasons/weather
        self.weather = "sunny"
        self.weather_dict = {"sunny": [25, 40, 400], "rainy": [20, 90, 200], "stormy": [19, 100, 300],
                             "cloudy": [], "windy": [], "foggy": [], "drought": []}

        self.temperature = np.random.normal(temperature, temperature_dev, self.dimensions)  # 25 degree Celsius
        self.humidity = np.random.normal(humidity, humidity_dev, self.dimensions)  # 40% humidity
        self.air_quality = np.random.normal(air_quality, air_quality_dev, self.dimensions)  # 400 ppm

    def initialize_grass(self):
        """
        This function initializes grass for zebra
        @return:
        """

        grass = []

        for row in range(300):
            for col in range(200):
                grass.append(Grass([100+row, 600+col]))

        for row in range(250):
            for col in range(300):
                grass.append(Grass([700+row, 100+col]))

        for row in range(150):
            for col in range(350):
                grass.append(Grass([500+row, 550+col]))

        self.grass = grass

    def grass_update(self):
        """

        @return:
        """

        for grass in self.eaten_grass:
            grass.respawn()

            if grass.respawn_timer == 0:
                self.eaten_grass.remove(grass)

    def map_update(self, temperature_change: float = 0, humidity_change: float = 0, air_quality_change: float = 0,
                   temperature_dev: float = 0.2, humidity_dev: float = 5, air_quality_dev: float = 20):
        """
            @param temperature_change: float - mean for temperature change
            @param humidity_change: float - mean for humidity change
            @param air_quality_change: float - mean for air quality change

            @param temperature_dev: float- std for temperature change
            @param humidity_dev: float - std for humidity change
            @param air_quality_dev: float - std for air quality change
        """

        # self.temperature += np.random.normal(temperature_change, temperature_dev, self.dimensions)
        # self.humidity += np.random.normal(humidity_change, humidity_dev, self.dimensions)
        # self.air_quality += np.random.normal(air_quality_change, air_quality_dev, self.dimensions)

        # self.grass_update()

    def weather(self):
        pass

    def find_distance(self, location):
        """
        Find distance at a given location from the closest object in that direction
        """
        
        pass
    
    def find_temperature(self, location):
        """
        Find temperature at given location

            @param location: (2,) array-like - location of an animal
            @return: float - temperature at location
        """

        temperature = self.temperature[int(location[0])][int(location[1])]

        for lake in self.lakes:
            dist = utils.distance(location, lake[:2])

            if lake[2] <= dist < lake[2] + 15:
                temperature -= dist / 5 - 3
                return temperature

        return temperature

    def find_humidity(self, location):
        """
        Find humidity at given location

            @param location: (2,) array-like - location of an animal
            @return: float - humidity at location
        """

        humidity = self.humidity[int(location[0])][int(location[1])]

        for lake in self.lakes:
            dist = utils.distance(location, lake[:2])

            if lake[2] <= dist < lake[2] + 15:
                humidity -= dist / 5 - 3
                return humidity

        return humidity

    def find_air_quality(self, location):
        """
        Find air quality at given location

            @param location: (2,) array-like - location of an animal
            @return: float - air quality at location
        """

        air_quality = self.air_quality[int(location[0])][int(location[1])]

        for lake in self.lakes:
            dist = utils.distance(location, lake[:2])

            if lake[2] <= dist < lake[2] + 15:
                air_quality -= dist / 5 - 3
                return air_quality

        return air_quality

    def find_closest_lake(self, location) -> list:
        """
        Find location of the closest lake (closest point on the boundary)
        
            @param location: (2,) array-like - location of an animal
            @return First value: the closest lake center (to where we go), second value: the lake location
        """

        min_location = None
        min_dist: float = 10000  # TODO become np.inf

        loc = Point(location[0], location[1]) # Use Shapely.Point

        ##########################################################
        # Use shapely polygon and point to calculate the nearest #  --> Please check doc for shapely usage
        # point on the boundary of the lake from the animal      #
        ##########################################################

        for lake in self.lakes:
            # Point.buffer(int) gives a circle and gets its boundary for intersection check
            lake_bound = Point(lake[0], lake[1]).buffer(lake[2]).boundary

            # Find the closest point (straight line) from current loc to the circle
            p1, p2 = nearest_points(lake_bound, loc)

            # p1.coords[0] returns the coordinate for the Shapely Point object
            curr_dist = utils.distance(location, list(p1.coords[0]))

            if curr_dist < min_dist:
                min_location = list(p1.coords[0])
                min_dist = curr_dist

        return min_location

    def find_closest_food(self, location):
        """
        This function finds the closest food location for zebra
        """

        min_dist = 10000
        min_loc = None

        for grass in self.grass:
            dist = utils.distance(location, grass.location)

            if dist < min_dist:
                min_dist = dist
                min_loc = grass

        return min_loc

    # FIXME If the distance is larger than 30 (or vision distance)
    def find_closest_prey(self, animal, all_animal):
        """
        Traverse all the predators and get the closest one W.R.T. current location

            @param: animal (Animal.py): object, single animal identity
            @param: all_animal (Animal.py): (N, ) array_like - a list of all animals on the map
            @return: closest prey (object)
        """

        location = animal.location
        closest_prey = None  # replace empty animal
        min_dist = 10000  # replace with np.inf

        # Fixed here
        for prey in all_animal:
            if prey.type.lower() == "zebra":
                # Make sure it is the only predator (if two lions go after the same zebra and one gets there first)
                if prey.predator is not None and prey.predator != self:
                    continue

                if prey.isDead is False or prey.respawn_time > 15:  # at respawn timer = 5, the body decomposed
                    curr_dist = utils.distance(location, prey.location)
                    if curr_dist < min_dist:
                        closest_prey = prey
                        min_dist = curr_dist

        return closest_prey

    def find_closest_enemy(self, animal: Animal, all_animal):
        """
        Traverse all the predators and get the closest one W.R.T. current location

            @param: animal (Animal.py): object, single animal identity
            @param: all_animal (Animal.py): (N, ) array_like - a list of all animals on the map
            @return: closest predator (object)
        """

        location = animal.location
        closest_enemy = None  # replace empty animal
        min_dist = 10000  # replace with np.inf 

        # Fixed here
        for enemy in all_animal:
            if enemy.type.lower() == "lion" and enemy.isDead is False:
                curr_dist = utils.distance(location, enemy.location)
                if curr_dist < min_dist:
                    closest_enemy = enemy
                    min_dist = curr_dist

        return closest_enemy

    def is_valid(self, location, new_location) -> bool:
        """
        Check whether a given location is valid on a map
        Note that we not only check for the new location, but also any point on the path to the new location
        For detail information, please check Shapely documentation

            @param location: array_like (2,) - Coordinates of the current location
            @param new_location: array_like (2,) - Coordinates of the new location
            @return bool - True if self is not in any non-enterable (i.e. lake and mountain). False otherwise
        """

        # Construct the path using Linestring
        path = LineString([location, new_location])

        # For all lakes mountains, check whether this path passes through.
        for lake in self.shapely_lakes:
            # Check whether the new location is not within invalid location
            if Point(new_location).within(lake):
                return False

            # Check whether the path intersects with the boundary of the circle
            # This makes sure that along the moving direction, it does not pass invalid location
            if path.intersects(lake.boundary):
                return False

        # TODO remove this since there is no mountains now
        for mountain in self.shapely_mountains:
            if path.intersects(mountain):
                return False

        # Check whether the new location is within boundaries
        if 0 < new_location[0] < self.dimensions[0] and 0 < new_location[1] < self.dimensions[1]:
            return True

        return False

    def random_location(self):
        """
        This function returns a randomly selected location on the simulation map

            @return loc: array-like (2,) - random valid location on the map
        """

        loc = [np.random.randint(0, self.dimensions[0]), np.random.randint(0, self.dimensions[1])]
        while not self.is_valid(loc, loc):
            loc = [np.random.randint(0, self.dimensions[0]), np.random.randint(0, self.dimensions[1])]

        return loc
