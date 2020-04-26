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

    int idleTimer;
    int idleMaxLength;

    public Vector2 targetPoint;
    public Vector3 targetDirection;

    public Vector2 previousPos;
    public Vector2 currentPos;
    public int stuckTimer;
    public int stuckMaxLength; 
    

    List<Fence> fences;

    bool PositionEquality(Vector2 pos1, Vector2 pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) < 0.1f && Mathf.Abs(pos1.y - pos2.y) < 0.1f;
    }

    // Start is called before the first frame update
    void Start()
    {
        globalObj = GameObject.Find("GlobalObject").GetComponent<Global>();

        feedMeter = 50.0f;
        speed = 2.0f;

        feedTickLength = 75;
        checkFencesTickLength = 70;

        feedTimer = 0;
        stuckTimer = 0;
        stuckMaxLength = 100;
        checkFencesTimer = 0;

        targetPoint = Vector2.zero;
        targetDirection = Vector3.zero;

        fences = new List<Fence>();

        Fence[] fenceArray = GameObject.FindObjectsOfType<Fence>();
        foreach (Fence f in fenceArray) {
            if (f.penNumber == penNumber) {
                fences.Add(f);
            }
        }

        previousPos = Vector2.zero;
        currentPos = Vector2.zero;

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
        currentPos.x = gameObject.transform.position.x;
        currentPos.y = gameObject.transform.position.z;

        if (PositionEquality(targetPoint, currentPos))
        {
            targetDirection = Vector3.zero;
        }

        if (penNumber > 0)
        {
            checkFencesTimer += 1;
            if (checkFencesTimer >= checkFencesTickLength)
            {
                checkFencesTimer = 0;

                int fenceIndex = CheckForEscape();
                if (fenceIndex > 0)
                {
                    GetEscapeDirection(fenceIndex);
                    //stuckTimer = 0;
                }
                else if (targetDirection == Vector3.zero)
                {
                    //GetIdleDirection();
                    //stuckTimer = 0;
                }
            }
        }
        else {
            if (targetDirection == Vector3.zero)
            {
                GetWanderDirection();
               // stuckTimer = 0;
            } else if (PositionEquality(previousPos, currentPos)) {
             //   stuckTimer += 1;
            }

            if (stuckTimer >= stuckMaxLength) {
                stuckTimer -= 10;
            }
        }

        gameObject.transform.position += targetDirection * speed * Time.deltaTime;
    }

    bool GetTargetPoint(Vector2 direction)
    {
        int targetIndex = globalObj.grid_raycastFromPoint(currentPos, direction);
        if (targetIndex >= 0)
        {
            targetPoint = globalObj.grid_getCenterOfCell(targetIndex);
            targetDirection = new Vector3(targetPoint.x, 0.0f, targetPoint.y)
                              - new Vector3(currentPos.x, 0.0f, currentPos.y);
            targetDirection = targetDirection.normalized;
            return true;

        }
        else
        {
            return false;
        }
    }

    // Returns the index of the fence that is broken,
    // if any. If all fences are intact, return -1;
    int CheckForEscape() {
        for (int i = 0; i < fences.Count; i++) {
            if (fences[i].broken) {
                penNumber = 0;
                return i;
            }
        }

        return -1;
    }

    void GetEscapeDirection(int index)
    {
        Fence f = fences[index];

        // Calculate the range of angles the animal can move on,
        // choose a random one
        Vector2[] endpoints = f.GetEndpoints();
        Vector2 u = endpoints[0] - currentPos;
        Vector2 v = endpoints[1] - currentPos;

        float angleRange = Mathf.Acos(Vector2.Dot(u, v) / (u.magnitude * v.magnitude));
        angleRange /= 2;

        float angle = Random.Range(-angleRange, angleRange);

        Vector2 animalToFence = new Vector2(f.gameObject.transform.position.x,
                                              f.gameObject.transform.position.z)
                                   - currentPos;

        Vector2 targetDir = new Vector2(animalToFence.x * Mathf.Cos(-angle) - animalToFence.y * Mathf.Sin(-angle),
                                        animalToFence.x * Mathf.Sin(-angle) + animalToFence.y * Mathf.Cos(-angle));

        if (!GetTargetPoint(targetDir)) {
            targetDirection = Vector3.zero;
        }

    }

    void GetIdleDirection()
    {
        float wanderX = Random.Range(-1.0f, 1.0f);
        float wanderZ = Random.Range(-1.0f, 1.0f);
        targetDirection = new Vector3(wanderX, 0.0f, wanderZ);
        
    }

    void GetWanderDirection()
    {
        Vector2 currentPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        float wanderX = Random.Range(-1.0f, 1.0f);
        float wanderZ = Random.Range(-1.0f, 1.0f);
        targetDirection = new Vector3(wanderX, 0.0f, wanderZ);

        // Ray to cast along
        Vector2 targetDir = new Vector2(targetDirection.x, targetDirection.z);
        if (!GetTargetPoint(targetDir)) {
            targetDirection = Vector3.zero;
        }
        
    }

    public void FeedAnimal()
    {
        feedMeter = 100.0f;
        feedTimer = 0;
    }
}
