using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : MonoBehaviour
{

    Global globalObj;


    public bool broken;
    private float health;
    int breakTimer;
    int breakTickLength;    // Controls how fast health deterioriates over time

    List<int> occupiedCells = new List<int>();  // Saves the indices of the cells
                                                // that this fence occupies, which
                                                // s calculated in Global's Start().

    public int penNumber;       // Corresponds with player number, used for
                                // tracking pen fences. Ranges from 1 - 4.

    // Start is called before the first frame update
    void Start()
    {
        broken = false;
        health = 100.0f;
        breakTimer = 0;
        breakTickLength = 200;

        globalObj = GameObject.Find("GlobalObject").GetComponent<Global>();
    }

    // Update is called once per frame
    void Update()
    {
        breakTimer += 1;
        if (breakTimer == breakTickLength) {
            breakTimer = 0;
            health -= 1.0f;
        }

        if (health <= 0.1f) {
            globalObj.grid_setCellsTrue(occupiedCells.ToArray());
            broken = true;
        }
    }

    public void FixFence() {
        broken = false;
        health = 100.0f;
    }
    

    public void SetOccupiedCells(List<int> indices) {
        occupiedCells = indices;
    }
}
