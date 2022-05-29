#
# Copyright (c) University of Illinois and Bangladesh University of Engineering and Technology. All rights reserved.
# Unauthorized copying of this file, via any medium is strictly prohibited
# Proprietary and confidential. No warranties, express or implied.
#
import random
import math

from FSM_Model import *
import utilities as utils


class Animal:
    def __init__(self, location: list, id: int, map: map,
                 hunger: int = 100, thirst: int = 100, sleep: int = 100, health: int = 100):
        """
        Initialize Animal Class with location, animal id, map, and internal states

            @param location: array-like - (x, y) coordinates of the animal TODO: Suppress in the future
            @param id: int - id of the animal
            @param map: - map (Map_generator.py) - The simulation map

            @param hunger: int - Hunger
            @param thirst: int - Thirst
            @param sleep : int - Sleep
            @param health: int - Health
        """

        ###############################################
        # Attributes that define animal's information #
        ###############################################
        self.id = id
        self.location = map.random_location()
        self.type = None

        ##############################################
        # Attributes that define animal's conditions #
        ##############################################
        self.action = "walk"

        self.vision = 30
        self.health = health
        self.state = {"hunger": hunger, "thirst": thirst, "sleep": sleep}

        #############################################
        # Attributes that contain simulation status #
        #############################################
        self.isDead = False
        self.respawn_time = 10

        ############################################
        # Attributes that define animal's movement #
        ############################################
        self.walk_stride = 1  # Moving speed
        self.run_stride = 2  # Moving speed
        self.move_angle = random.uniform(0, 2 * np.pi)

        ######################################################
        # Attributes that define animal's external knowledge #
        ######################################################
        self.circuit = None
        self.map = map  # Simulation Map
        self.all_animal = []  # List of all other animals including itself

    def drink(self):
        pass

    def eat(self):
        pass

    def walk(self):
        pass

    def run(self):
        pass

    def sleep(self):
        pass

    def update_states(self):
        pass

    def update_health(self):
        pass

    def update_thirst(self):
        pass

    def update_hunger(self):
        pass

    def update_sleep(self):
        pass

    def move_direction(self):
        pass

    def get_actions(self):
        pass


class Lion(Animal):
    def __init__(self, location: list, id: int, map: map,
                 hunger: int = 100, thirst: int = 100, sleep: int = 100, health: int = 100):
        """
        Initializer for Lion Class; Takes argument location, id, map, and internal states

            @param location: array-like - (x, y) coordinates of the animal
            @param id: int - id of the animal
            @param map: - map (map.py) - The simulation map

            @param hunger: int - Hunger
            @param thirst: int - Thirst
            @param sleep : int - Sleep
            @param health: int - Health
        """

        super().__init__(location, id, map, hunger=100, thirst=100, sleep=100, health=100)

        ################################
        # Attributes that define lions #
        ################################
        self.type = "lion"
        self.model = FSMLionModel()

        ##############################################
        # Attributes that define lion's conditions #
        ##############################################
        self.vision = 40
        self.action = "walk"
        self.prey = None

        ###########################################
        # Attributes that define lions's movement #
        ###########################################
        self.drink_stride = 2
        self.run_stride = 4
        self.previous_angle = None

        ######################################
        # Attribute reserved for future used #
        ######################################
        self.action_dict = {0: "drink", 1: "eat", 2: "sleep", 3: "walk",
                            4: "run"}

    def __call__(self, circuit=None):
        # Reserve for future use
        self.circuit = circuit

        self.execute_action()

    ##############################################################
    # This section corresponds to different action of the animal #
    ##############################################################

    def eat(self):
        """
        This function simulates eat action. The lion acts differently depending on its location toward the prey.

            if dist to prey > 1:
                hunt the prey
            if dist to prey <= 1:
                kill and eat the prey

        Notice it may take several time steps to complete killing and eating

            @return: None
        """

        ###############################################################
        # Decide whether it should start running toward its prey or   #
        # it should start killing it or it should starting eating     #
        ###############################################################

        # If the lion doesn't have a target, find the closest one
        if self.prey is None:
            prey: Zebra = self.map.find_closest_prey(self, self.all_animal)

            # if all zebras are dead, then the lion does nothing
            if prey is None:
                return
            self.prey = prey  # set the lion's prey to be the one found
        else:
            # set the lion's target prey as self.prey
            prey = self.prey

        # If location of its prey is not next to it, start running toward it
        if utils.distance(self.location, prey.location) > 1:
            # TODO Add feature where lion's group share a prey
            #      Zebra.predator to be the group (self.herd) and self.herd.prey to be the zebra)

            # TODO Add feature where lion still goes after naturally dead animal (add attribute to prolong respawn time)
            #      prey.hunted = True -> not respawn
            #      prey.hunted = True -> self.health does not change (not depending on internal states temporarily)

            # Lion always goes after the closest prey
            prey = self.map.find_closest_prey(self, self.all_animal)

            # if all zebras are dead, then the lion does nothing
            if prey is None:
                return
            self.prey = prey

            self.run(location=prey.location)
        else:
            # if the health of its prey is not 0, it needs to be killed first
            if prey.health != 0:
                prey.health -= 30  # Each time step the prey's health is reduced by 40

                prey.health = max(0, prey.health)  # the health is kept at non-zero value
                for state in prey.state:  # set internal states to self.health (avoid recalculation of health)
                    prey.state[state] = prey.health

                if prey.health == 0:
                    prey.isDead = True
                    prey.predator = self  # Used in Zebra class --> respawn only after its predator finishes eating

                # The killing process takes same energy as running [check self.run()]
                for key, value in self.state.items():
                    self.state[key] = value - 2 / 10
                    self.state[key] = max(self.state[key], 0)

            # The prey is dead (set all internal states to be 0) and mark its predator to be me
            else:
                # Each time step lion will recover 2 hunger value and keep at maximum of 100
                self.state["hunger"] += 2
                self.state["hunger"] = min(100, self.state["hunger"])

    def drink(self):
        """
        This function simulates drink action. The lion acts differently depending on its location toward the lake.

            if dist to lake <= 1:
                drink at the lake
            else if dist to lake <= 10:
                walk toward the lake
            else
                run to the lake

            @return: None
        """

        ######################################################
        # Decide whether it should start running toward the  #
        # lake or start walking toward it or start drinking  #
        ######################################################
        lake: list[2, ] = self.map.find_closest_lake(self.location)  # lake is a (2,) List

        # If it is at the lake, start drinking (thirst
        if utils.distance(self.location, lake) < 1:
            self.state["thirst"] += 2
            self.state["thirst"] = min(100, self.state["thirst"])

        # If it is near the lake, start running toward it
        elif utils.distance(self.location, lake) < 10:
            stride: float = np.exp((utils.distance(self.location, lake) - 1) / (9 / np.log(2)))
            self.walk(mutation=0, stride=stride, location=lake)

        # If it is far away from the lake, start running
        else:
            self.run(stride=self.drink_stride, location=lake)

    def sleep(self):
        """
        The function simulates sleep action.
        """

        # Every time_step it restores 2 sleep
        self.state["sleep"] += 2
        self.state["sleep"] = min(100, self.state["sleep"])

    def stand(self):
        """
        The function simulates stand action.
        """

        # Do nothing while standing
        pass

    def walk(self, mutation=1, stride=None, location=None):
        """
        This function simulates walk action of the animal. The animal will move along its original direction
        until it hits un-passable or it randomly changes direction.

            @param mutation: int, optional - 1 if the animal will randomly change direction, 0 otherwise
            @param stride: float, optional - The distance travelled each move; Default to be None
            @param location: array-like, optional (2,) - Coordinates of the destination
        """

        if stride is None:
            stride = self.walk_stride

        # Update all internal states (
        for key, value in self.state.items():
            self.state[key] = value - 1 / 10
            self.state[key] = max(0, self.state[key])

        self.location = self.move_direction(mutation=mutation, stride=stride, location=location)

    def run(self, stride=None, location=None):
        """
        This function simulates run action of the animal. The animal will move toward the given location
        until it hits un-passable obstacle.

            @param stride: int, optional - The distance travelled each move
            @param location: array-like, optional - (2,) Coordinates of the destination
        """

        for key, value in self.state.items():
            self.state[key] = value - 2 / 10
            self.state[key] = max(0, self.state[key])

        if stride is None:
            stride = self.run_stride

        self.location = self.move_direction(mutation=0, stride=stride, location=location)

    ############################################################
    # This section corresponds to help functions for the class #
    ############################################################

    def move_direction(self, mutation: int = 0, stride: int = 1, location: list = None) -> list:
        """
        If the animal is wandering, it want it to keep moving the same direction with some chances of mutation
        Note that the change of the moving angle are limited to -pi/3 to pi/3

            @param mutation: int, optional - The default value is 0, meaning no mutation
                    If mutation is 1, it will randomly change direction with 25% probability
            @param stride: number, optional - The default value is 1
                    The length of each movement
            @param location: array_like, optional - (2,)
                    If location is given, it will move toward that direction using self.move_direction()
            @return new_location: array_like, optional - (2,)
                    Coordinates of the new location of the animal
        """

        #################################################################################################
        # Determine the moving angle depending on whether there is chance to randomly change direction  #
        #################################################################################################
        if location is None:
            if mutation == 1:
                mutation = np.random.randint(0, 100)
                if mutation % 4 == 0:
                    self.move_angle = random.uniform(self.move_angle - np.pi / 3, self.move_angle + np.pi / 3)
        else:
            self.move_angle = math.atan2(location[1] - self.location[1], location[0] - self.location[0])

        ###########################################################################################
        # If the distance is closer than one footstep, set the new location to be the destination #
        ###########################################################################################
        if location is not None and utils.distance(self.location, location) < abs(stride):
            return location

        new_location = [0, 0]
        new_location[0] = self.location[0] + np.cos(self.move_angle) * stride
        new_location[1] = self.location[1] + np.sin(self.move_angle) * stride

        ######################################################################################################
        # Check whether my next step will be valid and shift move angle by pi/20 until my next step is valid #
        ######################################################################################################
        counter = 1  # Counter for (-1)^n

        while not self.map.is_valid(self.location, new_location):
            self.move_angle += np.pi / 20 * counter * (-1) ** counter

            # Example: move_angle = pi/3 -> it will check:
            #          move_angle = pi/3 + pi/20,
            #                ...  = pi/3 - pi/20,
            #                ...  = pi/3 - pi/20,
            #                ...  = pi/3 + 2 * pi/20,
            #                ...  = pi/3 - 2 * pi/20,
            #                ...    ...
            #          until a movable direction is found

            new_location[0] = self.location[0] + np.cos(self.move_angle) * stride
            new_location[1] = self.location[1] + np.sin(self.move_angle) * stride
            counter += 1

        return new_location

    ######################################################
    # This section updates internal states of the animal #
    ######################################################

    def update_states(self):
        self.update_thirst()
        self.update_sleep()
        self.update_hunger()
        self.update_health()

    def update_thirst(self):
        self.state["thirst"] -= 2 / 10
        self.state["thirst"] = max(0, self.state["thirst"])

    def update_hunger(self):
        self.state["hunger"] -= 2 / 10
        self.state["hunger"] = max(0, self.state["hunger"])

    def update_sleep(self):
        self.state["sleep"] -= 2 / 10
        self.state["sleep"] = max(0, self.state["sleep"])

    def update_health(self):
        self.health = 0.40 * self.state["thirst"] + 0.35 * self.state["hunger"] + 0.25 * self.state["sleep"]

        if self.health == 0:
            self.isDead = True

    def respawn(self):
        """
        This function is responsible for the respawn of the dead animal. It will recover the initial state of the animal
        and it will be placed at a random location on the map.
        """

        if self.isDead:
            if self.prey is not None:
                self.prey.predator = None
            self.prey = None

            if self.respawn_time > 0:
                self.respawn_time -= 1
            else:
                self.respawn_time = 10
                self.isDead = False

                self.health = 100
                self.state["thirst"] = 100
                self.state["hunger"] = 100
                self.state["sleep"] = 100

                self.run_stride = 4
                self.action = "walk"

                self.location = self.map.random_location()

    #################################################
    # This section gets action and executes actions #
    #################################################

    def get_action(self):
        """
        This function calls the FSM Model to get action for the current time step
        """
        action: str = self.model.predict(self.state, self.action)

        self.action = action

    def execute_action(self):
        """
        This function calls get_action to get animal action and execute in one time_step
        """

        # If the animal is dead do nothing and respawn
        if self.isDead:
            self.respawn()
            return

        # Call get_action and execute the decision
        self.get_action()

        ###################################################
        # Execute action by calling simulation functions  #
        ###################################################
        if self.action == "eat":
            self.eat()
        elif self.action == "drink":
            self.drink()
        elif self.action == "sleep":
            self.sleep()
        elif self.action == "walk":
            # Give some probabilities for the lion to either stand or walk
            mutation = np.random.randint(0, 100)
            if mutation % 4 == 0:
                self.stand()
            else:
                self.walk()
        elif self.action == "run":
            prey = self.map.find_closest_prey(self, self.all_animal)
            self.run(location=prey.location)

        self.update_states()


class Zebra(Animal):
    def __init__(self, location: list, id: int, map: map,
                 hunger: int = 100, thirst: int = 100, sleep: int = 100, health: int = 100):
        """
        Initializer for Lion Class; Takes argument location, id, map, and internal states

            @param location: array-like - (x, y) coordinates of the animal
            @param id: int - id of the animal
            @param map: - map (map.py) - The simulation map

            @param hunger: int - Hunger
            @param thirst: int - Thirst
            @param sleep: int - Sleep
            @param health: int - Health
        """

        super().__init__(location, id, map, hunger=100, thirst=100, sleep=100, health=100)

        ################################
        # Attributes that define lions #
        ################################
        self.type = "zebra"
        self.model = FSMZebraModel()

        ###########################################
        # Attributes that define zebra's movement #
        ###########################################
        self.vision = 20

        self.action = "walk"
        self.predator = None

        ######################################
        # Attribute reserved for future used #
        ######################################
        self.action_dict = {0: "drink", 1: "eat", 2: "sleep", 3: "walk",
                            4: "run"}

    def __call__(self, circuit=None):
        self.circuit = circuit
        self.execute_action()

    def eat(self):
        """
        This function simulates eat action

            @return: None
        """

        self.state["hunger"] += 2
        self.state["hunger"] = min(100, self.state["hunger"])

    def drink(self):
        """
        This function simulates drink action. The lion acts differently depending on its location toward the lake.

            if dist to lake <= 1:
                drink at the lake
            else if dist to lake <= 10:
                walk toward the lake
            else
                run to the lake
        """

        ######################################################
        # Decide whether it should start running toward the  #
        # lake or start walking toward it or start drinking  #
        ######################################################
        lake = self.map.find_closest_lake(self.location)  # lake is a 2D List

        # If it is at the lake, start drinking
        if utils.distance(self.location, lake) <= 1:
            self.state["thirst"] += 2
            self.state["thirst"] = min(100, self.state["thirst"])

        # If it is near the lake, start walk toward it
        elif utils.distance(self.location, lake) < 10:
            stride = np.exp((utils.distance(self.location, lake) - 1) / (9 / np.log(2)))
            self.walk(mutation=0, stride=stride, location=lake)

        # If it is far away from the lake, start running
        else:
            self.run(location=lake)  # self.walk with argument of the lake's coordinates (no mutation)

    def sleep(self):
        """
        The function simulates sleep action.
        """
        # Zebra's vision will be limited during sleep. It will be reset to 30 every time_step

        self.vision = 10
        self.state["sleep"] += 2
        self.state["sleep"] = min(100, self.state["sleep"])

    def stand(self):
        """
        The function simulates stand action.
        """

        self.action = "stand"
        pass

    def walk(self, mutation=1, stride=None, location=None):
        """
        This function simulates walk action of the animal. The animal will move along its original direction
        until it hits un-passable or it randomly changes direction.

            @param mutation: int, optional - 1 if the animal will randomly change direction, 0 otherwise
            @param stride: float, optional - The distance travelled each move; Default to be None
            @param location: array-like, optional (2,) - Coordinates of the destination
        """
        if stride is None:
            stride = self.walk_stride

        # Update all internal states (
        for key, value in self.state.items():
            self.state[key] = value - 1 / 10
            self.state[key] = max(0, self.state[key])

        self.location = self.move_direction(mutation=mutation, stride=stride, location=location)

    def run(self, direction=1, location=None):
        """
        This function simulates run action of the animal. The animal will move toward the given location
        until it hits un-passable obstacle.

            @param location: array-like, optional (2,) - Coordinates of the destination
            @param direction: int - the direction of the movement. Default to be 1 if moving forward, otherwise -1
                    This parameter is set to -1 when the zebra is running away from the lion
        """

        for key, value in self.state.items():
            self.state[key] = value - 2 / 10
            self.state[key] = max(0, self.state[key])

        self.location = self.move_direction(mutation=0, stride=direction * self.run_stride, location=location)

    ############################################################
    # This section corresponds to help functions for the class #
    ############################################################

    def move_direction(self, mutation=0, stride=1, location=None):
        """
        If the animal is wandering, it want it to keep moving the same direction with some chances of mutation
        Note that the change of the moving angle are limited to -pi/3 to pi/3

            @param mutation: int, optional - The default value is 0, meaning no mutation
                    If mutation is 1, it will randomly change direction with 25% probability
            @param stride: number, optional - The default value is 1
                    The length of each movement
            @param location: array_like, optional - (2,)
                    If location is given, it will move toward that direction using self.move_direction()
            @return new_location: array_like, optional - (2,)
                    Coordinates of the new location of the animal
        """

        #################################################################################################
        # Determine the moving angle depending on whether there is chance to randomly change direction  #
        #################################################################################################
        if location is None:
            if mutation == 1:
                mutation = np.random.randint(0, 100)
                if mutation % 4 == 0:
                    self.move_angle = random.uniform(self.move_angle - np.pi / 3, self.move_angle + np.pi / 3)
        else:
            self.move_angle = math.atan2(location[1] - self.location[1], location[0] - self.location[0])

        ###########################################################################################
        # If the distance is closer than one footstep, set the new location to be the destination #
        ###########################################################################################
        if location is not None and utils.distance(self.location, location) < abs(stride):
            return location

        new_location = [0, 0]
        new_location[0] = self.location[0] + np.cos(self.move_angle) * stride
        new_location[1] = self.location[1] + np.sin(self.move_angle) * stride

        # Check whether my next step will be valid and move perpendicular to the old direction (move_angle)
        mutation = 2  # np.random.randint(0, 100)
        counter = 1

        while not self.map.is_valid(self.location, new_location):
            if mutation % 2 == 0:
                self.move_angle += np.pi / 10 * counter * (-1) ** counter
                # self.move_angle = (self.move_angle // (np.pi/2))*np.pi/2
            else:
                self.move_angle -= np.pi / 10
                # self.move_angle = (self.move_angle // (np.pi / 2)) * np.pi / 2

            new_location[0] = self.location[0] + np.cos(self.move_angle) * stride
            new_location[1] = self.location[1] + np.sin(self.move_angle) * stride

        return new_location

    ######################################################
    # This section updates internal states of the animal #
    ######################################################

    def update_states(self):
        self.update_thirst()
        self.update_sleep()
        self.update_hunger()
        self.update_health()

    def update_thirst(self):
        self.state["thirst"] -= 2 / 10
        self.state["thirst"] = max(0, self.state["thirst"])

    def update_hunger(self):
        self.state["hunger"] -= 2 / 10
        self.state["hunger"] = max(0, self.state["hunger"])

    def update_sleep(self):
        self.state["sleep"] -= 2 / 10
        self.state["sleep"] = max(0, self.state["sleep"])

    def update_health(self):
        self.health = 0.40 * self.state["thirst"] + 0.35 * self.state["hunger"] + 0.25 * self.state["sleep"]

        if self.health == 0:
            self.isDead = True

    def respawn(self):
        """
        This function is responsible for the respawn of the dead animal. It will recover the initial state of the animal
        and it will be placed at a random location on the map.
        """

        if self.isDead:
            # It will not respawn until its predator finishes eating
            if self.predator is not None and self.predator.action == "eat":
                return
            # Reset predator's prey to be none after its predator finished eating
            elif self.predator is not None:
                self.predator.prey = None
                self.predator = None

            # Start the respawn timer
            if self.respawn_time > 0:
                self.respawn_time -= 1

            else:
                self.respawn_time = 10
                self.isDead = False

                self.health = 100
                self.state["thirst"] = 100
                self.state["hunger"] = 100
                self.state["sleep"] = 100

                self.run_stride = 2
                self.action = "walk"

                self.location = self.map.random_location()

    #################################################
    # This section gets action and executes actions #
    #################################################

    def get_action(self):
        """
        This function calls the FSM Model to get action for the current time step

            @return: None
        """

        action = self.model.predict(self.state, self.action)
        #################################################################################################
        # Regardless of the FSM Model, it will run away from predator if its distance is within vision  #
        #################################################################################################

        closest_enemy = self.map.find_closest_enemy(self, self.all_animal)
        if closest_enemy is not None and utils.distance(self.location, closest_enemy.location) < self.vision:
            action = "run"

        self.action = action

    def execute_action(self):
        """
        This function calls get_action to get animal action and execute in one time_step

            @return: None
        """

        # If the animal is dead do nothing and call self.respawn()
        if self.isDead:
            self.respawn()
            return

        # Call get_action and execute the decision
        self.get_action()

        ###################################################
        # Execute action by calling simulation functions  #
        ###################################################
        if self.action == "eat":
            self.eat()
        elif self.action == "drink":
            self.drink()
        elif self.action == "sleep":
            self.sleep()
        elif self.action == "walk":
            mutation = np.random.randint(0, 100)
            if mutation % 4 == 0:
                self.stand()
            else:
                self.walk()
        elif self.action == "run":
            enemy = self.map.find_closest_enemy(self, self.all_animal)
            self.run(direction=-1, location=enemy.location)

        # Reset vision every time we execute new action
        if self.action != "sleep":
            self.vision = 30

        self.update_states()
