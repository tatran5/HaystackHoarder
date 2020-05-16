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

	// AUDIO SETUP STARTS -------------
	public AudioClip hayInteractionAC;
	public float hayInteractionVolume;
	AudioSource hayInteractionAS;

	// OBJECT HOLDING SETUP -----------
	// These are associated with the meshes hidden within player's prefab, not used to be spawn!
	public GameObject hayHeld;
	public GameObject fuelHeld;
	public GameObject baleHeld;

	// OBJECT SPAWN -------------------
	// These are associated with objects to be spawn as player drops object
	public GameObject fuel;

	// Use this for initialization
	void Start()
	{
		SetupSound();
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
		}
	}

	void SetupSound()
	{
		hayInteractionAS = gameObject.AddComponent<AudioSource>();
		hayInteractionAS.clip = hayInteractionAC;
		hayInteractionAS.volume = hayInteractionVolume;
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
			gameObject.SetActive((bool)stream.ReceiveNext());
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (!photonView.IsMine)
		{
			//Update remote player (smooth this, this looks good, at the cost of some accuracy)
			transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 5);
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
				} else if (collidedObject.tag == "Player")
				{
					PUN2_PlayerSync otherPlayerS = (PUN2_PlayerSync)collidedObject.GetComponent<PUN2_PlayerSync>();
					Player otherPlayer = (Player)gameObject.GetComponent<Player>();
					//Debug.Log("STOP PLAYER HAVIVNG ANIMAL FOLLOWING SOMEHOW");
				}

			}

			if (destroy)
			{
				destroy = false;
				photonView.RPC("DoDeath", RpcTarget.All);
			}

			Player play = (Player)localScripts[0];

			if (play.state == PlayerState.HasHay && Input.GetKey(KeyCode.X))
			{
				play.state = PlayerState.Empty;
			}
		}
	}

	public void callFollowAnimal(int viewID)
	{
		photonView.RPC("followAnimal", RpcTarget.AllViaServer, viewID);
	}

	[PunRPC]
	public void followAnimal(int viewID) //viewID of animal
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<Animal>().SetFollowingPlayer(this.gameObject.GetComponent<Player>());
	}

	public void callPlaceAnimal(int viewID)
	{
		photonView.RPC("placeAnimal", RpcTarget.AllViaServer, viewID);
	}

	public void callDropFuel(Vector3 position)
	{
		photonView.RPC("dropFuel", RpcTarget.AllViaServer, position);
	}

	[PunRPC]
	public void dropFuel(Vector3 position)
	{
		GameObject[] droppedFuelGOs = GameObject.FindGameObjectsWithTag("GasCan");
		for (int i = 0; i < droppedFuelGOs.Length; i++)
		{
			GameObject droppedFuelGO = droppedFuelGOs[i];
			if (!droppedFuelGO.activeSelf)
			{
				droppedFuelGO.transform.position = position;
				droppedFuelGO.SetActive(true);
				Debug.Log("HUHUHAWUISDAUIUID");
				return;
			}
		}
		PhotonNetwork.Instantiate(fuel.name, position, transform.rotation, 0);
	}

	[PunRPC]
	public void placeAnimal(int viewID) //viewID of animal
	{
		PhotonView target = PhotonView.Find(viewID);
		Debug.Log("in place animal...");
		target.gameObject.GetComponent<Animal>().SetStopFollowingPlayer();
		gameObject.GetComponent<Player>().animalFollowing = null;
	}

	public void callStopAnimalFollowingOther(int playerID) {
		photonView.RPC("stopAnimalFollowingOther", RpcTarget.All, playerID);
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
	void stopAnimalFollowingOther(int playerID)
	{
		PhotonView targetPlayer = PhotonView.Find(playerID);
		targetPlayer.gameObject.GetComponent<Player>().animalFollowing.isFollowingPlayer = false;
		targetPlayer.gameObject.GetComponent<Player>().animalFollowing = null;
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
		}
		else
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
			photonView.RPC("displayHay", RpcTarget.AllViaServer, viewID, false);
			photonView.RPC("displayBale", RpcTarget.AllViaServer, viewID, false);
			photonView.RPC("displayFuel", RpcTarget.AllViaServer, viewID, false);
		}
		else if (state == 1)
		{
			target.gameObject.GetComponent<Player>().state = PlayerState.HasHay;//PlayerState.HasHay
			photonView.RPC("displayHay", RpcTarget.AllViaServer, viewID, true);
		}
		else if (state == 2)
		{
			target.gameObject.GetComponent<Player>().state = PlayerState.HasBale;//PlayerState.HasBale
			photonView.RPC("displayBale", RpcTarget.AllViaServer, viewID, true);
		}
		else if (state == 3)
		{
			target.gameObject.GetComponent<Player>().state = PlayerState.HasFuel;//PlayerState.HasFuel
			photonView.RPC("displayFuel", RpcTarget.AllViaServer, viewID, true);
		}
	}

	[PunRPC]
	public void displayBale(int viewID, bool active)
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<PUN2_PlayerSync>().baleHeld.SetActive(active);
	}

	[PunRPC]
	public void displayHay(int viewID, bool active)
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<PUN2_PlayerSync>().hayHeld.SetActive(active);
	}

	[PunRPC]
	public void displayFuel(int viewID, bool active)
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<PUN2_PlayerSync>().fuelHeld.SetActive(active);
	}

	[PunRPC]
	void DoDeath()
	{
		gameObject.SetActive(false);
	}


}
