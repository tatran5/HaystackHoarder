using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cow : Animal
{
    // Cows are slow and heavy.
    // They can sense players from farther away than chickens
    // and will start walking away as soon as they sense them.
    // However, their slower speed makes them easier to catch up
    // with. When they are not bothered by a player,
    // they rest in place, just like the sheep.

    float detectionRadius;

    // Start is called before the first frame update
    void Start()
    {
        InitializeBaseVariables();

        speed = 1.0f;
        weight = 4.0f;

        detectionRadius = 8.0f;
    }

    // Update is called once per frame
    protected override void GetWanderDirection()
    {
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, detectionRadius);
        List<GameObject> players = new List<GameObject>();

        foreach (Collider c in hitColliders)
        {
            if (c.tag == "Player")
            {
                players.Add(c.gameObject);
            }
        }

        if (players.Count > 0)
        {
            GetRunAwayDirection(players.ToArray());
        }
    }

    void GetRunAwayDirection(GameObject[] players)
    {
        Vector2 average = Vector2.zero;
        for (int i = 0; i < players.Length; i++)
        {
            Vector2 playerToAnimal = currentPos - new Vector2(players[i].transform.position.x,
                                                              players[i].transform.position.z);
            average += playerToAnimal;
        }

        average /= players.Length;

        float angleRange = Mathf.PI / 8;
        float angle = Random.Range(-angleRange, angleRange);

        Vector2 targetDir = RotateVectorByAngle(average, angle);
        if (!GetTargetPoint(targetDir))
        {
            targetDirection = Vector3.zero;
        }
    }
}
