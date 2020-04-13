using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static float speed = 3f; //dummy speed to test hay harvesting function

    private float timeHarvestHay = 0f;
    private bool hasHay = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Dummy movement for player
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.position -= new Vector3(speed * Time.deltaTime, 0, 0);
        if (Input.GetKey(KeyCode.RightArrow))
            transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            timeHarvestHay = 0;
    }
    private void OnCollisionWithHaystack(Haystack haystack)
    {     
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        if (!hasHay)
        {
            if (Input.GetKey(KeyCode.Space))
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

    private void OnCollisionStay(Collision collision)
    {
        GameObject objectCollided = collision.gameObject;
        if (objectCollided.GetComponent<Haystack>())
        {
            OnCollisionWithHaystack(collision.gameObject.GetComponent<Haystack>());
        }
    }

    private void OnCollisionExit(Collision collision)
    {

        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        timeHarvestHay = 0;
    }
}
