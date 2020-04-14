# HaystackHoarder

Unity version used to create this project: 2019.2.13f1

### Before implementing, if your implementation is related to user input, you might want to take a look at Controller class. This avoids the situation of changing input handling in many different classes when the key binded to an action needs to be changed.

### Tractor
#### Fields
- Whether or not the tractor currently has hay.
- Speed.
- Fuel max.
- Fuel depletion rate. 
- Fuel left. A tractor cannot be moved if it's out of fuel.

#### How player interacts with tractor
- Enter or exit the tractor using shift key
- Pickup hay while in tractor by using space bar when the player is close to tractor
- Pickup hay from the tractor by using space bar when the player is close to tractor
- Fuel depletes whenever player  moves the tractor

### Haystack
#### Fields: 
- Number of times hay can be taken from haystack before haystack disappear (and hence spawn a new one.)
- Amount of time to harvest per hay. Player needs to have no hay currently and collides with haystack while pressing appropriate key for a certain amount of time (field within Haystack) to harvest hay.

#### Scene setup: 
When putting haystacks into scene, it is **important** to have some haystack [deactivate](https://docs.unity3d.com/Manual/DeactivatingGameObjects.html). When an active haystack runs out of hay, it will be set to be inactive, and try to randomly find an inactive haystack in the scene and set it to be active. If there's no haystack in the scene that is inactive, the program will run into an **infinite loop**

#### Potential changes
- The player can harvest hay without actually colliding with the object (just need to be close enough?)
