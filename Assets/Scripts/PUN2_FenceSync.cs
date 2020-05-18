﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//display fixing progress
//make sure player 2 can fix
public class PUN2_FenceSync : MonoBehaviourPun, IPunObservable
{
	//List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
	public MonoBehaviour[] localScripts;
	//List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
	public GameObject[] localObjects;
	//Values that will be synced over network
	Vector3 latestPos;
	Quaternion latestRot;

	public int team;
	public bool broken;
	public ProgressBar breakMeter;
	public bool beingBroken = false;

	public bool beingFixed = false;

	public GameObject healthyFence;
	public GameObject tatteredFence;
	public GameObject brokenFence;

	public AudioClip fixFenceAC;
	public float fixFenceVolume = 1f;
	AudioSource fixFenceAS;

	// Start is called before the first frame update
	void Start()
	{
		SetupSound();
		SetupBreakMeter();
	}

	void SetupSound()
	{
		fixFenceAS = gameObject.AddComponent<AudioSource>();
		fixFenceAS.volume = fixFenceVolume;
		fixFenceAS.clip = fixFenceAC;
		fixFenceAS.loop = true;
	}

	void SetupBreakMeter()
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
		breakMeter = canvasGO.transform.GetChild(0).gameObject.GetComponent<ProgressBar>();
		breakMeter.SetActive(false);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (breakMeter.IsActive())
		{
			Fence f = (Fence)localScripts[0];
			if (beingBroken)
			{
				breakMeter.SetMaxValue(f.totalTimeToBreak);
				breakMeter.SetValue(f.timeToBreak, f.totalTimeToBreak);
			}
			if (beingFixed)
			{
				breakMeter.SetMaxValue(f.totalTimeToBreak);
				breakMeter.SetValue(f.timeToFix, f.totalTimeToFix);
			}

		}
		if (beingBroken || beingFixed)
		{
			Fence f = (Fence)localScripts[0];
			breakMeter.SetActive(true);
		}
		else
		{
			breakMeter.SetActive(false);
		}

		Fence fence = (Fence)localScripts[0];
        if (fence.health <= 0f)
		{
			healthyFence.SetActive(false);
			tatteredFence.SetActive(false);
			brokenFence.SetActive(true);
		}
		else if (fence.health < fence.maxHealth / 5)
		{
			healthyFence.SetActive(false);
			tatteredFence.SetActive(true);
			brokenFence.SetActive(false);
		} else
		{
			healthyFence.SetActive(true);
			tatteredFence.SetActive(false);
			brokenFence.SetActive(false);
		}
	}

	public void updateStats(float timeToBreak, bool beingBroken)
	{
		int ID = photonView.ViewID;
		photonView.RPC("updateFenceScript", RpcTarget.AllViaServer, ID, timeToBreak, beingBroken);
	}

	[PunRPC]
	public void updateFenceScript(int viewID, float timeToBreak, bool beingBroken)
	{
		PhotonView target = PhotonView.Find(viewID);
		Fence f = target.GetComponent<Fence>();
		f.timeToBreak = timeToBreak;
		target.GetComponent<PUN2_FenceSync>().beingBroken = beingBroken;
	}

	public void updateFixStats(float timeToBreak, bool beingBroken)
	{
		int ID = photonView.ViewID;
		photonView.RPC("updateFenceScriptFix", RpcTarget.AllViaServer, ID, timeToBreak, beingBroken);
	}

	[PunRPC]
	public void updateFenceScriptFix(int viewID, float timeToFix, bool beingFixed)
	{
		PhotonView target = PhotonView.Find(viewID);
		Fence f = target.GetComponent<Fence>();
		f.timeToFix = timeToFix;
		target.GetComponent<PUN2_FenceSync>().beingFixed = beingFixed;
	}

	public void Break()
	{
		int ID = photonView.ViewID;
		photonView.RPC("BreakFence", RpcTarget.AllViaServer, ID);
	}

	[PunRPC]
	void BreakFence(int viewID)
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponentInChildren<Renderer>().enabled = false;
		target.gameObject.GetComponent<Collider>().enabled = false;
		Destroy(target.gameObject.GetComponent<Rigidbody>());
		broken = true;
		Fence f = target.gameObject.GetComponent<Fence>();
		f.broken = true;
		f.health = 0;
		target.gameObject.GetComponent<PUN2_FenceSync>().beingBroken = false;
	}

	public void Fix(bool wasBroken)
	{
		int ID = photonView.ViewID;
		photonView.RPC("FixFence", RpcTarget.AllViaServer, ID, wasBroken);
	}

	[PunRPC]
	void FixFence(int viewID, bool wasBroken)
	{
		PhotonView target = PhotonView.Find(viewID);
		broken = false;
		Fence f = target.gameObject.GetComponent<Fence>();
		f.breakTimer = 0;
		f.health = f.maxHealth;
		f.broken = false;

		if (wasBroken)
		{
			target.gameObject.GetComponentInChildren<Renderer>().enabled = true;
			target.gameObject.AddComponent<Rigidbody>();
			target.gameObject.GetComponent<Collider>().enabled = true;
			target.gameObject.GetComponent<Rigidbody>().isKinematic = true;
			target.gameObject.GetComponent<Rigidbody>().useGravity = false;
		}
	}
}
