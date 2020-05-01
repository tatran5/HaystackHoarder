using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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

	// Start is called before the first frame update
	void Start()
    {
		if (photonView.IsMine)
		{
			SetupBreakMeter();
		}
		else
		{
			SetupBreakMeter();
		}
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
		//Debug.Log("Break Meter? " + breakMeter);
		breakMeter.SetActive(false);
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
			//Update remote player (smooth this, this looks good, at the cost of some accuracy)
			transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 5);
			//gameObject.GetComponentInChildren<Renderer>().enabled = RendCollEnabled;
			//gameObject.GetComponent<Collider>().enabled = RendCollEnabled;
		}
		else
		{
			//Debug.Log(breakMeter.IsActive());
		}
		if (breakMeter.IsActive())
		{
			Fence f = (Fence)localScripts[0];
			breakMeter.SetMaxValue(f.totalTimeToBreak);
			breakMeter.SetValue(f.timeToBreak, f.totalTimeToBreak);
		}
		if (beingBroken)
		{
			Fence f = (Fence)localScripts[0];
			breakMeter.SetActive(true);
			//Debug.Log("Set to active");
		}
		else
		{
			breakMeter.SetActive(false);
			//Debug.Log("Set to inactive");
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
		Destroy(gameObject.GetComponent<Rigidbody>());
		broken = true;
		Fence f = target.gameObject.GetComponent<Fence>();
		f.broken = true;
		f.health = 0;
		target.gameObject.GetComponent<PUN2_FenceSync>().beingBroken = false;
	}
}
