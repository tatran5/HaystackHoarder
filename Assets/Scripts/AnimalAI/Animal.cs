using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimalType { General, Chicken, Cow, Sheep }

public class Animal : MonoBehaviour
{
	Global globalObj;

	// Variables related to the feed meter that depletes with time
	public float feedMeter;
	public float maxFeedMeter;
	public float feedTickSeconds;  // How much time it takes to deplete meter by one.
								   // Altered per animal, can also be altered to affect difficulty.
	float feedTimer;              // Pairs with previous variable, updates with Update()

	public float weight;        // Varies per animal
	public float speed;         // Speed at which the animal runs

	public int penNumber;       // Corresponds with player number, used for
								// tracking pen fences. Ranges from 1 - 4.
								// If the animal is running free, penNumber
								// will be set to 0.

	int checkFencesTickLength;
	int checkFencesTimer;

	int idleTimer;
	int idleMaxLength;

	public Vector2 targetPoint;
	public Vector3 targetDirection;

	public Vector2 currentPos;

	int stuckTimer;
	int stuckTimerTicks;
	int stuckTimerTickLength;
	int stuckMaxTicks;

	public Material normal;
	public Material highlighted;

	bool selected = false;

	public float epsilonDistanceOffset = 0.00001f;
	public bool isFollowingPlayer = false;
	public Player playerFollowing;

	/********************
     * MISC. HELPER FUNCTIONS
     ********************/

	protected bool PositionEquality(Vector2 pos1, Vector2 pos2)
	{
		return Mathf.Abs(pos1.x - pos2.x) < 0.1f && Mathf.Abs(pos1.y - pos2.y) < 0.1f;
	}

	protected Vector2 RotateVectorByAngle(Vector2 v, float angle)
	{
		return new Vector2(v.x * Mathf.Cos(angle) - v.y * Mathf.Sin(angle),
						   v.x * Mathf.Sin(angle) + v.y * Mathf.Cos(angle));
	}

	// Start is called before the first frame update
	protected void InitializeBaseVariables()
	{
		gameObject.GetComponent<MeshRenderer>().material = normal;
		globalObj = GameObject.Find("GlobalObject").GetComponent<Global>();

		maxFeedMeter = 100.0f;
		feedMeter = Random.Range(maxFeedMeter / 2, 3 * maxFeedMeter / 4);


		feedTimer = 0;
		feedTickSeconds = 2;

		checkFencesTimer = 0;
		checkFencesTickLength = 35;

		stuckTimer = 0;
		stuckTimerTicks = 0;
		stuckTimerTickLength = 100;
		stuckMaxTicks = 3;

		targetPoint = Vector2.zero;
		targetDirection = Vector3.zero;

		currentPos = Vector2.zero;
	}

	// Update is called once per frame
	void Update()
	{
		// Update feed Meter
		feedTimer += Time.deltaTime;
		if (feedTimer >= feedTickSeconds)
		{
			feedMeter -= 1.0f;
			if (feedMeter < 0f)
			{
				feedMeter = 0f;
			}
			feedTimer = 0;
		}

		//slowly darken the color the lower the feed meter gets
		if (!selected)
		{
			Color curr = gameObject.GetComponent<MeshRenderer>().material.color;
			gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color((feedMeter / 50f) * curr.r, (feedMeter / 50f) * curr.g, (feedMeter / 50f) * curr.b, 1.0f));
		}


		if (isFollowingPlayer && playerFollowing != null)
		{
			float positionY = transform.position.y;
			transform.position = playerFollowing.transform.position - (1f + epsilonDistanceOffset) *
				playerFollowing.transform.forward * 0.5f * (playerFollowing.transform.localScale.z + transform.localScale.z);
			transform.position = new Vector3(transform.position.x, positionY, transform.position.z);
			transform.rotation = playerFollowing.transform.rotation;
		}
		else
		{
			currentPos.x = gameObject.transform.position.x;
			currentPos.y = gameObject.transform.position.z;

			if (PositionEquality(targetPoint, currentPos))
			{
				targetDirection = Vector3.zero;
			}

			if (penNumber > 0)
			{
				checkFencesTimer += 1;
				if (checkFencesTimer >= checkFencesTickLength)
				{
					checkFencesTimer = 0;

					int fenceIndex = CheckForEscape();
					if (fenceIndex >= 0)
					{
						GetEscapeDirection(fenceIndex);
					}
					else if (targetDirection == Vector3.zero)
					{
						//GetIdleDirection();
					}
				}
			}
			else
			{
				checkFencesTimer += 1;
				if (checkFencesTimer >= checkFencesTickLength)
				{
					checkFencesTimer = 0;
					GetInsidePenStatus();
				}
				if (penNumber == 0)
				{
					GetWanderDirection();
				}
				gameObject.transform.position += targetDirection * speed * Time.deltaTime;
			}
		}
	}

	void FixedUpdate()
	{
	}

	public void SetFollowingPlayer(Player player)
	{
		isFollowingPlayer = true;
		playerFollowing = player;
		globalObj.RemoveAnimal(penNumber);
		penNumber = 0;
		gameObject.GetComponent<PUN2_AnimalSync>().callUpdatePenNumber(penNumber);
	}

	public void SetStopFollowingPlayer()
	{
		Debug.Log("in stop following player...");
		if (playerFollowing != null)
		{
			Transform pTrans = playerFollowing.transform;
			isFollowingPlayer = false;
			playerFollowing = null;
			Debug.Log("hello?? playerFollowing is " + (playerFollowing == null));
			transform.position = pTrans.position + (1f + epsilonDistanceOffset) * pTrans.forward *
				0.5f * (transform.localScale.z + pTrans.localScale.z);
			Debug.Log("placing the chickies");
		}
		GetInsidePenStatus();
	}

	// If the animal collides with something dynamic (a constantly changing
	// position, like the player, tractor, and other animals), this should
	// redirect it.

	void OnCollisionEnter(Collision collision)
	{
		Collider collider = collision.collider;
		if (collider.gameObject.layer == 9)
		{
			targetDirection = Vector3.zero;
		}

	}

	void OnCollisionStay(Collision collision)
	{
		Collider collider = collision.collider;
		if (collider.gameObject.layer == 8)
		{
			stuckTimer += 1;
			if (stuckTimer >= stuckTimerTickLength)
			{
				stuckTimerTicks += 1;
				if (stuckTimerTicks >= stuckMaxTicks)
				{
					targetDirection = Vector3.zero;
				}
			}
		}
	}

	void OnCollisionExit(Collision collision)
	{
		Collider collider = collision.collider;
		if (collider.gameObject.layer == 8)
		{
			stuckTimer = 0;
			stuckTimerTicks = 0;
		}
	}

	public void Highlighted()
	{
		gameObject.GetComponent<MeshRenderer>().material = highlighted;
		selected = true;
	}

	public void Normal()
	{
		gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color((feedMeter / 50f) * normal.color.r,
																	(feedMeter / 50f) * normal.color.g,
																	(feedMeter / 50f) * normal.color.b, 1.0f));
		selected = false;
	}

	protected bool GetTargetPoint(Vector2 direction)
	{
		int targetIndex = globalObj.grid_raycastFromPoint(currentPos, direction);
		if (targetIndex >= 0)
		{
			targetPoint = globalObj.grid_getCenterOfCell(targetIndex);
			targetDirection = new Vector3(targetPoint.x, 0.0f, targetPoint.y)
							  - new Vector3(currentPos.x, 0.0f, currentPos.y);
			targetDirection = targetDirection.normalized;
			return true;

		}
		else
		{
			return false;
		}
	}

	// Returns the index of the fence that is broken,
	// if any. If all fences are intact, return -1;
	protected int CheckForEscape()
	{
		Pen p = globalObj.GetPlayerPen(penNumber);
		int brokenFence = p.GetBrokenFenceIndex();
		return brokenFence;
	}

	protected void GetEscapeDirection(int index)
	{
		Fence f = globalObj.GetPlayerPen(penNumber).fences[index];

		// Calculate the range of angles the animal can move on,
		// choose a random one
		Vector2[] endpoints = f.GetEndpoints();
		Vector2 u = endpoints[0] - currentPos;
		Vector2 v = endpoints[1] - currentPos;

		float angleRange = Mathf.Acos(Vector2.Dot(u, v) / (u.magnitude * v.magnitude));

		float angle = Random.Range(-angleRange / 4, angleRange / 4);

		Vector2 animalToFence = new Vector2(f.gameObject.transform.position.x,
											  f.gameObject.transform.position.z)
								   - currentPos;

		Vector2 targetDir = RotateVectorByAngle(animalToFence, angle);

		if (!GetTargetPoint(targetDir))
		{
			targetDirection = Vector3.zero;
		}
		else
		{
			globalObj.RemoveAnimal(penNumber);
			penNumber = 0;
			gameObject.GetComponent<PUN2_AnimalSync>().callUpdatePenNumber(penNumber);
		}

	}

	protected void GetIdleDirection()
	{
		float wanderX = Random.Range(-1.0f, 1.0f);
		float wanderZ = Random.Range(-1.0f, 1.0f);
		targetDirection = new Vector3(wanderX, 0.0f, wanderZ);

	}


	protected virtual void GetWanderDirection()
	{
		if (targetDirection == Vector3.zero)
		{

			Vector2 currentPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
			float wanderX = Random.Range(-1.0f, 1.0f);
			float wanderZ = Random.Range(-1.0f, 1.0f);
			targetDirection = new Vector3(wanderX, 0.0f, wanderZ);

			// Ray to cast along
			Vector2 targetDir = new Vector2(targetDirection.x, targetDirection.z);
			if (!GetTargetPoint(targetDir))
			{
				targetDirection = Vector3.zero;
			}
		}
	}


	// Check if we're inside another player's pen; if so,
	// neutralize the target direction and change pen number.
	void GetInsidePenStatus()
	{
		if (!isFollowingPlayer)
		{
			for (int i = 1; i <= globalObj.numPlayers; i++)
			{
				Pen p = globalObj.GetPlayerPen(i);
				if (p.FencesIntact() && p.InsidePenArea(gameObject))
				{
					penNumber = i;
					gameObject.GetComponent<PUN2_AnimalSync>().callUpdatePenNumber(penNumber);
					globalObj.AddAnimal(i);
					targetDirection = Vector3.zero;
					return;
				}
			}
		}
	}

	public void FeedAnimal()
	{
		Debug.Log("FEEDING ANIMAL!!");
		gameObject.GetComponent<PUN2_AnimalSync>().callFeedAnimal(50f);
		feedTimer = 0;
	}

	public virtual AnimalType GetAnimalType()
	{
		return AnimalType.General;
	}
}
