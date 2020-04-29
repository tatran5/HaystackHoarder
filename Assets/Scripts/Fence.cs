using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : MonoBehaviour
{

	Global globalObj;

	public bool broken;
	public float health;
	int breakTimer;
	int breakTickLength;    // Controls how fast health deterioriates over time

	public bool vertical;
	List<int> occupiedCells = new List<int>();  // Saves the indices of the cells
												// that this fence occupies, which
												// s calculated in Global's Start().

	public int penNumber;       // Corresponds with player number, used for
								// tracking pen fences. Ranges from 1 - 4.

	// Start is called before the first frame update
	void Start()
	{
		broken = false;
		health = Random.Range(90.0f, 100.0f);
		breakTimer = 0;
		breakTickLength = 400;

		Vector3 rotation = gameObject.transform.eulerAngles;
		vertical = Mathf.Approximately(rotation.y, 90.0f) ||
						Mathf.Approximately(rotation.y, 270.0f);

		globalObj = GameObject.Find("GlobalObject").GetComponent<Global>();
	}

	// Update is called once per frame
	void Update()
	{
		breakTimer += 1;
		if (breakTimer >= breakTickLength)
		{
			breakTimer = 0;
			health -= 1.0f;
		}

		if (health <= 0.1f)
		{
			BreakFence();
		}
	}

	public void BreakFence()
	{
		broken = true;
		globalObj.grid_setCellsTrue(occupiedCells.ToArray());
		gameObject.GetComponent<PUN2_FenceSync>().Break();
	}

	public void FixFence()
	{
		broken = false;
		health = 100.0f;
		breakTimer = 0;
		globalObj.grid_setCellsFalse(occupiedCells.ToArray());
		gameObject.GetComponentInChildren<Renderer>().enabled = true;
		gameObject.AddComponent<Rigidbody>();
		gameObject.GetComponent<Collider>().enabled = true;
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
