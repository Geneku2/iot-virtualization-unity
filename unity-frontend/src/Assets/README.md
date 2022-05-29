# African Savannah

## Project Structure
Note: Some TODO items are things not well understood at the moment and should be looked at to see if anything can be 
improved  
Note: GameObject.Find doesn't find inactive objects, meaning some of the "looking..." clauses in start functions will fail.
    Their purpose is to show what the expected instantiation should be in the editor.  
Scenes:
- The AfricanSavannahPC scene is for the PC version
- The AfricanSavannahVR scene is for the VR version
- The TutorialVR scene is the tutorial for the VR version

### Controls:
All controls defined in Controls.cs (with different versions for PC and VR) and should be kept that way.
This script should be used to consolidate all controls which allows easy enabling and disabling of conflicting controls,
but should rely on other controller scripts to do the heavy lifting.
#### PC:
  [WASD] - movement
  [LShift] - sprint
  [Spacebar] - jump
  [Tab] - open/close the device-animal list.
  [M] - open/close map
    Map:
      Click on position in map to teleport to it
  [LCtrl] - lock/unlock cursor for interacting with the device attachment (upper right) and settings (lower right)
    Device attach list:
      Dropdown UI button opens expanded device scrolllist
      Scroll with scrollwheel while moused over scrolllist to navigate
      Select a device for attachment
      Click on panel (turns green) to enable attachment
      Click on animal to attach device when the panel is green
    Settings UI:
      Click on settings cog in bottom left to open settings menu; pauses game (currently crashes game if animals are present)

#### VR:
- Move with left thumbstick. Turn with right thumbstick
- B - open/close map
- Y - open/close entire UI (Note movement is disabled)
    - Scroll through list with left thumbstick, up and down
    - Switch between device and animal list with left thumbstick, left and right
    - X - Select device for attachment, select animal to teleport to
    - Left trigger/shoulder - If only one of device/animal list open, open the other list filtered by what devices are 
    attached to what animals. If both open, then this button opens the console and LED list for that specific device-animal pair
        - Scroll through console with left thumbstick, LED list with right thumbstick
    - Right trigger/shoulder - Go back/close currently open list
- Right trigger/shoulder - Hold for laser pointer.
    - Point and let go at animal to attach currently selected device
    - Point and let go at location on map to teleport there

### Animals
AnimalType.cs defines the current animal types  
AnimalState.cs defines the possible states the animal can be in  
Note: It would be good if animal types and states can be synced with backend for consistency  

Animal.cs is base animal class that all animals should be extended from.  
AnimalController.cs takes information from Client and calls the appropriate functions in Animal.cs

AnimalTemplates hold the templates for each animal type.

Note: Animals are the least well understood at the moment. Specifically how movement works in the
AnimalController.DoAction function and each \[AnimalType\].Move function

### UI:
The ScrollListController is the base class that includes default behavior for how a scroll rect can be manipulated  
Tutorial on how the scroll list was built here: https://www.youtube.com/watch?v=ZI6DwJtjlBA  
The ScrollListButtonController is a generic class for when the scroll list contains button elements.
It is assumed that each button in the list corresponds to some Unity object such as device or animal and this
class is for manipulating such lists.  
Some differences between the two classes:
- ScrollListController manipulate the scroll bar directly instead since there is not need to
iterate through each item
- ScrollListButtonController iterates through each item in the list, and the scroll bar is adjusted accordingly
- Note: The scrolling methods are only used for the VR version as the EventSystem handles scrolling in the PC version
- The GameObjects in the list themselves are called 'items' while the Unity class objects associated with buttons in
ScrollListButtonController are referred to as 'entries'
- The UIListMasterController is responsible for handling all of the other lists/console elements for PC with a UIListMasterControllerVR version handling the VR things

PCAssets:
- UICanvasPC includes the device attachment in the upper right and the settings gear at the lower right
- UIListCanvasPC includes the device/animal list, console log, and LED list
- PCDeviceAttachScripts are scripts for the device attachment UI

VRAssets:
- UIListCanvasVR is the same thing as above for the VR version.
- There is currently no settings/pause menu for the VR version.

Additional Notes:
- Device.cs should be modified to reflect information about IoT devices received from backend
- Animal.cs may also need to be modified
- DeviceListController.cs should be populated with devices retrieved from backend
- LEDItemTemplate should be changed to some sort of sprite that shows the on/off state of LEDs
- Currently, each of the individual Item.cs scripts (e.g. LEDListItem.cs) do not do anything, but are there if 
functionality needs to be added to individual entries in the list, e.g. toggling the state of a LED item

### LaserShooter
One function draws the laser line, one function notifies the current object that its been hit  
Requires object to implement the ILaserTarget interface.  
Objects can also implement the ILaserColor interface to change the laser color when pointed at

Note: This is essentially a custom event handler. It would be better if VR had an event handler built in and to use that

### Map
MapController controls interactions with the map such as teleporting and also provides functions to go from map coordinates
to world coordinates and viceversa  
MapIconController is responsible for keeping track of the animal icons and player icon  
Uses the icons in AnimalIconTemplate to instantiate the icon for each animal type

There is a separate script for the VR version to handle the user moving away from the map and putting the map in front of the user

Note: Map itself is boring, it would be nice if the background was more interesting/informative

### Rain
Not much known.  
Currently able to receive a list of locations to set rain intensity  
Note: Explore differing intensities based on proximity to rain location

### Client
Receives information from server about animals, send back attached device information  
Responsible for initializing animal list and animal icons on map

#### AnimalSimulationClient
Should be combined with DeviceSimulationClient. Currently does not use movesim's server because the movement commands cause the animals to stutter and teleport around. It is not very stable

#### DeviceSimulationClient
Should be combined with AnimalSimulationClient so that one call to the movesim server is needed. Currently, this client is responsible for sending the server information about what devices are attached to what animals (specifically what sensors are attached) and receiving sensor data for each animal to be displayed to the user.

### Server

#### Local server
Temporary server for sending animal info to game. Replace with actual backend  
More animals can be added by making changes to the \[AnimalType]_NAMES lists at the start of the python scripts

#### Movesim server
The current movesim server is located at https://github.com/mccaesar/iot-virtualization/tree/Movesim-VR-integration
Run the server by navigating to the movesim folder and running "python server.py"

### Terrain
Terrain is currently 10000x10000. Ideally, it is preferred to be only 1000x1000, but shrinking the resolution causes
many other issues because all of the trees and other objects are bunched together, creating a lot of lag. One idea is to 
put boundary walls at 1000x1000 and also shrink the map accordingly.  
Another issue is that the view range is extremely low and should be increased. Animals do not show up unless they are
very close to the player.  
Finally, the trees become billboards at a very close range and does not look nice at all.

### Tutorial
The scripts in the tutorial folder are modified versions of their parent scripts in order to advance the tutorial state.

### Cursor lock/unlock
This only applies to the PC version. The cursor can be unlocked and there are rules to when it is locked again. The script is found at src/Assets/Standard Assets/Characters/FirstPersonCharacter/Scripts/MouseLook.cs 
Rules:
- The user clicks anywhere and there is no gameobject underneath their cursor
- There is a gameobject underneath, but neither it nor its root gameobject has the 'UI' tag

### Networking
The package used to make REST requests is https://github.com/proyecto26/RestClient

#### Testing the login and device retrieval functionality

##### Local testing
Make sure the API backend is running on your machine.
First time setup: Make sure to register a user and add some circuits to the user through a program like Postman. Refer to web backend documentation for the API endpoints here: https://github.com/mccaesar/iot-virtualization/tree/master/services/api-backend (repo access required).

In Login.cs change USE_LOCAL_PATH to true for local testing and make sure the local endpoint is correct. You can also check the 'Localhost' box in the Login scene.
Note the port is hard-coded to 443 and HTTP protocol to 'https://' in ./Scripts/DeviceSimulationClient.cs
and WebSocket protocol to 'wss://' in ./UnitySocketIO/Scripts/SocketIO/NativeSocketIO.cs
which will not work unless you have local certs set up correctly.

Starting from the Login scene, you should be able to navigate to the AfricanSavannah scene and see all of the devices/circuits you have created.
