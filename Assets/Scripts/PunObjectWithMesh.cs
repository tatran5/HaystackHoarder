using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunObjectWithMesh : MonoBehaviourPun, IPunObservable
{
	// ONLY WORKS IF THIS HAS BOX COLLIDER COMPONENT

	

	void Start()
	{
		SetCollider();
	}

	public void SetCollider()
	{
		Vector3 curPos = gameObject.transform.position;
		Vector3 curSca = gameObject.transform.localScale;
		Quaternion curRot = gameObject.transform.rotation;

		transform.position = new Vector3(0, 0, 0);
		transform.rotation = Quaternion.identity;

		Renderer thisRenderer = gameObject.GetComponent<Renderer>();
		Bounds bounds = thisRenderer.bounds;
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++)
		{
			Renderer render = renderers[i];
			if (render != thisRenderer)
				bounds.Encapsulate(render.bounds);
		}

		Debug.Log(bounds.extents * 2);
		BoxCollider collider = gameObject.GetComponent<BoxCollider>();
		collider.size = bounds.extents * 2;
		collider.center = 0.5f * (bounds.min + bounds.max);
		Debug.Log(gameObject.GetComponent<BoxCollider>().size);

		gameObject.transform.position = curPos;
		gameObject.transform.localScale = curSca;
		gameObject.transform.rotation = curRot;
	}



	public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{}
}
