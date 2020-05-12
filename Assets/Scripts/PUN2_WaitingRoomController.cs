using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PUN2_WaitingRoomController : MonoBehaviourPunCallbacks, IPunObservable
{
	Text txt;
	// Start is called before the first frame update
	void Start()
	{
		txt = GameObject.Find("Canvas").transform.Find("Text").gameObject.GetComponent<Text>();
	}

	// Update is called once per frame
	void Update()
	{
		txt.text = "#Players = " + PhotonNetwork.PlayerList.Length;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public void onButtonClick()
	{

		int numP = PhotonNetwork.PlayerList.Length;

		photonView.RPC("SwitchScenes", RpcTarget.All, numP);
	}

	public override void OnJoinedRoom()
	{
		txt.text = "#Players = " + PhotonNetwork.PlayerList.Length;

	}

	public override void OnLeftRoom()
	{
		//We have left the Room, return back to the GameLobby
		UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
	}

	[PunRPC]
	void SwitchScenes(int numP)
	{
		if (numP == 1)
		{
			PhotonNetwork.LoadLevel("2PlayerVS");
		}
		else if (numP == 2)
		{
			PhotonNetwork.LoadLevel("2PlayerVS");
		}
	}
}
