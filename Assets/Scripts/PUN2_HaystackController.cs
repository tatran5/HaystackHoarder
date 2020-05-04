using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PUN2_HaystackController : MonoBehaviourPun, IPunObservable
{

	//List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
	public MonoBehaviour[] localScripts;
	//List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
	public GameObject[] localObjects;
	//Values that will be synced over network
	Vector3 latestPos;
	Quaternion latestRot;
	public bool deactivated = false;
	public bool becomeActive = false;

	public int hayAmountLeft;
	public int hayAmountInitial;
	public int inactiveHay;

	//In tractor, tell it to do pun2_haystack.decreaseHay vs haystack.decreaseHay
	//In here, create decreaseHay function that makes a PunRPC to decrease hay
	//if name (Haystack 2) is the same
	//OR just stream hayAmountLeft across the network and use that value
    //to determine how to set active - i.e. if active and hayleft is 0 is

	// Use this for initialization
	void Start()
	{
		//if (photonView.IsMine)
		//{
		//	//Player is local

		//}
		//else
		//{
		//	//Player is Remote, deactivate the scripts and object that should only be enabled for the local player
		//	for (int i = 0; i < localScripts.Length; i++)
		//	{
		//		localScripts[i].enabled = false;
		//	}
		//	for (int i = 0; i < localObjects.Length; i++)
		//	{
		//		localObjects[i].SetActive(false);
		//	}
		//}
		Haystack hay = (Haystack)localScripts[0];
		hayAmountLeft = hay.hayAmountLeft;
		hayAmountInitial = hay.hayAmountInitial;

		//foreach (GameObject h in Resources.FindObjectsOfTypeAll<GameObject>())
		//{
		//	if (h.tag == "Haystack" && !h.activeSelf)
		//	{
		//		inactiveHay = h.GetComponent<PhotonView>().ViewID;
		//	}
		//}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			//We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			//Debug.Log("Setting " + photonView.ViewID + " to " + gameObject.activeSelf);
		}
		else
		{
			//Network player, receive data
			latestPos = (Vector3)stream.ReceiveNext();
			latestRot = (Quaternion)stream.ReceiveNext();
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (!photonView.IsMine)
		{
			//Update remote player (smooth this, this looks good, at the cost of some accuracy)
			transform.position = latestPos; // Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
			transform.rotation = latestRot;// Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 5);
		}
	}

    public void adjustStacks()
	{
		Debug.Log("Adjusting stacks");
			foreach (GameObject h in Resources.FindObjectsOfTypeAll<GameObject>())
			{
				if (h.tag == "Haystack" && h.transform.position.y == -3.5f)
				{
					h.transform.position = new Vector3(h.transform.position.x, -0.3352258f, h.transform.position.z);
					gameObject.transform.position = new Vector3(gameObject.transform.position.x, -3.5f, gameObject.transform.position.z);
					hayAmountLeft = hayAmountInitial;
				    break;
						//photonView.RPC("SetActive", RpcTarget.All, photonView.ViewID, false);
				}
			}
	}

    public void callDecrease()
	{
        int ID = photonView.ViewID;
		photonView.RPC("DecreaseHay", RpcTarget.AllViaServer, ID);
	}

	[PunRPC]
	void DecreaseHay(int viewID)
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<PUN2_HaystackController>().hayAmountLeft -= 1;
		if (hayAmountLeft == 0)
		{
			adjustStacks();
		}
	}
}
