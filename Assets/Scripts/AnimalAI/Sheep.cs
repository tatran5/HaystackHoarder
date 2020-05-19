using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : Animal
{

    // Sheep are slow and medium weight.
    // They wander off without much regard for
    // players, 

    int restTimer;
    int restMaxTicks;

    // Start is called before the first frame update
    void Start()
    {
        InitializeBaseVariables();

        speed = 1.0f;
        weight = 4.0f;

        restTimer = 0;
        restMaxTicks = 350;
    }

    protected override void GetWanderDirection()
    {
        if (targetDirection == Vector3.zero)
        {
            restTimer += 1;
            if (restTimer >= restMaxTicks)
            {
                Vector2 currentPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
                float wanderX = Random.Range(-1.0f, 1.0f);
                float wanderZ = Random.Range(-1.0f, 1.0f);
                targetDirection = new Vector3(wanderX, 0.0f, wanderZ);

                float angleToRotate = Mathf.Acos(Vector3.Dot(new Vector3(0, 0, 1), targetDirection.normalized)) * Mathf.Rad2Deg;
                if (targetDirection.x < 0) angleToRotate *= -1;
                transform.eulerAngles = new Vector3(0, angleToRotate, 0);

                // Ray to cast along
                Vector2 targetDir = new Vector2(targetDirection.x, targetDirection.z);
                if (!GetTargetPoint(targetDir))
                {
                    targetDirection = Vector3.zero;
                }
                else
                {
                    restTimer = 0;
                }
            }
        }
    }

}
