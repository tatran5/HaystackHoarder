using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{

    // Variables related to the feed meter that depletes with time
    public float feedMeter;
    public int feedTickLength;  // How mHuch time it takes to deplete meter by one.
                                // Altered per animal, can also be altered to affect difficulty.
    int feedTimer;       // Pairs with previous variable, updates with Update()
    
    public float weight;        // Varies per animal
    public float speed;         // Speed at which the animal runs

    public int wanderLength;
    public int wanderTimer;
    public Vector3 wanderDirection;


    // Start is called before the first frame update
    void Start()
    {
        feedMeter = 50.0f;
        feedTickLength = 75;
        feedTimer = 0;

        weight = 10.0f;
        speed = 1.0f;

        wanderLength = 200;
        wanderTimer = 0;
        wanderDirection = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // Feed Meter

        feedTimer += 1;
        if (feedTimer >= feedTickLength) {
            feedMeter -= 1.0f;
            feedTimer = 0;
        }

        // Wander Behavior
        UpdateWander();
    }

    void FeedAnimal() {
        feedMeter = 100.0f;
        feedTimer = 0;
    }

    void UpdateWander() {
        if (wanderDirection == Vector3.zero) {
            float wanderX = Random.Range(-1.0f, 1.0f);
            float wanderZ = Random.Range(-1.0f, 1.0f);
            wanderDirection = new Vector3(wanderX, 0, wanderZ);
            wanderDirection = Vector3.Normalize(wanderDirection);
        }

        wanderTimer += 1;

        if (wanderTimer >= wanderLength)
        {
            wanderDirection = Vector3.zero;
            wanderTimer = 0;
        }
        else {
            gameObject.transform.position += wanderDirection * speed * Time.deltaTime;
        }
    }
    
}
