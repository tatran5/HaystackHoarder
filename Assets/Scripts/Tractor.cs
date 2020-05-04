﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TractorState { HasHayOnly, HasPlayerOnly, HasHayAndPlayer, Empty}
public class Tractor : ControllableObject
{
    public int team = 0;
	public float timeMoveMax = 5f; // The max time that this tractor can be moved
	public float turnSpeed = 270f;

	public GameObject playerPrefab;
    

    // Fields to prevent player enter and exit the tractor at the same time
    private float timeSincePlayerEnter = 0f;
    public static float timeOffsetPlayerEnter = 0.1f; 

    public float timeMove = 0;
    public float timeHarvestHay = 0f;
	public float timeHarvestRequired = 0f;
	public bool spawnedPlayer = false;

	public TractorState state = TractorState.Empty;

    public Material testTractorMaterial; //TODO: delete this once finish debuggin has hay
    public Material testHasHayMaterial; //TODO: delete this once finish debugging has hay
	public Material T1;
	public Material T2;
	public Material T3;
	public Material T1HasHay;
	public Material T2HasHay;
	public Material T1Dead;
	public Material T2Dead;

	public Vector3 playerPos;

	// Start is called before the first frame update
	void Start()
    {
        speed = 7f;
		timeMoveMax = 25f;
	}

    // Update is called once per frame
    void Update()
    {
		
		if (team == 1)
		{
			if (state == TractorState.HasHayOnly || state == TractorState.HasHayAndPlayer)
			{
				gameObject.GetComponent<MeshRenderer>().material = T1HasHay;
			} else
			{
				gameObject.GetComponent<MeshRenderer>().material = T1;
			}
			if (!HasFuel())
			{
				gameObject.GetComponent<MeshRenderer>().material = T1Dead;
			}
		}
		else if (team == 2)
		{
			if (state == TractorState.HasHayOnly || state == TractorState.HasHayAndPlayer)
			{
				gameObject.GetComponent<MeshRenderer>().material = T2HasHay;
			} else
			{
				gameObject.GetComponent<MeshRenderer>().material = T2;
			}
			if (!HasFuel())
			{
				gameObject.GetComponent<MeshRenderer>().material = T2Dead;
			}
			timeMoveMax = 30f;
		}
		else
		{
			gameObject.GetComponent<MeshRenderer>().material = T3;
		}

        

		if (state == TractorState.HasPlayerOnly || state == TractorState.HasHayAndPlayer)
        {
            timeSincePlayerEnter += Time.deltaTime;

			if (timeMove < timeMoveMax && HandleTractorMovement())
			{
				timeMove += Time.deltaTime;
				gameObject.GetComponent<PUN2_TractorSync>().callChangeStats(timeMove, false, timeHarvestHay);
			}

			// The latter condition prevents player from instantly exit tractor upon entering due to keypress lag
			//if (Input.GetKeyDown(KeyCode.LeftShift) && timeSincePlayerEnter >= timeOffsetPlayerEnter) //LINE MODIFIED BY EVIE
			//	HandlePlayerExitTractor(); // This calls PlayerExitsTractor

			// Handle collision with tractor
			if (Input.GetKey(kbInteract))
            {
                InteractOverTime();
            }
            else
            {
                //progressBar.gameObject.SetActive(false);
                timeHarvestHay = 0f;
            }
        }
		//progressBar.SetValue(timeMoveMax - timeMove, timeMoveMax);
		//Debug.Log("Value: " + (timeMoveMax - timeMove));
	}

	bool HandleTractorMovement()
	{
		if (Input.GetAxisRaw("Vertical") != 0)
		{
			transform.position += Input.GetAxisRaw("Vertical") * Time.deltaTime * speed * transform.forward;
			transform.Rotate(new Vector3(0, Input.GetAxisRaw("Horizontal") * turnSpeed * Time.deltaTime, 0));
			return true;
		}
		return false;
	}


	public override void InteractOverTime()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position,
                    transform.localScale + epsilon, transform.rotation);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject.tag.Equals("Haystack"))
                HarvestHay(colliders[i].gameObject.GetComponent<Haystack>());
        }
    }
     
    private void HarvestHay(Haystack haystack)
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        if (state != TractorState.HasHayAndPlayer)
        {
            if (timeHarvestHay >= haystack.timeHarvestRequired)
            {
                state = TractorState.HasHayAndPlayer;
				gameObject.GetComponent<PUN2_TractorSync>().callChangeState(3);
				timeHarvestHay = 0f;
                haystack.DecreaseHay();
                gameObject.GetComponent<MeshRenderer>().material = testHasHayMaterial; //TODO: delete this after finish debugging
				gameObject.GetComponent<PUN2_TractorSync>().callChangeStats(timeMove, false, timeHarvestHay);
			}
            else
            {
                timeHarvestHay += Time.fixedDeltaTime;
				gameObject.GetComponent<PUN2_TractorSync>().callChangeStats(timeMove, true, timeHarvestHay);
				timeHarvestRequired = haystack.timeHarvestRequired;
			}
        }
    }

	private void HandlePlayerExitTractor()
	{
		Vector3 tractorScale = transform.localScale;
		Vector3 playerScale = playerPrefab.transform.localScale;
		Vector3 offsetScale = Vector3.zero; // offset due to scale

		// additional offset in the each direction when spawning player due to tractor and player's scale
		offsetScale.x += tractorScale.x % 2 == 0 ? 0f : 0.5f;
		offsetScale.z += tractorScale.z % 2 == 0 ? 0f : 0.5f;
		offsetScale.x += playerScale.x % 2 == 0 ? 0f : -0.5f;
		offsetScale.z += playerScale.z % 2 == 0 ? 0f : -0.5f;

		playerPos = new Vector3(0,
			transform.position.y - tractorScale.y / 2f + playerScale.y / 2f + epsilon.y,
			0);
		Vector3 offsetBoundary = Vector3.zero; // offset to avoid spawning a player that collides with this tractor
		spawnedPlayer = false;
		//for (float x = -1; x < tractorScale.x + 1 && !spawnedPlayer; x++)
		//{
		//	offsetBoundary.x = 0f;
		//	if (x == -1) offsetBoundary.x = -epsilon.x;
		//	else if (x == tractorScale.x) offsetBoundary.x = epsilon.x;

		//	//for (float z = -1; z < tractorScale.z + 1 && !spawnedPlayer; z++)
		//	//{
		//		offsetBoundary.z = 0f;
		//		if (z == -1) offsetBoundary.z = -epsilon.z;
		//		else if (z == tractorScale.z) offsetBoundary.z = epsilon.z;

		//		playerPos.x = transform.position.x + 2f;//+ offsetScale.x + offsetBoundary.x + x;
		//												//playerPos.z = transform.position.z + offsetScale.z + offsetBoundary.z + z;

		//		//if (!PlayerOverlapOthers(playerPos, transform.rotation))
		//		//{
		//		//Instantiate(playerPrefab, playerPos, transform.rotation);

		//	//}
		//}

		spawnedPlayer = true;
		timeSincePlayerEnter = 0f;
		if (state == TractorState.HasHayAndPlayer)
		{
			state = TractorState.HasHayOnly;
			gameObject.GetComponent<PUN2_TractorSync>().callChangeState(1);
		}
		else
		{
			state = TractorState.Empty;
			gameObject.GetComponent<PUN2_TractorSync>().callChangeState(0);
		}

		if (!spawnedPlayer)
			Debug.Log("There's not enough space for player to get out off the tractor");
	}

	private bool PlayerOverlapOthers(Vector3 playerPos, Quaternion playerRot)
    {
        Vector3 playerScale = playerPrefab.transform.localScale;
        Collider[] colliders = Physics.OverlapBox(playerPos, playerScale + epsilon, playerRot);
        for (int i = 0; i < colliders.Length; i++)
            if (!colliders[i].gameObject.CompareTag("Ground") &&
                !colliders[i].gameObject.GetComponent<Tractor>())
                return true;
        return false;
    }

    public bool GetHay()
    {
        if (state == TractorState.HasHayOnly)
        {
            state = TractorState.Empty;
			gameObject.GetComponent<PUN2_TractorSync>().callChangeState(0);
			gameObject.GetComponent<MeshRenderer>().material = testTractorMaterial;
            return true;
        }
        return false;
    }

    public bool HasPlayer()
    {
        return state == TractorState.HasHayAndPlayer || state == TractorState.HasPlayerOnly;
    }

    public void PlayerEnter()
    {
        if (state == TractorState.Empty)
		{
			state = TractorState.HasPlayerOnly;
			gameObject.GetComponent<PUN2_TractorSync>().callChangeState(2);
		} else if (state == TractorState.HasHayOnly)
		{
			state = TractorState.HasHayAndPlayer;
			gameObject.GetComponent<PUN2_TractorSync>().callChangeState(3);
		}
	}

    public void RefillFuel()
    {
        timeMove = 0f;
		gameObject.GetComponent<PUN2_TractorSync>().callChangeStats(timeMove, false, timeHarvestHay);
	}

	public void RemoveFuel()
	{
		timeMove = timeMoveMax + 1;
		Debug.Log("Removing fuel! " + timeMove);
	}

	public bool HasFuel()
    {
        return timeMove < timeMoveMax;
    }
}
