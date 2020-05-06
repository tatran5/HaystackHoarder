using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : MonoBehaviour
{

	Global globalObj;

	public bool broken;
	public float health;
	public int breakTimer;
	int breakTickLength;    // Controls how fast health deterioriates over time

	public bool vertical;
	List<int> occupiedCells = new List<int>();  // Saves the indices of the cells
												// that this fence occupies, which
												// s calculated in Global's Start().

	public int team;       // Corresponds with player number, used for
								// tracking pen fences. Ranges from 1 - 4.
	public float totalTimeToBreak;
	public float timeToBreak = 0f;
	public float totalTimeToFix;
	public float timeToFix = 0f;

	public bool fixing;


	// Start is called before the first frame update
	void Start()
	{
		fixing = false;
		broken = false;
		//health = 100.0f;
		breakTimer = 0;
		breakTickLength = 300;
		totalTimeToBreak = 0.8f;
		totalTimeToFix = 0.8f;
		Vector3 rotation = gameObject.transform.eulerAngles;
		vertical = Mathf.Approximately(rotation.y, 90.0f) ||
						Mathf.Approximately(rotation.y, 270.0f);

		globalObj = GameObject.Find("GlobalObject").GetComponent<Global>();
	}

	// Update is called once per frame
	void Update()
	{
		breakTimer += 1;
		if (breakTimer >= breakTickLength && !broken)
		{
			breakTimer = 0;
			health -= 1.0f;
		}

		if (health <= 0.1f && !broken)
		{
			BreakFence();
		}
	}

	public void BreakFence()
	{
		broken = true;
		globalObj.grid_setCellsTrue(occupiedCells.ToArray());
		gameObject.GetComponent<PUN2_FenceSync>().Break();
        //for (int i = 1; i <= 3; i++) {
        //    GameObject mesh = gameObject.transform.GetChild(i).gameObject;
        //    MeshRenderer mr = mesh.GetComponent<MeshRenderer>();
        //    mr.enabled = false;
        //}
	}

	public void FixFence()
	{
		globalObj.grid_setCellsFalse(occupiedCells.ToArray());
		gameObject.GetComponent<PUN2_FenceSync>().Fix(broken);
		//for (int i = 1; i <= 3; i++)
		//{
		//    GameObject mesh = gameObject.transform.GetChild(i).gameObject;
		//    MeshRenderer mr = mesh.GetComponent<MeshRenderer>();
		//    mr.enabled = true;
		//}
	}

	public void SetOccupiedCells(List<int> indices)
	{
		occupiedCells = indices;
	}

	public Vector2[] GetEndpoints()
	{
		// Assumes that the fence is either vertically or horizontally aligned
		// with the grid.
		float fenceLength = gameObject.transform.localScale.x;

		Vector2 center = new Vector2(gameObject.transform.position.x,
									 gameObject.transform.position.z);
		Vector2 endpoint1 = Vector2.zero;
		Vector2 endpoint2 = Vector2.zero;

		if (vertical)
		{
			endpoint1 = center - new Vector2(0.0f, fenceLength / 2.0f);
			endpoint2 = center + new Vector2(0.0f, fenceLength / 2.0f);
		}
		else
		{
			endpoint1 = center - new Vector2(fenceLength / 2.0f, 0.0f);
			endpoint2 = center + new Vector2(fenceLength / 2.0f, 0.0f);
		}

		return new Vector2[] { endpoint1, endpoint2 };
	}
}

public class Pen {
    public int teamNumber;
    public List<Fence> fences;
    Vector2 minCorner; // bottom left
    Vector2 maxCorner; // top right

    // not enough time to do this, but ideally would
    // calculate automatically
    public void CalculateCorners() {
        Vector2 minValues = Vector2.zero;
        Vector2 maxValues = Vector2.zero;

        if (fences.Count == 0) {
            return;
        }


        Vector2[] firstFenceEndpts = fences[0].GetEndpoints();
        
        minValues = firstFenceEndpts[0];
        maxValues = firstFenceEndpts[1];

        bool verticalFound = fences[0].vertical;

        for (int i = 1; i < fences.Count; i++) {
            if (fences[i].vertical && !verticalFound
                || !fences[i].vertical && verticalFound) {
                Vector2[] endpts = fences[i].GetEndpoints();

                if (minValues.x > endpts[0].x)
                {
                    minValues.x = endpts[0].x;
                }

                if (maxValues.x < endpts[1].x)
                {
                    maxValues.x = endpts[1].x;
                }

                if (minValues.y > endpts[0].y)
                {
                    minValues.y = endpts[0].y;
                }

                if (maxValues.y < endpts[1].y)
                {
                    maxValues.y = endpts[1].y;
                }
            }
        }
        
        minCorner = minValues;
        maxCorner = maxValues;
    }


    // Assumes that these corner values have been calculated. 
    public bool InsidePenArea(GameObject obj) {
        Vector3 position3D = obj.transform.position;
        Vector2 position2D = new Vector2(position3D.x, position3D.z);
        return minCorner.x <= position2D.x && position2D.x <= maxCorner.x
                && minCorner.y <= position2D.y && position2D.y <= maxCorner.y;
    }

    public bool FencesIntact() {
        return GetBrokenFenceIndex() < 0;
    }

    // returns -1 if fence is intact;
    public int GetBrokenFenceIndex() {
        for (int i = 0; i < fences.Count; i++)
        {
            if (fences[i].broken) return i;
        }

        return -1;
    }
}
