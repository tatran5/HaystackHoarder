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

	// Start is called before the first frame update
	void Start()
    {
		if (photonView.IsMine)
		{

		}
		else
		{
			////Player is Remote, deactivate the scripts and object that should only be enabled for the local player
			//for (int i = 0; i < localScripts.Length; i++)
			//{
			//	localScripts[i].enabled = false;
			//}
			//for (int i = 0; i < localObjects.Length; i++)
			//{
			//	localObjects[i].SetActive(false);
			//}
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			//We own this barn: send the others our data

			// //0 = empty, 1 = HasBale, 2 = Processing
   //         if (state == BarnState.Empty)
			//{
			//	stream.SendNext(0);
			//} else if (state == BarnState.HasBale)
			//{
			//	stream.SendNext(1);
			//} else
			//{
			//	stream.SendNext(2);
			//}
		}
		else
		{
			////Network player, receive data
			//float st = (int)stream.ReceiveNext();
   //         if (st == 0)
			//{
			//	state = BarnState.Empty;
			//} else if (st == 1)
			//{
			//	state = BarnState.HasBale;
			//} else
			//{
			//	state = BarnState.Processing;
			//}
		}
	}

	// Update is called once per frame
	void Update()
    {
        if (!photonView.IsMine)
		{

		} else
		{
			//Barn barn = (Barn)localScripts[0];
			//state = barn.state;
			//team = barn.team;
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
		Debug.Log("hello? change state to " + state + " for viewID " + viewID);
		PhotonView target = PhotonView.Find(viewID);
        if (state == 0)
		{
			target.gameObject.GetComponent<PUN2_BarnSync>().state = BarnState.Empty;
			target.gameObject.GetComponent<Barn>().state = BarnState.Empty;
		} else if (state == 1)
		{
			target.gameObject.GetComponent<PUN2_BarnSync>().state = BarnState.HasBale;
			target.gameObject.GetComponent<Barn>().state = BarnState.HasBale;
		} else
		{
			target.gameObject.GetComponent<PUN2_BarnSync>().state = BarnState.Processing;
			target.gameObject.GetComponent<Barn>().state = BarnState.Processing;
		}
	}
}
