using UnityEngine;
using Photon.Pun;

public class PUN2_PlayerSync : MonoBehaviourPun, IPunObservable
{

	//List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
	public MonoBehaviour[] localScripts;
	//List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
	public GameObject[] localObjects;
	//Values that will be synced over network
	Vector3 latestPos;
	Quaternion latestRot;
	public bool destroy = false;

	public static Vector3 epsilon = new Vector3(0.15f, 0.15f, 0.15f);

	// AUDIO SETUP STARTS -------------
	public AudioClip hayInteractionAC;
	public float hayInteractionVolume;
	AudioSource hayInteractionAS;
	// AUDIO SETUP END ----------------

	// Use this for initialization
	void Start()
	{
		SetupSound();
		Debug.Log("Name: " + PhotonNetwork.LocalPlayer.NickName);
		if (photonView.IsMine)
		{
			//Player is local
			Player p = (Player)gameObject.GetComponent<Player>();
            if (PhotonNetwork.LocalPlayer.NickName == "Player 1")
			{
				p.team = 1;
			    gameObject.transform.position = GameObject.Find("P1SpawnPoint").transform.position;
			} else if (PhotonNetwork.LocalPlayer.NickName == "Player 2")
			{
				p.team = 2;
				gameObject.transform.position = GameObject.Find("P2SpawnPoint").transform.position;
			} else
			{
				p.team = 3;
			}

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
		}
	}

	void SetupSound()
	{
		hayInteractionAS = gameObject.AddComponent<AudioSource>();
		hayInteractionAS.clip = hayInteractionAC;
		hayInteractionAS.volume = hayInteractionVolume;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			//We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(gameObject.activeSelf);

			Vector3 tempcolor = new Vector3(gameObject.GetComponent<MeshRenderer>().material.color.r, gameObject.GetComponent<MeshRenderer>().material.color.g, gameObject.GetComponent<MeshRenderer>().material.color.b);
			stream.Serialize(ref tempcolor);
		}
		else
		{
			//Network player, receive data
			latestPos = (Vector3)stream.ReceiveNext();
			latestRot = (Quaternion)stream.ReceiveNext();
			gameObject.SetActive((bool) stream.ReceiveNext());
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
			//Update remote player (smooth this, this looks good, at the cost of some accuracy)
			transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 5);
		}
		else
		{
			Collider[] colliders = Physics.OverlapBox(transform.position,
					transform.localScale + epsilon, transform.rotation);
			for (int i = 0; i < colliders.Length; i++)
			{
				GameObject collidedObject = colliders[i].gameObject;

                if (collidedObject.tag == "Tractor")
				{
					PUN2_TractorSync tract = (PUN2_TractorSync)collidedObject.GetComponent<PUN2_TractorSync>();
					Player p = (Player)gameObject.GetComponent<Player>();
					if (tract.team != p.team && Input.GetKey(KeyCode.Space))
					{
						tract.callRemoveFuel(collidedObject.GetComponent<PhotonView>().ViewID);
					}
				}

			}

			if (destroy)
			{
				//GameObject lo = localObjects[0];
				//localObjects[0] = null;
				destroy = false;
				photonView.RPC("DoDeath", RpcTarget.All);
				//Destroy(this);
			}

			Player play = (Player)localScripts[0];

			if (play.state == PlayerState.HasHay && Input.GetKey(KeyCode.X))
			{
				play.state = PlayerState.Empty;
			}
		}
	}
	[PunRPC]
	void DoDeath()
	{
		Debug.Log("HE DIED");
		gameObject.SetActive(false);
	}


}
