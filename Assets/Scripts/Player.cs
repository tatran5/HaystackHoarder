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
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("Player::OnCollisionStay");
        GameObject objectCollided = collision.gameObject;
        if (objectCollided.name.Equals("Haystack") && Input.GetKey(KeyCode.Space) && !hasHay)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
            collision.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            collision.gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
            if (timeHarvestHay >= Haystack.timeHarvestRequired)
            {
                Debug.Log("Player::OnCollisionStay: about to spawn new haystack");

                hasHay = true;
                objectCollided.GetComponent<Haystack>().decreaseHay();
                timeHarvestHay = 0;
            }
            else
            {
                Debug.Log("Player::OnCollisionStay: update time");
                timeHarvestHay += Time.fixedDeltaTime;
                Debug.Log("Time left: " + (Haystack.timeHarvestRequired - timeHarvestHay).ToString());
            }

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        GameObject objectCollided = collision.gameObject;
        if (objectCollided.name.Equals("Haystack") && Input.GetKey(KeyCode.Space) && !hasHay)
        {
            timeHarvestHay = 0;
        }
    }
}
