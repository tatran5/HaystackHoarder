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

    public int penNumber;       // Corresponds with player number, used for
                                // tracking pen fences. Ranges from 1 - 4.
                                // If the animal is running free, penNumber
                                // will be set to 0.
                                
    int checkFencesTickLength;
    int checkFencesTimer;

    public Vector2 targetPoint;
    public Vector3 targetDirection;

    List<Fence> fences;

    bool PositionEquality(Vector2 pos1, Vector2 pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) < 0.25f && Mathf.Abs(pos1.y - pos2.y) < 0.25f;
    }

    // Start is called before the first frame update
    void Start()
    {
        globalObj = GameObject.Find("GlobalObject").GetComponent<Global>();

        feedMeter = 50.0f;
        feedTickLength = 75;
        feedTimer = 0;

        speed = 2.0f;

        checkFencesTickLength = 70;
        checkFencesTimer = 0;

        targetPoint = Vector2.zero;
        targetDirection = Vector3.zero;
        
        Fence[] fenceArray = GameObject.FindObjectsOfType<Fence>();
        foreach (Fence f in fenceArray) {
            if (f.penNumber == penNumber) {
                fences.Add(f);
            }
        }
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

    void FixedUpdate()
    {
        if (penNumber > 0)
        {
            int fenceIndex = CheckForEscape();
            if (fenceIndex > 0)
            {
                GetEscapeDirection();
            }
            else {
                GetIdleDirection();
            }
        }
        else if (targetDirection == Vector3.zero) {
            GetWanderDirection();
        }

        gameObject.transform.position += targetDirection * speed * Time.deltaTime;
    }
    
    // Returns the index of the fence that is broken,
    // if any. If all fences are intact, return -1;
    int CheckForEscape() {
        checkFencesTimer += 1;
        if (checkFencesTimer >= checkFencesTickLength) {
            for (int i = 0; i < fences.Count; i++) {
                if (fences[i].broken) {
                    penNumber = 0;
                    return i;
                }
            }
        }

        return -1;
    }

    void GetEscapeDirection() {

    }

    void GetIdleDirection() {

    }

    void GetWanderDirection() {

        Vector2 currentPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);

        float wanderX = Random.Range(-1.0f, 1.0f);
        float wanderZ = Random.Range(-1.0f, 1.0f);
        targetDirection = new Vector3(wanderX, 0.0f, wanderZ);

        // Ray to cast along
        Vector2 targetDir = new Vector2(targetDirection.x, targetDirection.z);

        Vector2 targetPos = Vector2.zero;
        int targetIndex = globalObj.grid_raycastFromPoint(currentPos, targetDir);
        if (targetIndex >= 0) {
            targetPoint = globalObj.grid_getCenterOfCell(targetIndex);
            targetDirection = new Vector3(targetPoint.x, 0.0f, targetPoint.y)
                              - new Vector3(currentPos.x, 0.0f, currentPos.y);
            targetDirection = targetDirection.normalized;
        } else {
           targetDirection = Vector3.zero;
        }

        if (PositionEquality(targetPoint, currentPos))
        {
            targetDirection = Vector3.zero;
        }
    }

    public void FeedAnimal()
    {
        feedMeter = 100.0f;
        feedTimer = 0;
    }
}
