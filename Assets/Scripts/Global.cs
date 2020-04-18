using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{

    /* Grid is represented by row, column.
     * The cell (0, 0) is represented at the bottom left corner of the map,
     * with the (width - 1, height -1) cell at the top right corner.
     * A cell mapGrid[i][j] can be accessed by doing mapGrid[gridSize.x * i + j]
     * A cell is set to true if it can be traveled on, false otherwise.
     * This also assumes that the given map size dimensions are perfectly proportional
     * to the grid's size, such that the cells are square. */

    public Vector2 mapSize;     // world space length of environment
    public Vector2Int gridSize; // the number of cells along each side of the grid mapped to the environment
    public Vector2 mapCenter;   // world space position of the map's center
    float cellLength;           // world space length of the side of one cell
    ArrayList grid;

    // Helper function that returns the cell # at any position (within bounds)
    public int getGridCell(Vector2 pos) {
        Vector2 cellCoordinates = new Vector2(Mathf.Floor(pos.x) / this.cellLength,
                                              Mathf.Floor(pos.y) / this.cellLength);
        int cellNumber = (int) (cellCoordinates.x) * (int) (gridSize.x) + (int) (gridSize.y);
        if (cellNumber < 0 || cellNumber > gridSize.x * gridSize.y) {
            return -1;
        }
        return cellNumber;
    }

    public Vector2 getCenterOfCell(int cellNum) {
        int cellCol = cellNum % gridSize.x;
        int cellRow = (cellNum - cellCol) / gridSize.y;
        return new Vector2(((float)(cellRow) + 0.5f) * cellLength,
            ((float)(cellCol) + 0.5f) * cellLength);
    }

    public int raycastFromPoint(Vector2 pos, Vector2 direction) {
        return -1;
    }

    // Start is called before the first frame update
    void Start()
    {
        grid = new ArrayList(gridSize.x * gridSize.y);
        for (int i = 0; i < gridSize.x * gridSize.y; i++) {
            grid.Add(true);
        }

        cellLength = mapSize.x / (float) gridSize.x;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
