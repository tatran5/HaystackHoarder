using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : MonoBehaviour
{

    Global globalObj;

    // Controls how fast health deterioriates over time
    float health;
    int breakTimer;
    int breakTickLength;

    // Saves the indices of the cells that this fence occupies,
    // which is calculated in Global's Start() function. This
    // makes setting the cells to true / false a little faster.

    List<int> occupiedCells = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
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
        }
    }

    public void FixFence() {
        health = 100.0f;
    }
    

    public void SetOccupiedCells(List<int> indices) {
        occupiedCells = indices;
    }
}
