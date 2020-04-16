using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tractor : MonoBehaviour
{
    public static float speed = 7f;
    public static Vector3 epsilon = new Vector3(0.15f, 0.15f, 0.15f);
    public static float fuelMax = 100f;
    public static float fuelDepletePerSec = 5f;
    public static float timeOffsetPlayerEnter = 0.1f; // offset to prevent player enter and exit the tractor at the same time

    public GameObject playerPrefab;

    private float fuelLeft = fuelMax;
    private bool hasHay = false;
    private bool hasPlayer = false;
    private bool harvestingHay = false;

    private float timeSincePlayerEnter = 0f;
    private float timeHarvestHay = 0f;
    private float hayHarvested = 0f;

    // Start is called before the first frame update
   void Start()
    {
    }

    public void SetHasPlayer(bool hasPlayer)
    {
        this.hasPlayer = hasPlayer;
    }

    public bool GetHasPlayer()
    {
        return hasPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasPlayer)
        {
            timeSincePlayerEnter += Time.deltaTime;
            if (fuelLeft > 0 &&
                (Input.GetKey(Controller.kbMoveLeft) || Input.GetKey(Controller.kbMoveRight) ||
                 Input.GetKey(Controller.kbMoveForward) || Input.GetKey(Controller.kbMoveBackward)))
                HandleMovement();
            if (Input.GetKeyDown(Controller.kbEnterExitTractor) && timeSincePlayerEnter >= timeOffsetPlayerEnter)
                HandlePlayerExitTractor();
            if (hasPlayer && Input.GetKey(Controller.kbInteract))
            {
                harvestingHay = true;
                Collider[] colliders = Physics.OverlapBox(transform.position, 
                    transform.localScale + epsilon, transform.rotation);
                for (int i = 0; i < colliders.Length; i++)
                {
                    Haystack haystack = colliders[i].gameObject.GetComponent<Haystack>();
                    if (haystack) HandleHaystackDetected(haystack);
                }
            }
            if (hasPlayer && Input.GetKeyUp(Controller.kbInteract))
            {
                harvestingHay = false;
                timeHarvestHay = 0f;
            }

        }
    }

    private void HandleHaystackDetected(Haystack haystack)
    {
        Debug.Log("HandleHaystackDetected");
        if (hasPlayer && haystack)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            if (!hasHay)
            {
                if (timeHarvestHay >= Haystack.timeHarvestRequired)
                    {
                        hasHay = true;
                        timeHarvestHay = 0f;
                        haystack.DecreaseHay();
                    }
                    else
                        timeHarvestHay += Time.fixedDeltaTime;
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

        Vector3 playerPos =  new Vector3(0, 
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

               if (!PlayerOverlapOthers(playerPos, transform.rotation)) {
                    Instantiate(playerPrefab, playerPos, transform.rotation);
                    spawnedPlayer = true;
                    hasPlayer = false;
                    timeSincePlayerEnter = 0f;
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

    private void HandleMovement()
    {
        if (Input.GetKey(Controller.kbMoveLeft))
            transform.position -= new Vector3(Time.deltaTime * speed, 0, 0);
        else if (Input.GetKey(Controller.kbMoveRight))
            transform.position += new Vector3(Time.deltaTime * speed, 0, 0);
        if (Input.GetKey(Controller.kbMoveForward))
            transform.position += new Vector3(0, 0, Time.deltaTime * speed);
        else if (Input.GetKey(Controller.kbMoveBackward))
            transform.position -= new Vector3(0, 0, Time.deltaTime * speed);

        fuelLeft -= (Time.deltaTime % 1) * fuelDepletePerSec;
    }
}
