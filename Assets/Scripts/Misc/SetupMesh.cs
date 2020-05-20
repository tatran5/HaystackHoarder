using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ONLY WORKS IF THIS HAS BOX COLLIDER COMPONENT

public class SetupMesh : MonoBehaviour
{
	public Canvas canvas;
	float epsilon = 0.25f;

	void Start()
	{
		if (canvas != null)
			SetCollider();
		SetCanvas();
	}

	void SetCanvas()
	{
		Vector3 curPos = gameObject.transform.position;
		Vector3 curSca = gameObject.transform.localScale;
		Quaternion curRot = gameObject.transform.rotation;

		transform.position = new Vector3(0, 0, 0);
		transform.rotation = Quaternion.identity;

		canvas.transform.position = new Vector3(canvas.transform.position.x, 
			gameObject.GetComponent<BoxCollider>().size.y + 1f, canvas.transform.position.z);
		canvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

		gameObject.transform.position = curPos;
		gameObject.transform.localScale = curSca;
		gameObject.transform.rotation = curRot;
	}

	void SetCollider()
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

		BoxCollider collider = gameObject.GetComponent<BoxCollider>();
		
		// Set collider size
		collider.size = bounds.extents * 2 - new Vector3(epsilon, 0, epsilon);
		if (gameObject.CompareTag("Player"))
		{
			// The player might be in T pose which is overestimate the bounds in the x direction
			collider.size = new Vector3(collider.size.x / 2f, collider.size.y, collider.size.z);
		}
		float posSizeX = collider.size.x >= 0 ? collider.size.x : -collider.size.x;
		float posSizeY = collider.size.y >= 0 ? collider.size.y : -collider.size.y;
		float posSizeZ = collider.size.z >= 0 ? collider.size.z : -collider.size.z;
		collider.size = new Vector3(posSizeX, posSizeY, posSizeZ);


		// Set collider center
		collider.center = (bounds.min + bounds.max) * 0.5f;

		gameObject.transform.position = curPos;
		gameObject.transform.localScale = curSca;
		gameObject.transform.rotation = curRot;
	}
}
