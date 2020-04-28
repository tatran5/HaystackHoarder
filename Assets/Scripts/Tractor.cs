﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TractorState { HasHayOnly, HasPlayerOnly, HasHayAndPlayer, Empty}
public class Tractor : ControllableObject
{
    public int team = 0;
    public float timeMoveMax = 5f; // The max time that this tractor can be moved

    public GameObject playerPrefab;
    public ProgressBar progressBar;

    // Fields to prevent player enter and exit the tractor at the same time
    private float timeSincePlayerEnter = 0f;
    public static float timeOffsetPlayerEnter = 0.1f; 

    public float timeMove = 0;
    private float timeHarvestHay = 0f;

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
        SetupProgressBar();
        speed = 7f;
	}

    void SetupProgressBar()
    {
        GameObject canvasGO = transform.GetChild(0).gameObject;
        canvasGO.transform.localScale = new Vector3(
            canvasGO.transform.localScale.x * 1 / transform.localScale.x,
            canvasGO.transform.localScale.y * 1 / transform.localScale.y,
            canvasGO.transform.localScale.z * 1 / transform.localScale.z);
        canvasGO.transform.position = new Vector3(
            canvasGO.transform.position.x,
            canvasGO.transform.position.y * transform.localScale.y,
            canvasGO.transform.position.z);
        progressBar = canvasGO.transform.GetChild(0).gameObject.GetComponent<ProgressBar>();
        progressBar.SetActive(false);
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
		}
		else
		{
			gameObject.GetComponent<MeshRenderer>().material = T3;
		}

        

		if (state == TractorState.HasPlayerOnly || state == TractorState.HasHayAndPlayer)
        {
            progressBar.SetValue(timeMoveMax - timeMove, timeMoveMax);
            timeSincePlayerEnter += Time.deltaTime;

            if (timeMove < timeMoveMax && HandleMovement()) timeMove += Time.deltaTime;

            // The latter condition prevents player from instantly exit tractor upon entering due to keypress lag
            if (Input.GetKeyDown(KeyCode.RightShift) && timeSincePlayerEnter >= timeOffsetPlayerEnter) //LINE MODIFIED BY EVIE
                HandlePlayerExitTractor(); // This calls PlayerExitsTractor

            // Handle collision with tractor
            if (Input.GetKey(kbInteract))
            {
                InteractOverTime();
            }
            else
            {
                progressBar.gameObject.SetActive(false);
                timeHarvestHay = 0f;
            }
        }
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
                timeHarvestHay = 0f;
                haystack.DecreaseHay();
                gameObject.GetComponent<MeshRenderer>().material = testHasHayMaterial; //TODO: delete this after finish debugging
            }
            else
            {
                timeHarvestHay += Time.fixedDeltaTime;
                progressBar.SetValue(timeHarvestHay, haystack.timeHarvestRequired);
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
        bool spawnedPlayer = false;
        for (float x = -1; x < tractorScale.x + 1 && !spawnedPlayer; x++)
        {
            offsetBoundary.x = 0f;
            if (x == -1) offsetBoundary.x = -epsilon.x;
            else if (x == tractorScale.x) offsetBoundary.x = epsilon.x;

            for (float z = -1; z < tractorScale.z + 1 && !spawnedPlayer; z++)
            {
                offsetBoundary.z = 0f;
                if (z == -1) offsetBoundary.z = -epsilon.z;
                else if (z == tractorScale.z) offsetBoundary.z = epsilon.z;

                playerPos.x = transform.position.x + offsetScale.x + offsetBoundary.x + x;
                playerPos.z = transform.position.z + offsetScale.z + offsetBoundary.z + z;

                if (!PlayerOverlapOthers(playerPos, transform.rotation))
                {
                    //Instantiate(playerPrefab, playerPos, transform.rotation);
                    spawnedPlayer = true;
                    timeSincePlayerEnter = 0f;
                    if (state == TractorState.HasHayAndPlayer)
                        state = TractorState.HasHayOnly;
                    else
                        state = TractorState.Empty;
                }
            }
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
            state = TractorState.HasPlayerOnly;
        else if (state == TractorState.HasHayOnly)
            state = TractorState.HasHayAndPlayer;
        progressBar.SetActive(true);
        progressBar.SetValue(timeMoveMax - timeMove, timeMoveMax);
    }

    public void RefillFuel()
    {
        timeMove = 0f;
    }

    public bool HasFuel()
    {
        return timeMove < timeMoveMax;
    }
}
