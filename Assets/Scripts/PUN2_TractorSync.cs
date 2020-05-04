using UnityEngine;
using Photon.Pun;

public class PUN2_TractorSync : MonoBehaviourPun, IPunObservable
{

	//List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
	public MonoBehaviour[] localScripts;
	//List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
	public GameObject[] localObjects;
	//Values that will be synced over network
	public ProgressBar progressBar;
	Vector3 latestPos;
	Quaternion latestRot;
	float timeM;
	float timeMax;
	public bool harvestHay;
	float timeHarvest;
	float harvestRequired;
	public float team = 0;

	public TractorState state = TractorState.Empty;

	public GameObject hayOnTractor;

	// SOUND STARTS HERE ------------
	public AudioClip depleteFuelAC;
	public float depleteFuelVolume;
	AudioSource depleteFuelAS;
	//SOUND ENDS HERE ---------------

	// Use this for initialization
	void Start()
	{
		SetupSound();
		if (photonView.IsMine)
		{
			//Player is local
			Tractor t = (Tractor)gameObject.GetComponent<Tractor>();
			Player p = (Player)gameObject.GetComponent<Player>();
			if (PhotonNetwork.LocalPlayer.NickName == "Player 1")
			{
				t.team = 1;
				team = 1;
				gameObject.transform.position = GameObject.Find("T1SpawnPoint").transform.position;
			}
			else if (PhotonNetwork.LocalPlayer.NickName == "Player 2")
			{
				t.team = 2;
				team = 2;
				gameObject.transform.position = GameObject.Find("T2SpawnPoint").transform.position;
			}
			else
			{
				t.team = 3;
				team = 3;
			}
			SetupProgressBar();
			harvestHay = false;
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
			SetupProgressBar();
		}
	}

	void SetupSound()
	{
		depleteFuelAS = gameObject.AddComponent<AudioSource>();
		depleteFuelAS.clip = depleteFuelAC;
		depleteFuelAS.volume = depleteFuelVolume;
	}

	void SetupProgressBar()
	{
		GameObject canvasGO = transform.GetChild(0).gameObject;
		canvasGO.transform.localScale = new Vector3(
			canvasGO.transform.localScale.x * 1 / transform.localScale.x,
			canvasGO.transform.localScale.y * 1 / transform.localScale.y,
			canvasGO.transform.localScale.z * 1 / transform.localScale.z);
		canvasGO.transform.position = new Vector3(
			canvasGO.transform.position.x,
			canvasGO.transform.position.y * transform.localScale.y,
			canvasGO.transform.position.z);
		progressBar = canvasGO.transform.GetChild(0).gameObject.GetComponent<ProgressBar>();
		progressBar.SetMaxValue(25);
		//progressBar.SetActive(false);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		Tractor tractorState = (Tractor)localScripts[0];
		if (stream.IsWriting)
		{
			//We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			Vector3 tempcolor = new Vector3(gameObject.GetComponent<MeshRenderer>().material.color.r, gameObject.GetComponent<MeshRenderer>().material.color.g, gameObject.GetComponent<MeshRenderer>().material.color.b);
			stream.Serialize(ref tempcolor);
			stream.SendNext(tractorState.timeMove);
			stream.SendNext(tractorState.timeMoveMax);
			stream.SendNext(harvestHay);
			stream.SendNext(tractorState.timeHarvestHay);
			stream.SendNext(tractorState.timeHarvestRequired);
		}
		else
		{
			//Network player, receive data
			latestPos = (Vector3)stream.ReceiveNext();
			latestRot = (Quaternion)stream.ReceiveNext();
			Vector3 tempcolor = new Vector3(0.0f, 0.0f, 0.0f);
			stream.Serialize(ref tempcolor);
			gameObject.GetComponent<MeshRenderer>().material.color = new Color(tempcolor.x, tempcolor.y, tempcolor.z, 1.0f);
			timeM = (float)stream.ReceiveNext();
			timeMax = (float)stream.ReceiveNext();
            harvestHay = (bool)stream.ReceiveNext();
			timeHarvest = (float)stream.ReceiveNext();
			harvestRequired = (float)stream.ReceiveNext();
		}
	}

	// Update is called once per frame
	void Update()
	{
		//
		//Debug.Log("Sending time move : " + tractorState.timeMove + " " + tractorState.timeMoveMax);
		if (!photonView.IsMine)
		{
			//Tractor tractorState = (Tractor)localScripts[0];
			//Update remote player (smooth this, this looks good, at the cost of some accuracy)
			transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 5);
            if (harvestHay)
			{
				progressBar.SetMaxValue(harvestRequired);
				progressBar.SetValue(timeHarvest, harvestRequired);
			} else
			{
				progressBar.SetMaxValue(timeMax);
				progressBar.SetValue(timeMax - timeM, timeMax);
			}
			//Debug.Log("View ID: " + photonView.ViewID + " " + timeM);
		}
		else
		{
			Tractor tractorState = (Tractor)localScripts[0];
            if (harvestHay)
			{
				progressBar.SetMaxValue(tractorState.timeHarvestRequired);
				progressBar.SetValue(tractorState.timeHarvestHay, tractorState.timeHarvestRequired);
				Debug.Log("Amnt: " + tractorState.timeHarvestHay + " Max: " + tractorState.timeHarvestRequired);
			} else
			{
				progressBar.SetMaxValue(tractorState.timeMoveMax);
				progressBar.SetValue(tractorState.timeMoveMax - tractorState.timeMove, tractorState.timeMoveMax);
			}

			//Debug.Log("Fill: " + (tractorState.timeMoveMax - tractorState.timeMove) + " Max: " + tractorState.timeMoveMax);
			if (Input.GetKey(KeyCode.RightShift))
			{
				foreach (GameObject o in Resources.FindObjectsOfTypeAll<GameObject>())
				{
					if (o.tag == "Player")
					{
						if (!o.activeSelf)
						{
							//o.SetActive(true);
							Tractor t = (Tractor)localScripts[0];
							Player p = o.GetComponent<Player>();
							if (p.team == t.team && t.spawnedPlayer)
							{
								if (t.playerPos != null)
								{
									Debug.Log("Looking goood");
									Vector3 newPos = t.gameObject.transform.position;
									o.transform.position = new Vector3(newPos.x + 2f, newPos.y, newPos.z);
								}
								o.SetActive(true);
							}
						}
					}
				}
			}

            if (state == TractorState.HasHayOnly || state == TractorState.HasHayAndPlayer)
			{
				hayOnTractor.SetActive(true);
			} else
			{
				hayOnTractor.SetActive(false);
			}

		}
	}

    public void damageTractor()
	{
		//Debug.Log("We made it to this function! " + viewID + " in " + photonView.ViewID);
		Tractor tractorState = (Tractor)localScripts[0];
		if (tractorState.state == TractorState.Empty || tractorState.state == TractorState.HasHayOnly)
		{
			tractorState.RemoveFuel();
			depleteFuelAS.Play();
		}
	}

    public void callRemoveFuel(int viewID)
	{
		photonView.RPC("RemoveFuel", RpcTarget.AllViaServer, viewID);
	}

	public void callChangeState(int state)
	{
		photonView.RPC("changeState", RpcTarget.AllViaServer, photonView.ViewID, state);
	}

	[PunRPC]
	public void changeState(int viewID, int state)
	{
		Debug.Log("hello? change state to " + state + " for viewID " + viewID);
		PhotonView target = PhotonView.Find(viewID);
		if (state == 0)
		{
			target.gameObject.GetComponent<PUN2_TractorSync>().state = TractorState.Empty;
			target.gameObject.GetComponent<Tractor>().state = TractorState.Empty;
		}
		else if (state == 1)
		{
			target.gameObject.GetComponent<PUN2_TractorSync>().state = TractorState.HasHayOnly;
			target.gameObject.GetComponent<Tractor>().state = TractorState.HasHayOnly;
		}
		else if (state == 2)
		{
			target.gameObject.GetComponent<PUN2_TractorSync>().state = TractorState.HasPlayerOnly;
			target.gameObject.GetComponent<Tractor>().state = TractorState.HasPlayerOnly;
		} else
		{
			target.gameObject.GetComponent<PUN2_TractorSync>().state = TractorState.HasHayAndPlayer;
			target.gameObject.GetComponent<Tractor>().state = TractorState.HasHayAndPlayer;
		}
	}

	[PunRPC]
    public void RemoveFuel(int viewID)
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<PUN2_TractorSync>().damageTractor();
	}

	public void changeColor(Vector3 color)
	{
		photonView.RPC("changeMaterial", RpcTarget.AllViaServer, photonView.ViewID, color);
	}

	[PunRPC]
	void changeMaterial(int viewID, Vector3 rgb)
	{
		PhotonView target = PhotonView.Find(viewID);
        target.gameObject.GetComponent<MeshRenderer>().material.color = new Color(rgb.x, rgb.y, rgb.z, 1.0f);

	}
}
