using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PUN2_RoomController : MonoBehaviourPunCallbacks
{

	//Player instance prefab, must be located in the Resources folder
	public GameObject playerPrefab;
	public GameObject haystackPrefab;
	public GameObject tractorPrefab;
	//Player spawn point
	public List<Transform> playerSpawnPoints;
	public List<Transform> haystackSpawnPoints;
	public List<Transform> tractorSpawnPoints;

	public int playerCount = 0;

	// Use this for initialization
	void Start()
	{
		//In case we started this demo with the wrong scene being active, simply load the menu scene
		if (PhotonNetwork.CurrentRoom == null)
		{
			Debug.Log("Is not in the room, returning back to Lobby");
			UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
			return;
		}

		//We're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
		PhotonNetwork.Instantiate(playerPrefab.name, playerSpawnPoints[PhotonNetwork.PlayerList.Length - 1].position, Quaternion.identity, 0);
		//one tractor per player entering the room
		PhotonNetwork.Instantiate(tractorPrefab.name, tractorSpawnPoints[PhotonNetwork.PlayerList.Length - 1].position, Quaternion.identity, 0);
		//PhotonNetwork.Instantiate(haystackPrefab.name, haystackSpawnPoints[PhotonNetwork.PlayerList.Length - 1].position, Quaternion.identity, 0);

		
		//set team number for incoming player and tractor
		//foreach (GameObject o in Resources.FindObjectsOfTypeAll<GameObject>())
		//{
		//	if (o.tag == "Player")
		//	{
		//		Player p = (Player)o.GetComponent<Player>();
		//		if (p != null)
		//		{
		//			if (p.team == 0)
		//			{
		//				p.team = PhotonNetwork.PlayerList.Length;
		//				Debug.Log("p.team: " + p.team + " vs: " + PhotonNetwork.PlayerList.Length);
		//			}
		//		}
		//	}
		//	else if (o.tag == "Tractor")
		//	{
		//		Tractor t = (Tractor)o.GetComponent<Tractor>();
		//		if (t != null)
		//		{
		//			if (t.team == 0)
		//			{
		//				t.team = PhotonNetwork.PlayerList.Length;
		//			}
		//		}
		//	}
		//}


		//for (int i = 0; i < tractorSpawnPoints.Count; i++)
		//{
		//	PhotonNetwork.Instantiate(tractorPrefab.name, tractorSpawnPoints[i].position, Quaternion.identity, 0);
		//}
	}

	void OnJoinedRoom()
	{
		photonView.RPC("UpdatePlayerCount", RpcTarget.All, true);
		
	}

	void Update()
	{
  //      for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
		//{
		//	GameObject person = PhotonNetwork.PlayerList[i];
		//	Player scr = person.GetComponent<Player>();
		//}
	}

	void OnGUI()
	{
		if (PhotonNetwork.CurrentRoom == null)
			return;

		//Leave this Room
		if (GUI.Button(new Rect(5, 5, 125, 25), "Leave Room"))
		{
			PhotonNetwork.LeaveRoom();
		}

		//Show the Room name
		GUI.Label(new Rect(135, 5, 200, 25), PhotonNetwork.CurrentRoom.Name);

		//Show the list of the players connected to this Room
		for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
		{
			//Show if this player is a Master Client. There can only be one Master Client per Room so use this to define the authoritative logic etc.)
			string isMasterClient = (PhotonNetwork.PlayerList[i].IsMasterClient ? ": MasterClient" : "");
			GUI.Label(new Rect(5, 35 + 30 * i, 200, 25), PhotonNetwork.PlayerList[i].NickName + isMasterClient);
		}
	}

	public override void OnLeftRoom()
	{
		//We have left the Room, return back to the GameLobby
		UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
	}

	[PunRPC]
	void UpdatePlayerCount(bool AddToCount)
	{
		if (AddToCount)
		{
			playerCount += 1;
		}
		else
		{
			playerCount -= 1;
		}
	}
}