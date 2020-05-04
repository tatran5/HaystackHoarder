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

	// Start is called before the first frame update
	void Start()
    {
		Animal animalScr = (Animal)localScripts[0];
		feedMeter = animalScr.feedMeter;
		SetupProgressBar();

		if (photonView.IsMine)
		{
			
			
			Debug.Log(happinessMeter.GetMaxValue());
		}
		else
		{
			//Player is Remote, deactivate the scripts and object that should only be enabled for the local player
			//for (int i = 0; i < localScripts.Length; i++)
			//{
			//	localScripts[i].enabled = false;
			//}
			//for (int i = 0; i < localObjects.Length; i++)
			//{
			//	localObjects[i].SetActive(false);
			//}
			//SetupProgressBar();
		}
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
		//progressBar.SetMaxValue(25);
		//progressBar.SetActive(false);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			Animal animalScr = (Animal) localScripts[0];
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
			//Update remote player (smooth this, this looks good, at the cost of some accuracy)
			transform.position = latestPos; // Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
			transform.rotation = latestRot; // Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 5);
											//Debug.Log("Chickie #2: " + feedMeter + "/" + happinessMeter.GetMaxValue());
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

	[PunRPC]
	public void feedAnimal(int viewID, float amount)
	{
		Debug.Log("hello? add amount " + amount + " to animal " + viewID);
		PhotonView target = PhotonView.Find(viewID);
		target.gameObject.GetComponent<Animal>().feedMeter += amount;
		feedMeter += amount;
	}
}
