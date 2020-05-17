﻿using UnityEngine;
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
	float timeMax = 25f;
	public bool harvestHay;
	float timeHarvest;
	float harvestRequired = 2f;
	public float team = 0;

	public float timeSincePlayerEnter = 0f;
	public static float timeOffsetPlayerEnter = 0.2f;

	public TractorState state = TractorState.Empty;

	public GameObject hayOnTractor;

	// SOUND STARTS HERE ------------
	public AudioClip depleteFuelAC;
	public float depleteFuelVolume;
	AudioSource depleteFuelAS;

	public AudioClip enterTractorAC;
	public float enterTractorVolume = 1f;
	AudioSource enterTractorAS;

	public AudioClip moveTractorAC;
	public float moveTractorVolume = 1f;
	AudioSource moveTractorAS;

	public AudioClip exitTractorAC;
	public float exitTractorVolume = 1f;
	AudioSource exitTractorAS;

	public AudioClip harvestHayAC;
	public float harvestHayVolume = 1f;
	AudioSource harvestHayAS;

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

		enterTractorAS = gameObject.AddComponent<AudioSource>();
		enterTractorAS.clip = enterTractorAC;
		enterTractorAS.volume = enterTractorVolume;

		moveTractorAS = gameObject.AddComponent<AudioSource>();
		moveTractorAS.clip = moveTractorAC;
		moveTractorAS.volume = moveTractorVolume;
		moveTractorAS.loop = true;

		exitTractorAS = gameObject.AddComponent<AudioSource>();
		exitTractorAS.clip = exitTractorAC;
		exitTractorAS.volume = exitTractorVolume;

		harvestHayAS = gameObject.AddComponent<AudioSource>();
		harvestHayAS.clip = harvestHayAC;
		harvestHayAS.volume = harvestHayVolume;
		harvestHayAS.loop = true;

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
		}
		else
		{
			//Network player, receive data
			latestPos = (Vector3)stream.ReceiveNext();
			latestRot = (Quaternion)stream.ReceiveNext();
			Vector3 tempcolor = new Vector3(0.0f, 0.0f, 0.0f);
			stream.Serialize(ref tempcolor);
			gameObject.GetComponent<MeshRenderer>().material.color = new Color(tempcolor.x, tempcolor.y, tempcolor.z, 1.0f);
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
			if (harvestHay)
			{
				progressBar.SetMaxValue(harvestRequired);
				progressBar.SetValue(timeHarvest, harvestRequired);
			}
			else
			{
				progressBar.SetMaxValue(timeMax);
				progressBar.SetValue(timeMax - timeM, timeMax);
			}
		}
		else
		{
			Tractor tractorState = (Tractor)localScripts[0];
			if (harvestHay)
			{
				progressBar.SetMaxValue(tractorState.timeHarvestRequired);
				progressBar.SetValue(tractorState.timeHarvestHay, tractorState.timeHarvestRequired);
			}
			else
			{
				progressBar.SetMaxValue(tractorState.timeMoveMax);
				progressBar.SetValue(tractorState.timeMoveMax - tractorState.timeMove, tractorState.timeMoveMax);
			}
			if (Input.GetKey(KeyCode.LeftShift) && timeSincePlayerEnter >= timeOffsetPlayerEnter)
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
							if (p.team == t.team)
							{
								if (t.playerPos != null)
								{
									Vector3 newPos = t.gameObject.transform.position;
									o.transform.position = new Vector3(newPos.x + 2f, newPos.y, newPos.z);
								}
								o.SetActive(true);
								timeSincePlayerEnter = 0.0f;

								if (state == TractorState.HasHayAndPlayer)
								{
									callChangeState(1);
								}
								else
								{
									callChangeState(0);
								}
							}
						}
					}
				}
			}
			if (state == TractorState.HasHayAndPlayer || state == TractorState.HasPlayerOnly)
			{
				timeSincePlayerEnter += Time.deltaTime;
			}
			else
			{
				timeSincePlayerEnter = 0.0f;
			}
		}
	}


	public void damageTractor()
	{
		Tractor tractorState = (Tractor)localScripts[0];
		if (tractorState.state == TractorState.Empty || tractorState.state == TractorState.HasHayOnly)
		{
			tractorState.RemoveFuel();
			depleteFuelAS.Play();
			callChangeStats(tractorState.timeMove, harvestHay, tractorState.timeHarvestRequired);
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
	public void callChangeStats(float timeM, bool harvestHay, float timeHarvest)
	{
		photonView.RPC("changeStats", RpcTarget.AllViaServer, photonView.ViewID, timeM, harvestHay, timeHarvest);
	}

	public void callPlayHarvestHaySound()
	{
		photonView.RPC("playHarvestHaySound", RpcTarget.AllViaServer);
	}

	[PunRPC]
	public void playHarvestHaySound()
	{
		harvestHayAS.Play();
	}

	public void callStopHarvestHaySound()
	{
		photonView.RPC("stopHarvestHaySound", RpcTarget.AllViaServer);
	}

	[PunRPC]
	public void stopHarvestHaySound()
	{
		harvestHayAS.Stop();
	}

	[PunRPC]
	public void changeStats(int viewID, float timeM, bool harvestHay, float timeHarvest)
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<PUN2_TractorSync>().timeM = timeM;
		target.gameObject.GetComponent<PUN2_TractorSync>().harvestHay = harvestHay;
		target.gameObject.GetComponent<PUN2_TractorSync>().timeHarvest = timeHarvest;
	}

	[PunRPC]
	public void changeState(int viewID, int state)
	{
		PhotonView target = PhotonView.Find(viewID);
		if (state == 0)
		{
			if (target.gameObject.GetComponent<PUN2_TractorSync>().state == TractorState.HasPlayerOnly)
			{
				moveTractorAS.Stop();
				exitTractorAS.Play();
			}
				target.gameObject.GetComponent<PUN2_TractorSync>().state = TractorState.Empty;
			target.gameObject.GetComponent<Tractor>().state = TractorState.Empty;
			photonView.RPC("displayHay", RpcTarget.AllViaServer, viewID, false);
		}
		else if (state == 1)
		{
			if (target.gameObject.GetComponent<PUN2_TractorSync>().state == TractorState.HasHayAndPlayer)
			{
				moveTractorAS.Stop();
				exitTractorAS.Play();
			}
			target.gameObject.GetComponent<PUN2_TractorSync>().state = TractorState.HasHayOnly;
			target.gameObject.GetComponent<Tractor>().state = TractorState.HasHayOnly;
			photonView.RPC("displayHay", RpcTarget.AllViaServer, viewID, true);
		}
		else if (state == 2)
		{
			enterTractorAS.Play();
			moveTractorAS.PlayDelayed(enterTractorAS.clip.length);
			target.gameObject.GetComponent<PUN2_TractorSync>().state = TractorState.HasPlayerOnly;
			target.gameObject.GetComponent<Tractor>().state = TractorState.HasPlayerOnly;
			photonView.RPC("displayHay", RpcTarget.AllViaServer, viewID, false);
		}
		else
		{
			if (target.gameObject.GetComponent<PUN2_TractorSync>().state == TractorState.HasHayOnly)
			{
				enterTractorAS.Play();
				moveTractorAS.PlayDelayed(enterTractorAS.clip.length);
			}
			target.gameObject.GetComponent<PUN2_TractorSync>().state = TractorState.HasHayAndPlayer;
			target.gameObject.GetComponent<Tractor>().state = TractorState.HasHayAndPlayer;
			photonView.RPC("displayHay", RpcTarget.AllViaServer, viewID, true);
		}
	}

	[PunRPC]
	public void displayHay(int viewID, bool active)
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<PUN2_TractorSync>().hayOnTractor.SetActive(active);
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
