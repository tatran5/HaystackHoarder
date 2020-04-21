using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    Global globalObj;

    // Variables related to the feed meter that depletes with time
    public float feedMeter;
    public int feedTickLength;  // How mHuch time it takes to deplete meter by one.
                                // Altered per animal, can also be altered to affect difficulty.
    int feedTimer;              // Pairs with previous variable, updates with Update()
    
    public float weight;        // Varies per animal
    public float speed;         // Speed at which the animal runs


    // Variables for naive wander behavior
    int wanderLength;
    int wanderTimer;

    public Vector2 wanderPoint;
    public Vector3 wanderDirection;

    // Start is called before the first frame update
    void Start()
    {
        globalObj = GameObject.Find("GlobalObject").GetComponent<Global>();

        feedMeter = 50.0f;
        feedTickLength = 75;
        feedTimer = 0;

        wanderLength = 200;
        wanderTimer = 0;

        speed = 2.0f;

        wanderPoint = Vector2.zero;
        wanderDirection = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // Update feed Meter
        feedTimer += 1;
        if (feedTimer >= feedTickLength) {
            feedMeter -= 1.0f;
            feedTimer = 0;
        }
    }

    void FixedUpdate() {
        // Wander Behavior
        Move();
    }

    public void FeedAnimal() {
        feedMeter = 100.0f;
        feedTimer = 0;
    }

    bool PositionEquality(Vector2 pos1, Vector2 pos2) {
        return Mathf.Abs(pos1.x - pos2.x) < 0.25f && Mathf.Abs(pos1.y - pos2.y) < 0.25f;
    }

    void Move() {

        Vector2 currentPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);

        if (wanderDirection == Vector3.zero) {
            float wanderX = Random.Range(-1.0f, 1.0f);
            float wanderZ = Random.Range(-1.0f, 1.0f);
            wanderDirection = new Vector3(wanderX, 0.0f, wanderZ);

            // Ray to cast along
            Vector2 wanderDir = new Vector2(wanderDirection.x, wanderDirection.z);

            Vector2 targetPos = Vector2.zero;
            int targetIndex = globalObj.grid_raycastFromPoint(currentPos, wanderDir);
            if (targetIndex >= 0) {
                wanderPoint = globalObj.grid_getCenterOfCell(targetIndex);
                wanderDirection = new Vector3(wanderPoint.x, 0.0f, wanderPoint.y)
                                    - new Vector3(currentPos.x, 0.0f, currentPos.y);
                wanderDirection = wanderDirection.normalized;
            }
            else {
                wanderDirection = Vector3.zero;
            }
        }

        if (PositionEquality(wanderPoint, currentPos))
        {
            wanderDirection = Vector3.zero;
        }
        else {
            gameObject.transform.position += wanderDirection * speed * Time.deltaTime;
        }
    }
    
}
