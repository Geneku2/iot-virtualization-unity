#
# Copyright (c) University of Illinois and Bangladesh University of Engineering and Technology. All rights reserved.
# Unauthorized copying of this file, via any medium is strictly prohibited
# Proprietary and confidential. No warranties, express or implied.
#
def distance(location1, location2):
    """
    Find the euclidean distance between two locations

        :param location1: array_like - (2,)
                Coordinates of the first location
        :param location2: array_like - (2,)
                Coordinates of the second location
        :return: dist: float
                The euclidean distance between two locations
    """

    dist = ((location1[0] - location2[0]) ** 2 + (location1[1] - location2[1]) ** 2) ** 0.5
    return dist


def inside_entity(entity: list, location: list) -> bool:
    """
    Helper Function - Check whether a given location in inside a non-enterable entity

        :param entity: array_like - (2,)
                Coordinates of a specific non-enterable entity
        :param location: array_like - (2,)
                Coordinates of the current location
        :return: bool - True if the location is inside an entity. False otherwise
    """

    if entity[0] < location[0] < entity[0] + entity[2] and entity[1] < location[1] < entity[1] + entity[3]:
        return True

    return False
