using UnityEngine;
using Photon.Pun;

public class PUN2_PlayerSync : MonoBehaviourPun, IPunObservable
{

	//List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
	public MonoBehaviour[] localScripts;
	//List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
	public GameObject[] localObjects;
	//Values that will be synced over network
	Vector3 latestPos;
	Quaternion latestRot;
	public bool destroy = false;
	public GameObject hayPossesion;
	public GameObject fuelPossesion;

	public static Vector3 epsilon = new Vector3(0.15f, 0.15f, 0.15f);

	// Use this for initialization
	void Start()
	{
		Debug.Log("Name: " + PhotonNetwork.LocalPlayer.NickName);
		if (photonView.IsMine)
		{
			//Player is local
			Player p = (Player)gameObject.GetComponent<Player>();
            if (PhotonNetwork.LocalPlayer.NickName == "Player 1")
			{
				p.team = 1;
			    gameObject.transform.position = GameObject.Find("P1SpawnPoint").transform.position;
			} else if (PhotonNetwork.LocalPlayer.NickName == "Player 2")
			{
				p.team = 2;
				gameObject.transform.position = GameObject.Find("P2SpawnPoint").transform.position;
			} else
			{
				p.team = 3;
			}

		}
		else
		{
			//Player is Remote, deactivate the scripts and object that should only be enabled for the local player
			for (int i = 0; i < localScripts.Length; i++)
			{
				localScripts[i].enabled = false;
			}
			for (int i = 0; i < localObjects.Length; i++)
			{
				localObjects[i].SetActive(false);
			}
            //disable non-local sphere colliders to distinguish who is attacking whom?
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			//We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(gameObject.activeSelf);
		}
		else
		{
			//Network player, receive data
			latestPos = (Vector3)stream.ReceiveNext();
			latestRot = (Quaternion)stream.ReceiveNext();
			gameObject.SetActive((bool) stream.ReceiveNext());
			//gameObject.GetComponent<MeshRenderer>().material.color = new Color(tempcolor.x, tempcolor.y, tempcolor.z, 1.0f);
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (!photonView.IsMine)
		{
			//Update remote player (smooth this, this looks good, at the cost of some accuracy)
			transform.position = latestPos; //Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
			transform.rotation = latestRot; //Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 5);
		}
		else
		{
			Collider[] colliders = Physics.OverlapBox(transform.position,
					transform.localScale + epsilon, transform.rotation);
			for (int i = 0; i < colliders.Length; i++)
			{
				GameObject collidedObject = colliders[i].gameObject;

                if (collidedObject.tag == "Tractor")
				{
					PUN2_TractorSync tract = (PUN2_TractorSync)collidedObject.GetComponent<PUN2_TractorSync>();
					Player p = (Player)gameObject.GetComponent<Player>();
					if (tract.team != p.team && Input.GetKey(KeyCode.Space))
					{
						tract.callRemoveFuel(collidedObject.GetComponent<PhotonView>().ViewID);
					}
				}

			}

			if (destroy)
			{
				//GameObject lo = localObjects[0];
				//localObjects[0] = null;
				destroy = false;
				photonView.RPC("DoDeath", RpcTarget.All);
				//Destroy(this);
			}

			Player play = (Player)localScripts[0];

			if (play.state == PlayerState.HasHay && Input.GetKey(KeyCode.X))
			{
				play.state = PlayerState.Empty;
			}
		}
	}

    public void callCease(int opponentID, int state)
	{
		photonView.RPC("ceaseAndDesist", RpcTarget.All, opponentID, state);
	}

    public void callChangePlayerState(int state)
	{
		photonView.RPC("changePlayerState", RpcTarget.All, photonView.ViewID, state);
	}

	public void callChangePlayerActions(int viewID, float timeSinceCease, bool action)
	{
		photonView.RPC("changePlayerActions", RpcTarget.All, viewID, timeSinceCease, action);
	}

	[PunRPC]
	void changePlayerActions(int viewID, float timeSinceCease, bool action)
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<Player>().performingAnAction = action;
		target.gameObject.GetComponent<Player>().timeSinceCease = timeSinceCease;
	}

	[PunRPC]
    void ceaseAndDesist(int opponentID, int state)
	{
		PhotonView target = PhotonView.Find(opponentID);
        if (!target.gameObject.GetComponent<Player>().performingAnAction)
		{
			photonView.RPC("stealHayBale", RpcTarget.All, photonView.ViewID, opponentID, state);
		} else
		{
			target.gameObject.GetComponent<Player>().cease = true;
		}
	}

    [PunRPC]
    void stealHayBale(int viewID, int otherID, int oppState)
	{
		PhotonView me = PhotonView.Find(viewID);
        PhotonView opponent = PhotonView.Find(otherID);
        if (opponent.gameObject.GetComponent<Player>().state != PlayerState.Empty)
		{
			me.gameObject.GetComponent<Player>().state = opponent.gameObject.GetComponent<Player>().state;
			opponent.gameObject.GetComponent<Player>().state = PlayerState.Empty;
			photonView.RPC("changePlayerState", RpcTarget.All, opponent.gameObject.GetComponent<PhotonView>().ViewID, 0);
			photonView.RPC("changePlayerState", RpcTarget.All, me.gameObject.GetComponent<PhotonView>().ViewID, oppState);
		}
	}

    [PunRPC]
    void changePlayerState(int viewID, int state)
	{
		PhotonView target = PhotonView.Find(viewID);
        if (state == 0)
		{
            target.gameObject.GetComponent<Player>().state = PlayerState.Empty; //PlayerState.Empty
		} else if (state == 1)
		{
			target.gameObject.GetComponent<Player>().state = PlayerState.HasHay;//PlayerState.HasHay
		}
		else if (state == 2)
		{
			target.gameObject.GetComponent<Player>().state = PlayerState.HasBale;//PlayerState.HasBale
		}
		else if (state == 3)
		{
			target.gameObject.GetComponent<Player>().state = PlayerState.HasFuel;//PlayerState.HasFuel
		}
	}

	[PunRPC]
	void DoDeath()
	{
		Debug.Log("HE DIED");
		gameObject.SetActive(false);
	}


}
