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

	// Use this for initialization
	void Start()
	{
		Haystack hay = (Haystack)localScripts[0];
		hayAmountLeft = hay.hayAmountLeft;
		hayAmountInitial = hay.hayAmountInitial;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			//We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
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
			//Update remote player
			transform.position = latestPos;
			transform.rotation = latestRot;
		}
	}

	public void adjustStacks()
	{
		foreach (GameObject h in Resources.FindObjectsOfTypeAll<GameObject>())
		{
			if (h.tag == "Haystack" && h.transform.position.y == -3.5f)
			{
				h.transform.position = new Vector3(h.transform.position.x, -0.3352258f, h.transform.position.z);
				gameObject.transform.position = new Vector3(gameObject.transform.position.x, -3.5f, gameObject.transform.position.z);
				hayAmountLeft = hayAmountInitial;
				break;
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
