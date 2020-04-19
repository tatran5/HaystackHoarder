using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tractor : ControllableObject
{
    public static float fuelMax = 100f;
    public static float fuelDepletePerSec = 5f;
    public static float timeOffsetPlayerEnter = 0.1f; // offset to prevent player enter and exit the tractor at the same time

    public GameObject playerPrefab;

    private float fuelLeft = fuelMax;
    public float hayAmount = 0f;
    public bool hasPlayer = false;

    private float timeSincePlayerEnter = 0f;
    private float timeHarvestHay = 0f;
    private bool hasHay = false;

    public ProgressBar progressBar;

    public Material testTractorMaterial; //TODO: delete this once finish debuggin has hay
    public Material testHasHayMaterial; //TODO: delete this once finish debugging has hay

    // Start is called before the first frame update
    void Start()
    {
        progressBar.gameObject.SetActive(false);
        speed = 7f;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasPlayer)
        {
            timeSincePlayerEnter += Time.deltaTime;

            if (fuelLeft > 0 && HandleMovement()) fuelLeft -= (Time.deltaTime % 1) * fuelDepletePerSec;

            // The latter condition prevents player from instantly exit tractor upon entering due to keypress lag
            if (Input.GetKeyDown(kbEnterExitTractor) && timeSincePlayerEnter >= timeOffsetPlayerEnter)
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

        if (hayAmount == 0)
        {
            if (timeHarvestHay >= Haystack.timeHarvestRequired)
            {
                hasHay = true;
                timeHarvestHay = 0f;
                haystack.DecreaseHay();
                gameObject.GetComponent<MeshRenderer>().material = testHasHayMaterial; //TODO: delete this after finish debugging
            }
            else
            {
                timeHarvestHay += Time.fixedDeltaTime;
                progressBar.SetValue(timeHarvestHay, Haystack.timeHarvestRequired);
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

        Vector3 playerPos = new Vector3(0,
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

    public bool GetHay()
    {
        if (hasHay)
        {
            hasHay = false;
            gameObject.GetComponent<MeshRenderer>().material = testTractorMaterial;
            return true;
        }
        return false;
    }
}
