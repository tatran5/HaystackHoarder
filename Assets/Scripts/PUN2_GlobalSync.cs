using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PUN2_GlobalSync : MonoBehaviourPun, IPunObservable
{

	//List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
	public MonoBehaviour[] localScripts;
	//List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
	public GameObject[] localObjects;

	// Start is called before the first frame update
	void Start()
	{
	}


	//PHOTON FUNCTIONS
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (photonView.IsMine)
		{
			Global localscript = (Global)localScripts[0];
			photonView.RPC("updateGameState", RpcTarget.Others, photonView.ViewID, localscript.animalsPlayer1,
				localscript.animalsPlayer2, localscript.timeScorePlayer1, localscript.timeScorePlayer2, localscript.timeLeft);
			localscript.updateText();
		}

	}

	[PunRPC]
	public void updateGameState(int viewID, int player1animal, int player2animal, int player1score, int player2score, float timeLeft)
	{
		PhotonView target = PhotonView.Find(viewID);
		Global localscript = target.gameObject.GetComponent<Global>();
		localscript.animalsPlayer1 = player1animal;
		localscript.animalsPlayer2 = player2animal;
		localscript.timeScorePlayer1 = player1score;
		localscript.timeScorePlayer2 = player2score;
		localscript.timeLeft = timeLeft;

		localscript.updateText();
	}
}
