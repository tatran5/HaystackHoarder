using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tractor : MonoBehaviour
{
    public static float speed = 7f;
    public static Vector3 epsilonOffsetSpawnPlayer = new Vector3(0.1f, 0, 0.1f);
    public static float fuelMax = 100f;
    public static float fuelDepletePerSec = 5f;
    
    public GameObject playerPrefab;

    private float fuelLeft = fuelMax;
    private bool hasHay = false;
    private bool hasPlayer = true;

    private float timeHarvestHay = 0f;

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
            if (fuelLeft > 0 &&
                (Input.GetKey(Controller.kbMoveLeft) || Input.GetKey(Controller.kbMoveRight) ||
                 Input.GetKey(Controller.kbMoveForward) || Input.GetKey(Controller.kbMoveBackward)))
                HandleMovement();
            if (Input.GetKeyDown(Controller.kbEnterExitTractor))
                HandlePlayerExitTractor();
        }
    }

    private void HandlePlayerExitTractor()
    {
        float xOffset = 0f;
        float zOffset = 0f;
        
        float tractorScaleX = transform.localScale.x;
        float tractorScaleZ = transform.localScale.z;

        float playerScaleX = playerPrefab.transform.localScale.x;
        float playerScaleZ = playerPrefab.transform.localScale.z;

        // additional offset in the each direction when spawning player due to tractor's scale
        xOffset += tractorScaleX % 2 == 0 ? 0f : 0.5f;
        zOffset += tractorScaleZ % 2 == 0 ? 0f : 0.5f;

        // additional offset in the each direction when spawning player due to player's scale
        xOffset += playerScaleX % 2 == 0 ? 0f : -0.5f;
        zOffset += playerScaleZ % 2 == 0 ? 0f : -0.5f;

        float playerPosYBottom = transform.position.y - transform.localScale.y / 2f;

        float yOffset = 1.6f; //TODO: calculate the appropriate y 
        for (float x = -1; x < tractorScaleX + 1; x++)
        {
            float xOffsetBoundary = 0f;
            if (x == -1) xOffsetBoundary = -epsilonOffsetSpawnPlayer.x;
            if (x == tractorScaleX) xOffsetBoundary = epsilonOffsetSpawnPlayer.x;

            for (float z = -1; z < tractorScaleZ + 1; z++)
            {
                float zOffsetBoundary = 0f;
                if (z == -1) zOffsetBoundary = -epsilonOffsetSpawnPlayer.z;
                if (z == tractorScaleZ ) zOffsetBoundary = epsilonOffsetSpawnPlayer.z;

                float playerPosX = xOffsetBoundary + xOffset + x;
                float playerPosZ = zOffsetBoundary + zOffset + z;
                Vector3 playerPos = new Vector3(playerPosX, yOffset, playerPosZ);

                if (OverlapPlayer(playerPos)) {
                    Debug.Log(playerPos);
                    Instantiate(playerPrefab, playerPos, Quaternion.identity);
                }
                
            }
        }


        Debug.Log("There's no space for player to get out off the tractor");
    }

    private bool OverlapPlayer(Vector3 playerPos)
    {
        // Just in case the player's size in z or x direction is larger than 1
        return true;
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

    private void OnCollisionStay(Collision collision)
    {
        Haystack haystack = collision.gameObject.GetComponent<Haystack>();
        if (hasPlayer && haystack)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            if (!hasHay)
            {
                if (Input.GetKey(Controller.kbInteract))
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
                else
                    timeHarvestHay = 0f;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    { 
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        timeHarvestHay = 0;
    }
}
