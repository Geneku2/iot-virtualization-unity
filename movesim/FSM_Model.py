import numpy as np


class FSMLionModel:
    """
    This is the FSM implementation of the lion model. Next state is decided on current state and inputs.
    The following diagram illustrates a simplified version of the model and details will be in self.predict()

    ######### --------------> ########  -------------->  #######
    # Drink # is thirst < 30? # Walk #  is hunger < 30?  # Eat #
    ######### <-------------- ########  <--------------  #######
                                |  ↑
      ...       .........       |  |    is sleep < 30?     ...
      ...                       ↓  |                       ...
                              #########
      ...     More States     # Sleep #    More States     ...
                              #########

      ...                        ...                       ...
      ...     More States        ...       More States     ...

    """

    def __init__(self, walk_prob: int = 55, low_hunger: int = 30, high_hunger: int = 80,
                 low_thirst: int = 30, high_thirst: int = 80, low_sleep: int = 30, high_sleep: int = 80):
        """
        Initialize the thresholds for hunger, thirst, and sleep

            @param walk_prob: int The weighted value for the probability of walk/stand
            @param low_hunger: int -- lower threshold of hunger
            @param high_hunger: int -- higher threshold of hunger
            @param low_thirst: int -- lower threshold of thirst
            @param high_thirst: int -- higher threshold of thirst
            @param low_sleep: int -- lower threshold of sleep
            @param high_sleep: int -- higher threshold of sleep
        """

        self.walk_prob = walk_prob

        self.low_hunger = low_hunger
        self.high_hunger = high_hunger

        self.low_thirst = low_thirst
        self.high_thirst = high_thirst

        self.low_sleep = low_sleep
        self.high_sleep = high_sleep

        self.basic_action2state = {"hunger": "eat", "thirst": "drink", "sleep": "sleep", "walk": "walk", "stand": "stand","lay": "lay", "idle": "idle"}
        self.highlv_action2state = {"warmup": "warmup", "cooldown": "cooldown", "home": "home", "spouse": "spouse", "flee": "flee", "herd": "herd", 
                                    "find_friends": "find_friends", "find_family": "find_family", "find_leader": "find_leader", 
                                    "play": "play", "wander":"wander", "explore": "explore"}
    def get_action(self, states: dict = None):
        """
        Get specific action based on the weighted probability of each action. The probabilities of action
        are normalized to 1 using the corresponding condition values in the state dictionary.

        Example: p("eat") = hunger / sum(hunger + thirst + ... ) --> all values given in state (type: dict)

            @param states: dict {action_name : state_value, ...}
                    Dictionary describing actions and internal state values
                    Ex {"eat": hunger, ...}
            @return: action: str
                    Current action name after choosing with weighted probabilities
        """

        if states is None:
            states = {}
        action_list = list(states.keys())
        action_list.append("walk")

        prob = list(states.values())
        prob.append(self.walk_prob)
        prob = (100-np.array(prob))/np.sum(100-np.array(prob))

        action = np.random.choice(action_list, size=1, p=prob)
        if action == "walk":
            action = np.random.choice(["walk", "stand"], size=1, p=[0.6, 0.4])

        return action[0]

    def predict(self, state: dict, prev_action="walk"):
        """
        Forward to next state using the implementation of the FSM described above. Details could be
        found in the Readme.md and link:

            @param state: dict {state_name : state_value, ...}
                    Dictionary describing actions and internal state values
            @param prev_action: str
                    Previous action name
            @return: action: str
                    Current action name that the FSM produced
        """

        # Get all internal state values from the dict
        hunger = state["hunger"]
        thirst = state["thirst"]
        sleep = state["sleep"]

        # Default action to be stand (do nothing)
        action = "stand"

        # TODO Change 80 into a variable / different number (discusson needed)

        #####################################################################################
        # Forced actions for state values below low threshold. Continue the previous action #
        # until the respective state reached high threshold (So lion have consistency)      #
        #####################################################################################
        if hunger <= self.low_hunger:
            if prev_action == "drink" and thirst < self.high_thirst:
                action = "thirst"
            elif prev_action == "sleep" and sleep < self.high_sleep:
                action = "sleep"
            else:
                action = "hunger"
        elif thirst <= self.low_thirst:
            if prev_action == "eat" and hunger < self.high_hunger:
                action = "hunger"
            elif prev_action == "sleep" and sleep < self.high_sleep:
                action = "sleep"
            else:
                action = "thirst"
        elif sleep <= self.low_sleep:
            if prev_action == "eat" and hunger < self.high_hunger:
                action = "hunger"
            elif prev_action == "drink" and thirst < self.high_thirst:
                action = "thirst"
            else:
                action = "sleep"

        ###################################################################################################
        # Continue to eat or drink if previous state is eat or drink for states values between thresholds #
        ###################################################################################################
        elif self.low_thirst < thirst < self.high_thirst and prev_action == "drink":
            action = "thirst"
        elif self.low_thirst < thirst < self.high_thirst and prev_action == "eat" and hunger < self.high_hunger:
            action = "hunger"

        ##############################################################
        #  Weighted probability for each action for thirst value in  #
        #  between thresholds if previous state is not eat or drink  #
        ##############################################################
        elif self.low_thirst < thirst < self.high_thirst:
            if self.low_hunger < hunger < self.high_hunger:
                if self.low_sleep < sleep < self.high_sleep:
                    # Get Weighted Probability for all three state values between thresholds
                    state_dict = {"hunger": state["hunger"], "thirst": state["thirst"], "sleep": state["sleep"]}
                    action = self.get_action(state_dict)
                else:
                    # Get Weighted Probability for state values between thresholds
                    state_dict = {"hunger": state["hunger"], "thirst": state["thirst"]}
                    action = self.get_action(state_dict)
            elif self.low_sleep < sleep < self.high_sleep:
                # Get Weighted Probability for state values between thresholds
                state_dict = {"thirst": state["thirst"], "sleep": state["sleep"]}
                action = self.get_action(state_dict)
            else:
                # Get Weighted Probability for state values between thresholds
                state_dict = {"thirst": state["thirst"]}
                action = self.get_action(state_dict)

        ########################################################################################
        # Continue to eat if previous state is eat for hunger value between thresholds for eat #
        ########################################################################################
        elif self.low_hunger < hunger < self.high_hunger and prev_action == "eat":
            action = "hunger"
        elif self.low_hunger < hunger < self.high_hunger and prev_action == "drink" and thirst < self.high_thirst:
            action = "thirst"

        ##########################################################
        #  Weighted probability for each action for hunger value #
        #  in between thresholds if previous state is not eat    #
        ##########################################################
        elif self.low_hunger < hunger < self.high_hunger:
            if self.low_sleep < sleep < self.high_sleep:
                # Get Weighted Probability for state values between thresholds
                state_dict = {"hunger": state["hunger"], "sleep": state["sleep"]}
                action = self.get_action(state_dict)
            else:
                # Get Weighted Probability for state values between thresholds
                state_dict = {"hunger": state["hunger"]}
                action = self.get_action(state_dict)

        ####################################################################################################
        # Equal probability for continue sleeping or wake up for sleep value in between thresholds for eat #
        ####################################################################################################
        elif self.low_sleep < sleep < self.high_sleep and prev_action == "sleep":
            # Get equal Probability for sleep and wake up (either walk or stand)
            state_dict = {"sleep": self.walk_prob}
            action = self.get_action(state_dict)

        ###########################################################################
        #  Walk or stand with weighted probabilities defined in self.get_action() #
        ###########################################################################
        else:
            state_dict = {}
            action = self.get_action(state_dict)

        return self.action2state[action]


class FSMZebraModel:
    """
    This is the FSM implementation of the zebra model. Next state is decided on current state and inputs.
    The following diagram illustrates a simplified version of the model and details will be in self.predict()

                               #######
                               # Run #  <-- From any state
                               #######          ↑
                                |  ↑            |
                ...             |  |    is pred nearby?
                                ↓  |
    ######### --------------> ########  -------------->  #######
    # Drink # is thirst < 30? # Walk #  is hunger < 30?  # Eat #
    ######### <-------------- ########  <--------------  #######
                                |  ↑
      ...       .........       |  |    is sleep < 30?     ...
      ...                       ↓  |                       ...
                              #########
      ...     More States     # Sleep #    More States     ...
                              #########

      ...                        ...                       ...
      ...     More States        ...       More States     ...

    """

    def __init__(self, walk_prob: int = 55, low_hunger: int = 30, high_hunger: int = 80,
                 low_thirst: int = 30, high_thirst: int = 80, low_sleep: int = 30, high_sleep: int = 80):
        """
        Initialize the thresholds for hunger, thirst, and sleep

            @param walk_prob: int The weighted value for the probability of walk/stand
            @param low_hunger: int -- lower threshold of hunger
            @param high_hunger: int -- higher threshold of hunger
            @param low_thirst: int -- lower threshold of thirst
            @param high_thirst: int -- higher threshold of thirst
            @param low_sleep: int -- lower threshold of sleep
            @param high_sleep: int -- higher threshold of sleep
        """

        self.walk_prob = walk_prob

        self.low_hunger = low_hunger
        self.high_hunger = high_hunger

        self.low_thirst = low_thirst
        self.high_thirst = high_thirst

        self.low_sleep = low_sleep
        self.high_sleep = high_sleep

        self.action2state = {"hunger": "eat", "thirst": "drink", "sleep": "sleep", "walk": "walk", "stand": "stand"}

    def get_action(self, states: dict = None):
        """
        Get specific action based on the weighted probability of each action. The probabilities of action
        are normalized to 1 using the corresponding condition values in the state dictionary.

        Example: p("eat") = hunger / sum(hunger + thirst + ... ) --> all values given in state (type: dict)

            @param states: dict {action_name : state_value, ...}
                    Dictionary describing actions and internal state values
                    Ex {"eat": hunger, ...}
            @return: action: str
                    Current action name after choosing with weighted probabilities
        """

        if states is None:
            states = {}
        action_list = list(states.keys())
        action_list.append("walk")

        prob = list(states.values())
        prob.append(self.walk_prob)
        prob = (100-np.array(prob))/np.sum(100-np.array(prob))

        action = np.random.choice(action_list, size=1, p=prob)
        if action == "walk":
            action = np.random.choice(["walk", "stand"], size=1, p=[0.7, 0.3])  # Zebra tends to move more frequently

        return action[0]

    def predict(self, state: dict, prev_action="walk"):
        """
        Forward to next state using the implementation of the FSM described above. Details could be
        found in the Readme.md and link:

            @param state: dict {state_name : state_value, ...}
                    Dictionary describing actions and internal state values
            @param prev_action: str
                    Previous action name
            @return: action: str
                    Current action name that the FSM produced
        """

        # Get all internal state values from the dict
        hunger = state["hunger"]
        thirst = state["thirst"]
        sleep = state["sleep"]

        # Default action to be stand (do nothing)
        action: str = "stand"

        #######################################################
        # Forced actions for state values below low threshold #
        #######################################################

        # Major difference here between Lion Class is that it does not need to catch prey
        # -> Thirst has the highest priority
        if thirst < self.low_thirst:
            action = "thirst"
        elif hunger < self.low_hunger:
            action = "hunger"
        elif sleep < self.low_sleep:
            action = "sleep"

        ###################################################################################################
        # Continue to eat or drink if previous state is eat or drink for states values between thresholds #
        ###################################################################################################
        elif self.low_thirst < thirst < self.high_thirst and prev_action == "drink":
            action = "thirst"
        # elif self.low_thirst < thirst < self.high_thirst and prev_action == "eat":
        #     action = "hunger"

        ##############################################################
        #  Weighted probability for each action for thirst value in  #
        #  between thresholds if previous state is not eat or drink  #
        ##############################################################
        elif self.low_thirst < thirst < self.high_thirst:
            if self.low_hunger < hunger < self.high_hunger:
                if self.low_sleep < sleep < self.high_sleep:
                    # Get Weighted Probability for all three state values between thresholds
                    state_dict = {"hunger": state["hunger"], "thirst": state["thirst"], "sleep": state["sleep"]}
                    action = self.get_action(state_dict)
                else:
                    # Get Weighted Probability for state values between thresholds
                    state_dict = {"hunger": state["hunger"], "thirst": state["thirst"]}
                    action = self.get_action(state_dict)
            elif self.low_sleep < sleep < self.high_sleep:
                # Get Weighted Probability for state values between thresholds
                state_dict = {"thirst": state["thirst"], "sleep": state["sleep"]}
                action = self.get_action(state_dict)
            else:
                # Get Weighted Probability for state values between thresholds
                state_dict = {"thirst": state["thirst"]}
                action = self.get_action(state_dict)

        ########################################################################################
        # Continue to eat if previous state is eat for hunger value between thresholds for eat #
        ########################################################################################
        elif self.low_hunger < hunger < self.high_hunger and prev_action == "eat":
            action = "hunger"

        ##########################################################
        #  Weighted probability for each action for hunger value #
        #  in between thresholds if previous state is not eat    #
        ##########################################################
        elif self.low_hunger < hunger < self.high_hunger:
            if self.low_sleep < sleep < self.high_sleep:
                # Get Weighted Probability for state values between thresholds
                state_dict = {"hunger": state["hunger"], "sleep": state["sleep"]}
                action = self.get_action(state_dict)
            else:
                # Get Weighted Probability for state values between thresholds
                state_dict = {"hunger": state["hunger"]}
                action = self.get_action(state_dict)

        ####################################################################################################
        # Equal probability for continue sleeping or wake up for sleep value in between thresholds for eat #
        ####################################################################################################
        elif self.low_sleep < sleep < self.high_sleep and prev_action == "sleep":
            # Get equal Probability for sleep and wake up (either walk or stand)
            state_dict = {"sleep": self.walk_prob}
            action = self.get_action(state_dict)

        ###########################################################################
        #  Walk or stand with weighted probabilities defined in self.get_action() #
        ###########################################################################
        else:
            state_dict = {}
            action = self.get_action(state_dict)

        return self.action2state[action]
