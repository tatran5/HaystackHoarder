using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PUN2_AnimalSync : MonoBehaviourPun, IPunObservable
{
	//List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
	public MonoBehaviour[] localScripts;
	//List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
	public GameObject[] localObjects;
	//Values that will be synced over network
	public ProgressBar happinessMeter;
	float feedMeter, feedTickLength;
	Vector3 latestPos;
	Quaternion latestRot;

	// ANIMATION --------------------------------------------------------------
	public Animator animator;

	// Start is called before the first frame update
	void Start()
	{
		animator = GetComponent<Animator>();
     //		animator.Play("Eat");
	//	callPlayEatAnimation();
		Animal animalScr = (Animal)localScripts[0];
		feedMeter = animalScr.feedMeter;
		SetupProgressBar();
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
		happinessMeter = canvasGO.transform.GetChild(0).gameObject.GetComponent<ProgressBar>();
		happinessMeter.SetMaxValue(100);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			Animal animalScr = (Animal)localScripts[0];
			//We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(animalScr.feedMeter);
		}
		else
		{
			//Network player, receive data
			latestPos = (Vector3)stream.ReceiveNext();
			latestRot = (Quaternion)stream.ReceiveNext();
			feedMeter = (float)stream.ReceiveNext();
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
			happinessMeter.SetMaxValue(100);
			happinessMeter.SetValue(feedMeter, 100);
		}
		else
		{
			Animal animalScr = (Animal)localScripts[0];
			feedMeter = animalScr.feedMeter;
			happinessMeter.SetMaxValue(100);
			happinessMeter.SetValue(feedMeter, 100);
		}
	}

	public void callFeedAnimal(float amount)
	{
		photonView.RPC("feedAnimal", RpcTarget.AllViaServer, photonView.ViewID, amount);
	}

	public void callPlayEatAnimation()
	{
		photonView.RPC("playEatAnimation", RpcTarget.AllViaServer);
	}

	public void callPlayRunAnimation()
	{
		photonView.RPC("playRunAnimation", RpcTarget.AllViaServer);
	}

	public void callPlayIdleAnimation()
	{
		photonView.RPC("playIdleAnimation", RpcTarget.AllViaServer);
	}

	[PunRPC]
	public void playIdleAnimation()
	{
		animator.Play("Idle");
	}


	[PunRPC]
	public void playRunAnimation()
	{
		animator.Play("Run");
	}

	[PunRPC]
	public void playEatAnimation()
	{
		animator.Play("Eat");
	}

	[PunRPC]
	public void feedAnimal(int viewID, float amount)
	{
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<Animal>().feedMeter += amount;
		feedMeter += amount;
	}

    public void callUpdatePenNumber(int number)
    {
        photonView.RPC("updatePenNumber", RpcTarget.AllViaServer, photonView.ViewID, number);
    }

    [PunRPC]
    public void updatePenNumber(int viewID, int number)
    {
        PhotonView target = PhotonView.Find(viewID);
        target.gameObject.GetComponent<Animal>().penNumber = number;
    }
}
