using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chicken : Animal
{

    // Chickens are fast and light.
    // They run from away from players if approached
    // within a short radius. However, it's faster to
    // carry them back to the farm.

    float detectionRadius;

    // Start is called before the first frame update
    void Start()
    {
        InitializeBaseVariables();

        speed = 2.5f;
        weight = 1.0f;
        detectionRadius = 0.5f;

    }

    protected override void GetWanderDirection()
    {
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, detectionRadius);

        foreach (Collider c in hitColliders)
        {
            if (c.tag == "Player")
            {
                GetRunAwayDirection(c.gameObject);
            }
        }

        if (targetDirection == Vector3.zero)
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
        }
    }

    void GetRunAwayDirection(GameObject player)
    {
        Vector2 playerToAnimal = currentPos - new Vector2(player.transform.position.x,
                                                          player.transform.position.z);
        float angleRange = Mathf.PI / 6;
        float angle = Random.Range(-angleRange, angleRange);

        Vector2 targetDir = RotateVectorByAngle(playerToAnimal, angle);
        
        float angleToRotate = Mathf.Acos(Vector3.Dot(new Vector3(0, 0, 1), targetDir.normalized)) * Mathf.Rad2Deg;
        if (targetDir.x < 0) angleToRotate *= -1;
        transform.eulerAngles = new Vector3(0, angleToRotate, 0);

        if (!GetTargetPoint(targetDir))
        {
            targetDirection = Vector3.zero;
        }

    }
}
