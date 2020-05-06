using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Global : MonoBehaviour
{
    public AudioClip backgroundAC;
    public float backgroundVolume;
    AudioSource backgroundAS;

    public Image endScreenImage;
    public Text[] endScreenFixedText;

    public static float timeLeft = 10f; //7 minute
    public Text textTimeLeft;

    float lastSecond = 420f;
    int scoreTickTimer = 0;
    int scoreTickLength = 4;   // count points every four seconds.
    int timeScorePlayer1 = 0;
    int timeScorePlayer2 = 0;
    public Text textTimeScorePlayer1;
    public Text textTimeScorePlayer2;

    public Text textControls;

    public Text textTimeFinalScorePlayer1;
    public Text textTimeFinalScorePlayer2;
    public Text textBonusScorePlayer1;
    public Text textBonusScorePlayer2;

    public Text textTotalScorePlayer1;
    public Text textTotalScorePlayer2;

    public Text textWinner;

    bool gameEnd = false;

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

    public List<Animal> animals;

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

    public int grid_raycastFromPoint(Vector2 origin, Vector2 direction)
    {
        Vector2 dir = direction.normalized;
        float stepSize = cellLength;
        float tMax = 2 * mapSize.x / 3;

        Vector2 farthestPoint = Vector2.zero;
        int farthestIndex = -1;

        for (float t = 0.0f; t < tMax; t += stepSize)
        {
            Vector2 p = origin + t * dir;
            int index = grid_getCellIndexOfPos(p);
            if (index < 0)
            {
                break;
            }

            if (grid[index])
            {
                farthestPoint = p;
                farthestIndex = index;
            }
            else break;
        }

        if (farthestIndex < 0 || grid_getCellIndexOfPos(origin) == farthestIndex)
        {
            return -1;
        }

        return farthestIndex;
    }

    public void grid_setCellsTrue(int[] indices)
    {
        foreach (int i in indices)
        {
            if (i >= 0)
            {
                grid[i] = true;
            }
        }
    }

    public void grid_setCellsFalse(int[] indices)
    {
        foreach (int i in indices)
        {
            if (i >= 0)
            {
                grid[i] = false;
            }
        }
    }

    // Considers a long rectangular object with its orientation
    // and calculates its world-space endpoints.
    Vector2[] GetEndpointsRectangle(GameObject obj, bool vertical)
    {
        // Assumes that the fence is either vertically or horizontally aligned
        // with the grid.
        float length = obj.gameObject.transform.localScale.x;

        Vector2 center = new Vector2(obj.gameObject.transform.position.x,
                                     obj.gameObject.transform.position.z);

        Vector2 endpoint1 = Vector2.zero;
        Vector2 endpoint2 = Vector2.zero;

        if (vertical)
        {
            endpoint1 = center - new Vector2(0.0f, length / 2.0f);
            endpoint2 = center + new Vector2(0.0f, length / 2.0f);
        }
        else
        {
            endpoint1 = center - new Vector2(length / 2.0f, 0.0f);
            endpoint2 = center + new Vector2(length / 2.0f, 0.0f);
        }

        return new Vector2[] { endpoint1, endpoint2 };
    }

    List<int> GetIndicesRectangle(GameObject obj)
    {

        Vector3 rotation = obj.gameObject.transform.eulerAngles;
        bool vertical = Mathf.Approximately(rotation.y, 90.0f) ||
                        Mathf.Approximately(rotation.y, 270.0f);

        Vector2[] endpt = GetEndpointsRectangle(obj.gameObject, vertical);

        Vector2Int endpCoords1 = grid_getCellCoordsOfPos(endpt[0]);
        Vector2Int endpCoords2 = grid_getCellCoordsOfPos(endpt[1]);

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

        return indices;
    }


    // Considers a square object and calculates the world-space
    // positions of its bottom left and top right corners.
    Vector2[] GetCornersSquare(GameObject obj)
    {
        float xLength = obj.gameObject.transform.localScale.x;
        float zLength = obj.gameObject.transform.localScale.z;

        Vector2 center = new Vector2(obj.gameObject.transform.position.x,
                                     obj.gameObject.transform.position.z);

        Vector2 cornerBottomLeft = center - new Vector2(xLength / 2.0f, zLength / 2.0f);
        Vector2 cornerTopRight = center + new Vector2(xLength / 2.0f, zLength / 2.0f);

        return new Vector2[] { cornerBottomLeft, cornerTopRight };
    }

    List<int> GetIndicesSquare(GameObject obj)
    {
        Vector2[] corners = GetCornersSquare(obj);

        List<int> indices = new List<int>();

        Vector2Int cornerCoords1 = grid_getCellCoordsOfPos(corners[0]);
        Vector2Int cornerCoords2 = grid_getCellCoordsOfPos(corners[1]);

        for (int x = cornerCoords1.x; x <= cornerCoords2.x; x++)
        {
            for (int z = cornerCoords1.y; z <= cornerCoords2.y; z++)
            {
                indices.Add(grid_getCellIndexOfCoords(new Vector2Int(x, z)));
            }
        }

        return indices;
    }

    void SetupSound()
    {
        backgroundAS = gameObject.AddComponent<AudioSource>();
        backgroundAS.clip = backgroundAC;
        backgroundAS.volume = backgroundVolume;
        backgroundAS.loop = true;
        backgroundAS.Play();
    }
    // Start is called before the first frame update
    void Start()
    {
        SetupSound();
        grid = new List<bool>(gridSize.x * gridSize.y);
        for (int i = 0; i < gridSize.x * gridSize.y; i++)
        {
            grid.Add(true);
        }

        cellLength = mapSize.x / (float)gridSize.x;
        mapBottomLeftCorner = mapCenter - 0.5f * mapSize;

        // Find all static environment elements and map them on the grid

        MapFences();
        MapWalls();
        MapHaystacks();
        MapBuildings();

        animals = new List<Animal>();

        Animal[] animalArray = GameObject.FindObjectsOfType<Animal>();
        foreach (Animal a in animalArray)
        {
            animals.Add(a);
        }

        ToggleGameText();
    }

    void MapFences()
    {
        Fence[] fences = GameObject.FindObjectsOfType<Fence>();
        foreach (Fence f in fences)
        {
            List<int> indices = GetIndicesRectangle(f.gameObject);
            f.SetOccupiedCells(indices);
            grid_setCellsFalse(indices.ToArray());
        }
    }

    void MapWalls()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject w in walls)
        {
            List<int> indices = GetIndicesRectangle(w);
            grid_setCellsFalse(indices.ToArray());
        }
    }

    void MapHaystacks()
    {
        Haystack[] haystacks = GameObject.FindObjectsOfType<Haystack>();
        foreach (Haystack h in haystacks)
        {
            List<int> indices = GetIndicesSquare(h.gameObject);
            grid_setCellsFalse(indices.ToArray());
        }
    }

    void MapBuildings()
    {
        Barn[] barns = GameObject.FindObjectsOfType<Barn>();
        foreach (Barn b in barns)
        {
            List<int> indices = GetIndicesSquare(b.gameObject);
            grid_setCellsFalse(indices.ToArray());
        }

        FuelStation[] stations = GameObject.FindObjectsOfType<FuelStation>();
        foreach (FuelStation fs in stations)
        {
            List<int> indices = GetIndicesSquare(fs.gameObject);
            grid_setCellsFalse(indices.ToArray());
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimer();
        UpdateScores();
    }

    // Return false if out of time
    private void UpdateTimer()
    {
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0)
        {
            textTimeLeft.text = "00:00";
            if (!gameEnd)
            {
                EndGame();
            }
        }

        float min = Mathf.Floor(timeLeft / 60);
        float sec = Mathf.RoundToInt(timeLeft % 60);

        string minStr = (min < 10 ? "0" : "") + min.ToString();
        string secStr = (sec < 10 ? "0" : "") + Mathf.RoundToInt(sec).ToString();
        textTimeLeft.text = minStr + ":" + secStr;
    }

    private void UpdateScores()
    {
        if (lastSecond - timeLeft >= 1.0f)
        {
            lastSecond = timeLeft;
            scoreTickTimer += 1;
        }

        if (scoreTickTimer == scoreTickLength)
        {

            scoreTickTimer = 0;

            for (int i = 0; i < animals.Count; i++)
            {
                int playerNumber = animals[i].penNumber;
                float feedMeter = animals[i].feedMeter;

                int points = 0;

                // Determine score
                if (feedMeter >= 90f)
                {
                    points = 25;
                }
                else if (feedMeter >= 70f)
                {
                    points = 20;
                }
                else if (feedMeter >= 50f)
                {
                    points = 15;
                }
                else if (feedMeter >= 30f)
                {
                    points = 10;
                }
                else if (feedMeter >= 10f)
                {
                    points = 5;
                }

                // Determine where score goes
                switch (playerNumber)
                {
                    case 1:
                        timeScorePlayer1 += points;
                        textTimeScorePlayer1.text = "Player 1 Score: " + timeScorePlayer1;
                        break;
                    case 2:
                        timeScorePlayer2 += points;
                        textTimeScorePlayer2.text = "Player 2 Score: " + timeScorePlayer2;
                        break;
                }
            }
        }
    }

    private void EndGame()
    {
        gameEnd = true;
        ToggleEndText();
        CalculateScores();
    }

    private void CalculateScores()
    {
        int timeFinalScore1 = timeScorePlayer1;
        int timeFinalScore2 = timeScorePlayer2;
        textTimeFinalScorePlayer1.text = timeFinalScore1.ToString();
        textTimeFinalScorePlayer2.text = timeFinalScore2.ToString();

        int bonusScorePlayer1 = 0;
        int bonusScorePlayer2 = 0;

        // calculates bonus score
        for (int i = 0; i < animals.Count; i++)
        {
            int playerNumber = animals[i].penNumber;
            float feedMeter = animals[i].feedMeter;

            int points = 0;

            // Determine score
            if (feedMeter >= 90f)
            {
                points = 60;
            }
            else if (feedMeter >= 70f)
            {
                points = 40;
            }
            else if (feedMeter >= 50f)
            {
                points = 20;
            }

            // Determine where score goes
            switch (playerNumber)
            {
                case 1:
                    bonusScorePlayer1 += points;
                    break;
                case 2:
                    bonusScorePlayer2 += points;
                    break;
            }
        }

        textBonusScorePlayer1.text = bonusScorePlayer1.ToString();
        textBonusScorePlayer2.text = bonusScorePlayer2.ToString();

        int finalScorePlayer1 = timeFinalScore1 + bonusScorePlayer1;
        int finalScorePlayer2 = timeFinalScore2 + bonusScorePlayer2;

        textTotalScorePlayer1.text = finalScorePlayer1.ToString();
        textTotalScorePlayer2.text = finalScorePlayer2.ToString();

        if (finalScorePlayer1 > finalScorePlayer2)
        {
            textWinner.text = "Player 1 wins!";
        }
        else if (finalScorePlayer2 > finalScorePlayer1)
        {
            textWinner.text = "Player 2 wins!";
        }
        else
        {
            textWinner.text = "It's a tie!";
        }

    }

    private void ToggleGameText()
    {
        textTimeScorePlayer1.enabled = true;
        textTimeScorePlayer2.enabled = true;
        textTimeLeft.enabled = true;
        textControls.enabled = true;

        foreach (Text t in endScreenFixedText)
        {
            t.enabled = false;
        }

        textTimeFinalScorePlayer1.enabled = false;
        textTimeFinalScorePlayer2.enabled = false;
        textBonusScorePlayer1.enabled = false;
        textBonusScorePlayer2.enabled = false;
        textTotalScorePlayer1.enabled = false;
        textTotalScorePlayer2.enabled = false;
        textWinner.enabled = false;

        endScreenImage.enabled = false;
    }

    private void ToggleEndText()
    {
        textTimeScorePlayer1.enabled = false;
        textTimeScorePlayer2.enabled = false;
        textTimeLeft.enabled = false;
        textControls.enabled = false;

        foreach (Text t in endScreenFixedText)
        {
            t.enabled = true;
        }

        textTimeFinalScorePlayer1.enabled = true;
        textTimeFinalScorePlayer2.enabled = true;
        textBonusScorePlayer1.enabled = true;
        textBonusScorePlayer2.enabled = true;
        textTotalScorePlayer1.enabled = true;
        textTotalScorePlayer2.enabled = true;
        textWinner.enabled = true;

        endScreenImage.enabled = true;
    }
}

