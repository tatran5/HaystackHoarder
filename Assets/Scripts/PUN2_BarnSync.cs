using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PUN2_BarnSync : MonoBehaviourPun, IPunObservable
{
	//List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
	public MonoBehaviour[] localScripts;
	//List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
	public GameObject[] localObjects;
	public BarnState state = BarnState.Empty;
	public int team = 0;

	public GameObject processedHay;

	// Start is called before the first frame update
	void Start()
	{

	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (state == BarnState.HasBale)
		{
			processedHay.SetActive(true);
		}
		else
		{
			processedHay.SetActive(false);
		}

		Barn barn = (Barn)localScripts[0];

		if (state == BarnState.Processing)
		{
			barn.progressBar.SetActive(true);
		}
		else
		{
			barn.progressBar.SetActive(false);
		}
	}

	public void callStealBale(int viewID)
	{
		photonView.RPC("StealBale", RpcTarget.AllViaServer, viewID);
	}

	public void GetBarnBale()
	{
		Barn barn = (Barn)localScripts[0];
		barn.GetBale();
	}

	[PunRPC]
	public void StealBale(int viewID)
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<PUN2_BarnSync>().GetBarnBale();
	}

	public void callChangeState(int state)
	{
		photonView.RPC("changeState", RpcTarget.AllViaServer, photonView.ViewID, state);
	}

	[PunRPC]
	public void changeState(int viewID, int state)
	{
		PhotonView target = PhotonView.Find(viewID);
		if (state == 0)
		{
			target.gameObject.GetComponent<PUN2_BarnSync>().state = BarnState.Empty;
			target.gameObject.GetComponent<Barn>().state = BarnState.Empty;
		}
		else if (state == 1)
		{
			target.gameObject.GetComponent<PUN2_BarnSync>().state = BarnState.HasBale;
			target.gameObject.GetComponent<Barn>().state = BarnState.HasBale;
		}
		else
		{
			target.gameObject.GetComponent<PUN2_BarnSync>().state = BarnState.Processing;
			target.gameObject.GetComponent<Barn>().state = BarnState.Processing;
		}
	}
}
