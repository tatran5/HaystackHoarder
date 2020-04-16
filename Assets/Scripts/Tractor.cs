using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tractor : MonoBehaviour
{
    public static float speed = 7f;
    public static float epsilon = 0.15f;
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
        Vector3 tractorScale = transform.localScale;
        Vector3 playerScale = playerPrefab.transform.localScale;
        Vector3 offsetScale = Vector3.zero; // offset due to scale

        // additional offset in the each direction when spawning player due to tractor and player's scale
        offsetScale.x += tractorScale.x % 2 == 0 ? 0f : 0.5f;
        offsetScale.z += tractorScale.z % 2 == 0 ? 0f : 0.5f;
        offsetScale.x += playerScale.x % 2 == 0 ? 0f : -0.5f;
        offsetScale.z += playerScale.z % 2 == 0 ? 0f : -0.5f;

        Vector3 playerPos =  new Vector3(0, 
            transform.position.y - tractorScale.y / 2f + playerScale.y / 2f + epsilon, 
            0);
        Vector3 offsetBoundary = Vector3.zero; // offset to avoid spawning a player that collides with this tractor
        bool spawnedPlayer = false;
        for (float x = -1; x < tractorScale.x + 1 && !spawnedPlayer; x++)
        {
            offsetBoundary.x = 0f;
            if (x == -1) offsetBoundary.x = -epsilon;
            else if (x == tractorScale.x) offsetBoundary.x = epsilon;

            for (float z = -1; z < tractorScale.z + 1 && !spawnedPlayer; z++)
            {
                offsetBoundary.z = 0f;
                if (z == -1) offsetBoundary.z = -epsilon;
                else if (z == tractorScale.z) offsetBoundary.z = epsilon;

                playerPos.x = transform.position.x + offsetScale.x + offsetBoundary.x + x;
                playerPos.z = transform.position.z + offsetScale.z + offsetBoundary.z + z;

               if (!OverlapWithOthers(playerPos, playerScale, transform.rotation)) {
                    Instantiate(playerPrefab, playerPos, transform.rotation);
                    spawnedPlayer = true;
               }
                
            }
        }

        if (!spawnedPlayer)
           Debug.Log("There's not enough space for player to get out off the tractor");
    }

    private bool OverlapWithOthers(Vector3 objectPos, Vector3 objectScale, Quaternion objectRot)
    {
        float radius = epsilon + objectScale.z > objectScale.x ? objectScale.z : objectScale.x;
        Collider[] overlapColliders = Physics.OverlapSphere(objectPos, radius);
        
        // Because the radius is the longest scale of the player in a direction, 
        // Physics.OverlapSphere might return true with tractor as the collider 
        // (even though it's not overlapping.)
        for (int i = 0; i < overlapColliders.Length; i++)
            if (!overlapColliders[i].gameObject.CompareTag("Ground") &&
                !overlapColliders[i].gameObject.GetComponent<Tractor>())
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
