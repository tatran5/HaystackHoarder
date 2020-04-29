using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Global : MonoBehaviour
{
	public static float timeLeft = 420f; //7 minute
	public Text textTimeLeft;

	/* Grid is represented by [row][column].
     * The cell (0, 0) is represented at the bottom left corner of the map,
     * with the (width - 1, height -1) cell at the top right corner.
     * A cell mapGrid[i][j] can be accessed by doing mapGrid[gridSize.x * i + j]
     * A cell is set to true if it can be traveled on, false otherwise.
     * This also assumes that the given map size dimensions are perfectly proportional
     * to the grid's size, such that the cells are square. */

	public Vector2 mapSize;      // world space length of environment
	public Vector2Int gridSize;  // the number of cells along each side of the grid mapped to the environment
	public Vector2 mapCenter;    // world space position of the map's center
	Vector2 mapBottomLeftCorner; // world space position of the bottom left corner of cell (0, 0)
	float cellLength;            // world space length of the side of one cell
	List<bool> grid;

	/* ****************
     * GRID HELPER FUNCTIONS 
     * ****************/

	// converts cell coordinates (x, y), i.e. (col number, row number) -> cell index #
	public int grid_getCellIndexOfCoords(Vector2Int coords)
	{
		if (coords.x < 0 || coords.y < 0 || coords.x >= gridSize.x || coords.y >= gridSize.y)
		{
			return -1;
		}
		return gridSize.x * coords.y + coords.x;
	}

	// converts cell index # -> cell coordinates (col, row)
	public Vector2Int grid_getCoordsOfCellIndex(int cellIndex)
	{
		if (cellIndex < 0 || cellIndex >= gridSize.x * gridSize.y)
		{
			return new Vector2Int(-1, -1);
		}

		return new Vector2Int(cellIndex % gridSize.x, cellIndex / gridSize.x);
	}

	// Returns the coordinates of the cell at any world space position (within bounds)
	public Vector2Int grid_getCellCoordsOfPos(Vector2 pos)
	{
		// Let local grid space be the space where the position (0, 0) is at 
		// the bottom left corner of the grid. Then, we use this to reposition
		// the given point so we can find the correct cell, no matter where in
		// WORLD space the grid / position is.

		Vector2 localPos = new Vector2(pos.x - mapBottomLeftCorner.x, pos.y - mapBottomLeftCorner.y);
		Vector2Int cellCoordinates = new Vector2Int((int)(Mathf.Floor(localPos.x / this.cellLength)),
													(int)(Mathf.Floor(localPos.y / this.cellLength)));
		if (cellCoordinates.x < 0 || cellCoordinates.y < 0 || cellCoordinates.x >= gridSize.x
			|| cellCoordinates.y >= gridSize.y)
		{
			return new Vector2Int(-1, -1);
		}

		return cellCoordinates;
	}

	// Returns the cell # at any position (within bounds)
	public int grid_getCellIndexOfPos(Vector2 pos)
	{
		Vector2Int cellCoordinates = grid_getCellCoordsOfPos(pos);
		int cellNumber = grid_getCellIndexOfCoords(cellCoordinates);
		if (cellNumber < 0 || cellNumber >= gridSize.x * gridSize.y)
		{
			return -1;
		}
		return cellNumber;
	}

	// Assumes cellIndex is a valid index
	public Vector2 grid_getCenterOfCell(int cellIndex)
	{
		Vector2Int cellCoords = grid_getCoordsOfCellIndex(cellIndex);
		return new Vector2(((float)(cellCoords.x) + 0.5f) * cellLength + mapBottomLeftCorner.x,
						   ((float)(cellCoords.y) + 0.5f) * cellLength + mapBottomLeftCorner.y);
	}

	/* Raycasts from the current cell to the cells covered by the
     * direction, testing for cells set to false (blockages). The animal
     * will wander off as far as it can in a direction before wandering to
     * a new one.
     * Implementation is taking from https://theshoemaker.de/2016/02/ray-casting-in-2d-grids/.
     * CURRENT IMPLEMENTATION: Naive
     */

    public int grid_raycastFromPoint(Vector2 origin, Vector2 direction) {
        Vector2 dir = direction.normalized;
        float stepSize = cellLength;
        float tMax = 2 * mapSize.x / 3;

        Vector2 farthestPoint = Vector2.zero;
        int farthestIndex = -1;

        for (float t = 0.0f; t < tMax; t += stepSize) {
            Vector2 p = origin + t * dir;
            int index = grid_getCellIndexOfPos(p);
            if (index < 0) {
                break;
            }

            if (grid[index]) {
                farthestPoint = p;
                farthestIndex = index;
            } else break;
        }

        if (farthestIndex < 0 || grid_getCellIndexOfPos(origin) == farthestIndex) {
            return -1;
        }
        
        return farthestIndex;
    }

    public void grid_setCellsTrue(int[] indices) {
        foreach (int i in indices) {
            if (i > -1) {
                grid[i] = true;
            }
        }
    }

    public void grid_setCellsFalse(int[] indices)
    {
        foreach (int i in indices) {
            if (i > -1) {
                grid[i] = false;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        grid = new List<bool>(gridSize.x * gridSize.y);
        for (int i = 0; i < gridSize.x * gridSize.y; i++) {
            grid.Add(true);
        }

        cellLength = mapSize.x / (float) gridSize.x;
        mapBottomLeftCorner = mapCenter - 0.5f * mapSize;

        // Find all static environment elements and map them on the grid
        MapWalls();
        MapFences();
        MapHaystacks();
        MapBuildings();
    }

    void MapWalls() {
        GameObject[] wallArray = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject w in wallArray) {
            // Assumes that the fence is either vertically or horizontally aligned
            // with the grid.
            float wallLength = w.gameObject.transform.localScale.x;

            Vector2 center = new Vector2(w.gameObject.transform.position.x,
                                         w.gameObject.transform.position.z);
            Vector2 endpoint1 = Vector2.zero;
            Vector2 endpoint2 = Vector2.zero;


            Vector3 rotation = w.gameObject.transform.eulerAngles;
            bool vertical = Mathf.Approximately(rotation.y, 90.0f) ||
                            Mathf.Approximately(rotation.y, 270.0f);

            if (vertical)
            {
                endpoint1 = center - new Vector2(0.0f, wallLength / 2.0f);
                endpoint2 = center + new Vector2(0.0f, wallLength / 2.0f);
            }
            else
            {
                endpoint1 = center - new Vector2(wallLength / 2.0f, 0.0f);
                endpoint2 = center + new Vector2(wallLength / 2.0f, 0.0f);
            }

            Vector2Int endpCoords1 = grid_getCellCoordsOfPos(endpoint1);
            Vector2Int endpCoords2 = grid_getCellCoordsOfPos(endpoint2);

            List<int> indices = new List<int>();

            if (vertical)
            {
                for (int z = endpCoords1.y; z <= endpCoords2.y; z++)
                {
                    indices.Add(grid_getCellIndexOfCoords(new Vector2Int(endpCoords1.x, z)));
                }
            }
            else
            {
                for (int x = endpCoords1.x; x <= endpCoords2.x; x++)
                {
                    indices.Add(grid_getCellIndexOfCoords(new Vector2Int(x, endpCoords1.y)));
                }
            }
            
            grid_setCellsFalse(indices.ToArray());
        }
    }

    void MapFences() {
        Fence[] fences = GameObject.FindObjectsOfType<Fence>();
        foreach (Fence f in fences)
        {

            Vector2[] endpt = f.GetEndpoints();

            Vector2Int endpCoords1 = grid_getCellCoordsOfPos(endpt[0]);
            Vector2Int endpCoords2 = grid_getCellCoordsOfPos(endpt[1]);

            List<int> indices = new List<int>();

            if (f.vertical)
            {
                for (int z = endpCoords1.y; z <= endpCoords2.y; z++)
                {
                    indices.Add(grid_getCellIndexOfCoords(new Vector2Int(endpCoords1.x, z)));
                }
            }
            else
            {
                for (int x = endpCoords1.x; x <= endpCoords2.x; x++)
                {
                    indices.Add(grid_getCellIndexOfCoords(new Vector2Int(x, endpCoords1.y)));
                }
            }

            f.SetOccupiedCells(indices);
            grid_setCellsFalse(indices.ToArray());
        }
    }

    void MapHaystacks() {
        Haystack[] haystackArray = GameObject.FindObjectsOfType<Haystack>();

    }

    void MapBuildings() {
        Barn[] barnArray = GameObject.FindObjectsOfType<Barn>();

        foreach (Barn b in barnArray) {
            Vector2 center = new Vector2(b.gameObject.transform.position.x,
                                         b.gameObject.transform.position.z);
        }

        FuelStation[] fuelStationArray = GameObject.FindObjectsOfType<FuelStation>();

        foreach (FuelStation f in fuelStationArray)
        {
            Vector2 center = new Vector2(f.gameObject.transform.position.x,
                                         f.gameObject.transform.position.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimer();
    }

    // Return false if out of time
    private void UpdateTimer()
    {
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0) textTimeLeft.text = "00:00";
        
        float min = Mathf.Floor(timeLeft / 60);
        float sec = Mathf.RoundToInt(timeLeft % 60);
        string minStr = (min < 10 ? "0" : "") + min.ToString();
        string secStr = (sec < 10 ? "0" : "") + Mathf.RoundToInt(sec).ToString();
        textTimeLeft.text = minStr + ":" + secStr;
    }

    // Use mapSize 14 x 8, gridSize 7 x 4, map center 0 , 0
    private void Test() {
        /* Test print statements*/
        Debug.Log(mapBottomLeftCorner);
        Debug.Log(cellLength);
        Debug.Log("Cell coords of index 25: should be (4, 3), got " + grid_getCoordsOfCellIndex(25));
        Debug.Log("Cell index of coords (5, 2): should be 19, got " + grid_getCellIndexOfCoords(new Vector2Int(5, 2)));
        Debug.Log("Cell coords of position (-5.5, -0.5): should be (0, 1), got " + grid_getCellCoordsOfPos(new Vector2(-5.5f, -0.5f)));
        Debug.Log("Cell coords of position (1.2, 1): should be (4, 2), got" + grid_getCellCoordsOfPos(new Vector2(1.2f, 1.0f)));
        Debug.Log("Cell coords of position (-3.1, 3): should be 22, got " + grid_getCellIndexOfPos(new Vector2(-3.1f, 3.0f)));
        Debug.Log("Cell coords of position (4.2, -1.04): should be 12, got " + grid_getCellIndexOfPos(new Vector2(4.2f, -1.04f)));
        Debug.Log("Center of cell 17: " + grid_getCenterOfCell(17));
        Debug.Log("Raycast from (0.0, 1.0) in direction (-1.0, 0.0): " + grid_raycastFromPoint(new Vector2(0.0f, 1.0f),
                                                                                              new Vector2(-1.0f, 0.0f)));
        Debug.Log("Raycast from (-6.5, -1.0) in direction (1.0, 0.0): " + grid_raycastFromPoint(new Vector2(-6.5f, -1.0f),
                                                                                              new Vector2(1.0f, 0.0f)));
        Debug.Log("Raycast from (-4.5, 3.0) in direction (0.0, 1.0): " + grid_raycastFromPoint(new Vector2(-4.5f, 3.0f),
                                                                                              new Vector2(0.0f, 1.0f)));
        Debug.Log("Raycast from (-4.0, 3.0) in direction (1.0, -1.0): " + grid_raycastFromPoint(new Vector2(-4.0f, 3.0f),
                                                                                              new Vector2(1.0f, -1.0f)));
        grid[9] = false;
        grid[10] = false;

        Debug.Log("Raycast from (-6.5, -1.0) in direction (1.0, 0.0) with blockage: "
                    + grid_raycastFromPoint(new Vector2(-6.5f, -1.0f), new Vector2(1.0f, 0.0f)));

        Debug.Log("Raycast from (-4.0, 3.0) in direction (1.0, -1.0) with blockage: "
                    + grid_raycastFromPoint(new Vector2(-4.0f, 3.0f), new Vector2(1.0f, -1.0f)));
    }
}

