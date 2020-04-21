# HaystackHoarder

Unity version used to create this project: 2019.2.13f1

### GLOBAL GRID SYSTEM

The components in a level should be mapped to the grid system described below. Players and animals should be sized such that they take up one square unit of space. While their volume may intersect with other grid cells, their world position will correspond to one square at a time. Ideally, this system should be used for detecting collisions with STATIC components in the world, e.g. obstacles, farm structures, and fences (their location remains the same when they are destroyed / rebuilt).

##Setup

The grid system represented by a one-dimensional array to conserve space. Because of this, cells are marked by a single number that represents the array index used to access its data. Each array element is a boolean that is `true` if the space is empty for characters to walk on, `false` otherwise. This maps out as follows:

![](grid.png)

A `width` by `height` grid will have `height` rows and `width` columns. The row, column, and cell numbers are all zero-indexed. Furthermore, coordinates are treated as (col number, row number). Therefore, the square numbered 4 in the above example is at (4, 0), and the square numbered 50 would be at (1, 7). This can be converted into the array index number using `width` * `row` + `col`, where `row` and `col` are the row and column numbers. 

Lastly, the grid system's worldspace dimensions must be specified by the map creators. The grid assumes that these dimensions will be proportional to the grid's size, such that the unit spaces on the grid are square. Then it can simply calculate the length of a cell's sides. The map center determines where the entire grid is located in world space.

##Helper Functions
- **grid_getCellIndexOfCoords:** returns the index of the cell at the given coordinates
- **grid_getCoordsOfCellIndex:** returns the (col, row) representation of the cell at the given index
- **grid_getCellCoordsOfPos:** returns the (col, row) representation of the cell containing the given position
- **grid_getCellIndexOfPos:** return the index of the cell containing the given position
- **grid_getCenterOfCell:** return the position at the center of the cell at the given index

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
