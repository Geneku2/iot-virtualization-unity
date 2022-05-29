import numpy as np
import scipy as sp
from sklearn import hmm
import sklearn

"""
Decision making procedure: 
Initial state (default: Move)
While game playing:
  1. Check current state and determine next state(select max weighted state)
  2. Perform an action during the current state(select max weighted action from a single array)
"""
class Pred:
	def __init__(self):
  	# self.animal_id = None # int
  	# self.state = None  # str
    # self.action = None # str
    # self.gender = 1     # int, 1 is male, 0 is female
  	# self.health = None # Type: float, if == 0, animal dead
    # self.leader_flag = True # Type: Bool
    
    # self.status = None # Type: 2D array, state : action
    
    # self.hunger = 100
    # self.thirst = 100
    
    # self.isdead = (if health == 0)
    # self.sick = 0
    # self.target = animal_id # Type: int 
    # self.default_state = "Move”	# Type: str

    # transition_prob format:
    # {previous_state : {next_state : weight}}
    self.transition_prob = {
    # maslow's hierarchy of needs
    # when eating/drinking, execute the same action until hunger is 100
    # eat/drink if one of it falls under 20
    # sleep from 11pm to 7am
    # move/rest/social has the same prob
                        "Eat"    : { "Eat"   : (if not prey.isdead or self.hunger < 100) * 100
                        						 "Drink" : min(99, 25 * ceil(60 / self.thirst))
                                     "Move"  : 50
                                     "Rest"  : 50
                                     "Sleep" : (if not daytime) * 101
                                     "Social": 50
                                     "Decision" : (if bad weather or other pred) * 1000
                                   }      
         
                        "Drink"  : { "Drink" : (if thirst not full) * 100
                        						 "Eat"   : min(99, 25 * ceil(60 / self.hunger))
                                     "Move"  : 50
                                     "Rest"  : 50
                                     "Sleep" : (if not daytime) * 101
                                     "Social": 50
                                     "Decision" : (if bad weather or other pred) * 1000
                        					 }

                        "Move"   : { "Drink" : min(99, 25 * ceil(60 / self.thirst))
                                     "Eat"   : min(99, 25 * ceil(60 / self.hunger))
                        			       "Rest"  : 50
                                     "Move"  : 50
                                     "Sleep" : (if not daytime) * 101
                                     "Social": 50
                                     "Decision" : (if bad weather or other pred) * 1000 
                        					 }

                        "Rest"   : { "Drink" : min(99, 25 * ceil(60 / self.thirst))
                        						 "Eat"   : min(99, 25 * ceil(60 / self.hunger))
                        						 "Rest"  : 50
                                     "Move"  : 50
                                     "Sleep" : (if not daytime) * 101
                                     "Social": 50
                                     "Decision" : (if bad weather or other pred) * 1000
                        					 }
    										"Sleep"  : { "Drink" : 0
                        						 "Eat"   : 0
                        						 "Rest"  : 0
                                     "Move"  : (if daytime) * 100
                                     "Sleep" : (if not daytime) * 100
                                     "Social": 0
                                     "Decision" : (if bad weather or other pred) * 1000 
                        					 }
                        "Social" : { "Drink" : min(99, 25 * ceil(60 / self.thirst))
                        						 "Eat"   : min(99, 25 * ceil(60 / self.hunger))
                        						 "Rest"  : 50
                                     "Move"  : 50
                                     "Sleep" : (if not daytime) * 101
                                     "Social": 50
                                     "Decision" : (if bad weather or other pred) * 1000 
                        				   }
                        "Decision": { "Drink" : min(99, 25 * ceil(60 / self.thirst))
                        						 "Eat"   : min(99, 25 * ceil(60 / self.hunger))
                        						 "Rest"  : 50
                                     "Move"  : 50
                                     "Sleep" : (if not daytime) * 101
                                     "Social": 50
                                     "Decision" : (if bad weather or other pred) * 1000 
                        				   }
    									} 
    # emission_prob format:
    # {state : {action : weight}}            
 
    self.emission_prob = {
    											"Eat"   : {"Hunt": (if prey not nearby) * 100} ## TODO: needs to have a find_best_prey that returns the best prey in terms of closest distance and weaknesss
                          				  {"kill": (if prey is  nearby) * 100}
                                 		{"Eat" : (if prey killed) * 100}
                          "Drink" : {"Find": (if water not nearby) * 100}
                          					{"Drink": (if water is nearby) * 100}
    											"Sleep" : {"Find Shelter" : (if shelter not nearby) * 100}
                                    {"Sleep: 50},
                          "Move"	: {"explore": 100}
                          					{"wander": 100}
                                    {"idle": 100}
                          "Rest"	: {"sick": (if self.sick) * 100}
                          					{"stand": 50}
                                    {"lay": 50}
                          "social": {"mate": (if self.need_mate > threshold) * 100}
                          					{"play": 50}
                                    {"fight" 50}
                                    {"lick" : 50}
                                    {"find_leader": (if not within herd radius) * 1000}
                                    {"find_friend": 50}
                                    {"find_family": 0}
													"Decision": {"migrate": (if bad weather) * 100}
                          						{"territory": (if herd number > n) or 50 * 100}
                                      {"defend": (if other pred) * 100}
                       } 
    
    ## TODO: move these features to Animal.py
    """
    self.friend_list = {
    											Member ID: friendship (number)
                          ...
    									 }
                       
    self.herd_member = [Member ID, ...]
    
    if male:
      self.mate_interest = {
															Member ID: mate_interest (number)
                              ...	
                           }
                           
    self.spouse = self.mate_interest > x
    """
    
  
  def mate:
  	if it is male and other males within a certain radius of spouse
    	fight() -> compare power (x)
        if you are larger, you win he leaves and self.spouse.append(female_id)
        
        else, you leave and self.mate_interest[self.spouse_id]  = 0
 
      power decrease by a bit
  	
    if it is female
    	stay with male with highest power
    
  def play(self):
  	randonly select a friend, walk to it, and play together -> friendship increase by x (number depend on play time) 
    
  def fight(self):
    walk to self.target and fight -> friendship decrease by x (number depend on fight time)
    
  def find_friend(self):
  	randomly select a herd member not in friend_list and add it to friend_list and initialize friendship to x (number)
    
  def run(self, animal_id: int): #hmm
    
  	next_state = max(self.transition_prob[self.curr_state], key = lambda k : self.transition_prob[self.curr_state][k])
    self.action = max(self.emission_prob[self.curr_state], key = lambda k: self.emission_prob[self.curr_state][k])
    self.curr_state = next_state 
    
    return self.action
    
    # train use data sets of events running with current probabilities
    
  def forward(self, ):	# forward function
  
  def backward(self,): # backward func
  
  def evaluate(self,): # evaluate func
  
  def learn(self, _, smoothing = 0): # learning func

if __name__ == "__main__":
  # emission = np.array([[0.7, 0], [0.2, 0.3], [0.1, 0.7]])
  # transmission = np.array([ [0, 0, 0, 0], [0.5, 0.8, 0.2, 0], [0.5, 0.1, 0.7, 0], [0, 0.1, 0.1, 0]])
  # data for training
  observations = []
  model = Pred()
  # model.train(observations)
  #print("Model transmission probabilities:\n{}".format(model.transmission_prob))
  #print("Model emission probabilities:\n{}".format(model.emission_prob))
  # Probability of a new sequence
  #new_seq = ['1', '2', '3']
  #print("Finding likelihood for {}".format(new_seq))
  #likelihood = model.likelihood(new_seq)
  #print("Likelihood: {}".format(likelihood))
    