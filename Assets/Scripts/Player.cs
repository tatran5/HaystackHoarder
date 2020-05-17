using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum PlayerState {HasHay, HasBale, HasFuel, Empty}

// Player should only hold one thing at a time?
public class Player : ControllableObject
{

    public ProgressBar progressBar;
    public int team = 0;
    public PlayerState state = PlayerState.Empty;
	public bool toDestroy = false;

    // Motion
    public float rotationSpeed = 450;
    private Quaternion targetRotation;
    public Animator animator;

	public float timeSinceLastDisrupt = 0f;
	static float maxTimeDisrupt = 10f;

	public float timeSinceCease = 0f;
	static float maxCeaseTime = 200f;

	public float timeSinceGotAnimal = 0f;
	static float gotAnimalOffset = 0.2f;

	public float timeSincePickupObj = 0f;
	public float pickupOffset = 0.2f;

	// OBJECTS SPAWNING -------------------
	public GameObject gasCanSpawn;
	public GameObject haySpawn;
	public GameObject baleSpawn;

	public Animal animalFollowing = null;


	GameObject refFence;

	public SphereCollider animalTrigger;
	public SphereCollider playerTrigger;

	public bool cease = false;
	public bool performingAnAction = false;

	// Start is called before the first frame updates
	void Start()
    {
		animator = GetComponent<Animator>();
        progressBar.SetActive(false);
        speed = 4f;
    }


    // Update is called once per frame
    void Update()
    {
		HandlePlayerMovement();

		if (Input.GetKeyDown(kbEnterExitTractor))
		{
			EnterTractor();
		}
		else if (Input.GetKeyDown(kbInteract))
		{
			InteractOnce();
		}
		else if (Input.GetKey(kbInteract))
		{
			InteractOverTime();
			checkForBrokenFence();
		}
		else
		{
			if (refFence)
			{
				refFence.GetComponent<PUN2_FenceSync>().updateStats(0, false);
				refFence.GetComponent<PUN2_FenceSync>().updateFixStats(0, false);
				refFence.GetComponent<Fence>().timeToFix = 0f;
				refFence.GetComponent<Fence>().fixing = false;
				refFence = null;
			}
			if (performingAnAction)
			{
				performingAnAction = false;
				//update thru rpc
				gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerActions(gameObject.GetComponent<PhotonView>().ViewID,
																				   timeSinceCease, performingAnAction);
			}
		}
		timeSinceLastDisrupt += 1;
		timeSinceCease += 1;

		if (state != PlayerState.Empty)
		{
			timeSincePickupObj += Time.deltaTime;
		}

		if (animalFollowing != null)
			timeSinceGotAnimal += Time.deltaTime;
		else
			timeSinceGotAnimal = 0.0f;
	}

	bool HandlePlayerMovement()
	{
		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		if (input != Vector3.zero)
		{
			//Debug.Log("Movement input");
			targetRotation = Quaternion.LookRotation(input);
			transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y,
				targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);

			if (state == PlayerState.Empty)
				animator.Play("Running");
			else
				animator.Play("Carry");
			transform.position += speed * input * Time.deltaTime;
			return true;
		}
		else if (state == PlayerState.Empty)
		{
			animator.Play("Idle");
		} else
		{
			animator.Play("CarryIdle");
		}
		return false;
	}

	public override void InteractOnce()
    {
		bool interacted = false;
		Collider[] colliders = Physics.OverlapBox(transform.position,
		transform.localScale + epsilon, transform.rotation);
		for (int i = 0; i < colliders.Length; i++)
		{
			GameObject collidedObject = colliders[i].gameObject;
			if ((collidedObject.tag.Equals("Tractor") && InteractOnceWithTractor(collidedObject.GetComponent<Tractor>())) ||
				(collidedObject.tag.Equals("Barn") && InteractOnceWithBarn(collidedObject.GetComponent<Barn>())) ||
				(collidedObject.tag.Equals("FuelStation") && GetFuelFromStation(collidedObject.GetComponent<FuelStation>())) ||
				(collidedObject.tag.Equals("Animal") && InteractOnceWithAnimal(collidedObject.GetComponent<Animal>())) ||
				(collidedObject.tag.Equals("Hay") && InteractOnceWithHay(collidedObject)) ||
				(collidedObject.tag.Equals("GasCan") && InteractOnceWithGasCan(collidedObject)) ||
				(collidedObject.tag.Equals("Bale") && InteractOnceWithBale(collidedObject)) ||
				(collidedObject.tag.Equals("Player") && InteractOnceWithPlayer(collidedObject)))
			{
				interacted = true;
				break;
			}
		}

		// If the player a not interacted with any other object, 
		// drop whatever object that the player has in front of the player
		//if (!interacted)
		//{
		if (animalFollowing != null && timeSinceGotAnimal >= gotAnimalOffset) //timeSincePlayerEnter >= timeOffsetPlayerEnter
		{
			// Perform box casting to decide whether to drop animal
			Vector3 boxCenter = transform.position + transform.forward * transform.localScale.z * 0.5f;
			boxCenter.y = animalFollowing.transform.position.y;

			float maxDistance = animalFollowing.transform.localScale.z * 0.5f * (1 + epsilon.z);
			if (!Physics.BoxCast(boxCenter, animalFollowing.transform.localScale, transform.forward,
				transform.rotation, maxDistance))
			{
				gameObject.GetComponent<PUN2_PlayerSync>().callPlaceAnimal(animalFollowing.gameObject.GetComponent<PhotonView>().ViewID);
			}
			else
			{
				Debug.Log("Cannot drop animal here because there's an obstacle");
			}
		} else if (state != PlayerState.Empty && timeSincePickupObj >= pickupOffset)
		{
			DropObjectHeld();
		}
		//}
	}

	public bool DropObjectHeld()
	{
		GameObject objectSpawn = null;
		if (state == PlayerState.HasFuel) objectSpawn = gasCanSpawn;
		else if (state == PlayerState.HasHay) objectSpawn = haySpawn;
		else if (state == PlayerState.HasBale) objectSpawn = baleSpawn;

		if (objectSpawn != null)
		{
			// Perform box casting to decide whether to drop object held
			Vector3 boxCenter = transform.position + transform.forward * transform.localScale.z * 0.5f;
			boxCenter.y = objectSpawn.transform.position.y;

			float maxDistance = objectSpawn.transform.localScale.z * 0.5f * (1 + epsilon.z);
			if (!Physics.BoxCast(boxCenter, objectSpawn.transform.localScale, transform.forward,
				transform.rotation, maxDistance))
			{
				Vector3 position = transform.position +
					(1f + epsilon.x) * transform.forward * 0.5f * (transform.localScale.z + objectSpawn.transform.localScale.z);
				position.y = transform.position.y - transform.localScale.y / 2f + objectSpawn.transform.localScale.y / 2f;

				if (state == PlayerState.HasFuel)
				{
					GetComponent<PUN2_PlayerSync>().callChangePlayerState(0);
					GetComponent<PUN2_PlayerSync>().DropObject("GasCan", position, transform.rotation);
					GetComponent<PUN2_PlayerSync>().callPlayDropGasCanSound();
				}
				else if (state == PlayerState.HasHay)
				{
					GetComponent<PUN2_PlayerSync>().callChangePlayerState(0);
					GetComponent<PUN2_PlayerSync>().DropObject("Hay", position, transform.rotation);
					gameObject.GetComponent<PUN2_PlayerSync>().playHayInteractionSound();
				}
				else if (state == PlayerState.HasBale)
				{
					GetComponent<PUN2_PlayerSync>().callChangePlayerState(0);
					GetComponent<PUN2_PlayerSync>().DropObject("Bale", position, transform.rotation);
					gameObject.GetComponent<PUN2_PlayerSync>().playHayInteractionSound();
				}
				else Debug.Log("Player::DropObject: Uh oh problem");

				state = PlayerState.Empty;
				return true;
			}
			else
			{
				Debug.Log("Cannot drop object here because there's an obstacle");
			}
		}
		return false;
	}

    /* Player and fuel station must be on the same team for the player to get fuel from the station*/
    private bool GetFuelFromStation(FuelStation fuelStation) { 
        if (state == PlayerState.Empty && team == fuelStation.team)
		{
			state = PlayerState.HasFuel;
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(3);
			gameObject.GetComponent<PUN2_PlayerSync>().callPlayGetFuelSound();
			timeSincePickupObj = 0f;
			return true;
		}
		return false;
	} 

	private bool InteractOnceWithPlayer(GameObject otherPlayerGO)
	{
		Player otherPlayer = otherPlayerGO.GetComponent<Player>();
        if (team != otherPlayer.team)
		{
			if (otherPlayer.animalFollowing != null)
			{
				otherPlayerGO.GetComponent<PUN2_PlayerSync>().callPlaceAnimal(animalFollowing.gameObject.GetComponent<PhotonView>().ViewID);
				return true;
			}
		}
		return false;
	}
		
	private bool InteractOnceWithHay(GameObject hayGO)
	{
		if (state == PlayerState.Empty)
		{
			timeSincePickupObj = 0f;
			state = PlayerState.HasHay;
			gameObject.GetComponent<PUN2_PlayerSync>().playHayInteractionSound();
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(1);
			hayGO.GetComponent<PUN2_DroppableSync>().callMakeDisappear();
			return true;
		}
		return false;
	}

	private bool InteractOnceWithBale(GameObject baleGO)
	{
		if (state == PlayerState.Empty)
		{
			timeSincePickupObj = 0f;
			state = PlayerState.HasBale;
			gameObject.GetComponent<PUN2_PlayerSync>().playHayInteractionSound();
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(2);
			baleGO.GetComponent<PUN2_DroppableSync>().callMakeDisappear();
			return true;
		}
		return false;
	}
	private bool InteractOnceWithGasCan(GameObject gasCanGO)
	{
		if (state == PlayerState.Empty)
		{ 
			timeSincePickupObj = 0f;
			state = PlayerState.HasFuel;
			gameObject.GetComponent<PUN2_PlayerSync>().callPlayPickupGasCanSound();
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(3);
			gasCanGO.GetComponent<PUN2_DroppableSync>().callMakeDisappear();
			return true;
		}
		return false;
	}

    // Handle give hay & take bale
    private bool InteractOnceWithBarn(Barn barn)
    {
        if (state == PlayerState.HasHay && barn.state == BarnState.Empty && team == barn.team)
        {
			barn.StartProcessingHay();
            state = PlayerState.Empty;
			gameObject.GetComponent<PUN2_PlayerSync>().callPlayHayInteractionSound();
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(0);
			return true;
        }
        else if (state == PlayerState.Empty && barn.GetBale())
        {
			state = PlayerState.HasBale;
			gameObject.GetComponent<PUN2_PlayerSync>().callPlayHayInteractionSound();
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(2);
			timeSincePickupObj = 0f;
			return true;
		}
		return false;
    }
  
	public bool InteractOnceWithAnimal(Animal animal)
	{
		if (state != PlayerState.HasBale && animalFollowing ==null) // if player holds bale, feed animal. Otherwise, bring animal back inside fences
		{
			animalFollowing = animal;
			gameObject.GetComponent<PUN2_PlayerSync>().callFollowAnimal(animal.gameObject.GetComponent<PhotonView>().ViewID);
			return true;
		}
		return false;
	}

    public override void InteractOverTime()
    {
		Collider[] colliders = Physics.OverlapBox(transform.position,
						  transform.localScale + epsilon, transform.rotation);
		for (int i = 0; i < colliders.Length; i++)
		{
			GameObject collidedObject = colliders[i].gameObject;
			if (collidedObject.tag == "Fence")
			{
				PUN2_FenceSync sync = collidedObject.GetComponent<PUN2_FenceSync>();
				if (sync.team != team)
				{
					if (!sync.broken)
					{
						BreakFence(collidedObject);
						if (!performingAnAction)
						{
							performingAnAction = true;
							//update thru rpc
							gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerActions(gameObject.GetComponent<PhotonView>().ViewID, timeSinceCease, performingAnAction);
						}
					}
					else
					{
						if (performingAnAction)
						{
							performingAnAction = false;
							//update thru rpc
							gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerActions(gameObject.GetComponent<PhotonView>().ViewID, timeSinceCease, performingAnAction);
						}
					}
				}
			}
		}
	}

	private void BreakFence(GameObject fence)
	{
		if (state != PlayerState.Empty)
		{
			return;
		}
		refFence = fence;
		if (state == PlayerState.Empty)
		{
			if (fence.GetComponent<Fence>().timeToBreak >= fence.GetComponent<Fence>().totalTimeToBreak)
			{
				fence.GetComponent<Fence>().BreakFence();
				fence.GetComponent<PUN2_FenceSync>().updateStats(0, false);
			}
			else
			{
				if (timeSinceCease >= maxCeaseTime)
				{
					fence.GetComponent<PUN2_FenceSync>().updateStats(fence.GetComponent<Fence>().timeToBreak + Time.fixedDeltaTime, true);
				}
				else
				{
					fence.GetComponent<PUN2_FenceSync>().updateStats(0, false);
					cease = false;
					performingAnAction = false;
					//update cease/action
					gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerActions(gameObject.GetComponent<PhotonView>().ViewID, timeSinceCease, performingAnAction);
				}
			}
		}
	}

    public void FixFence(GameObject fence)
	{
		refFence = fence;
		if (state == PlayerState.Empty)
		{
			if (fence.GetComponent<Fence>().timeToFix >= fence.GetComponent<Fence>().totalTimeToFix)
			{
				fence.GetComponent<Fence>().FixFence();
				fence.GetComponent<Fence>().timeToFix = 0f;
				fence.GetComponent<Fence>().fixing = false;
				refFence.GetComponent<PUN2_FenceSync>().updateFixStats(0, false);
			}
			else
			{
				refFence.GetComponent<PUN2_FenceSync>().updateFixStats(fence.GetComponent<Fence>().timeToFix + Time.fixedDeltaTime, true);
				fence.GetComponent<Fence>().timeToFix += Time.fixedDeltaTime;
				fence.GetComponent<Fence>().fixing = true;
			}
		}
	}

	private void EnterTractor()
    {
		Collider[] colliders = Physics.OverlapBox(transform.position,
				   transform.localScale + epsilon, transform.rotation);
		for (int i = 0; i < colliders.Length; i++)
		{
			Tractor tractor = colliders[i].gameObject.GetComponent<Tractor>();
			if (tractor && !tractor.HasPlayer() && tractor.team == team)
			{
				GetComponent<Rigidbody>().velocity = Vector3.zero;
				GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				tractor.PlayerEnter();
				gameObject.GetComponent<PUN2_PlayerSync>().destroy = true;
			}
		}
	}

    public bool InteractOnceWithTractor(Tractor tractor)
    {
        if (state == PlayerState.HasFuel)
        {
			gameObject.GetComponent<PUN2_PlayerSync>().callPlayRefillFuelSound();
			state = PlayerState.Empty;
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(0);
			tractor.RefillFuel();
			return true;
        } else if (state == PlayerState.Empty)
        {
            return GetHayFromTractor(tractor);
        }
		return false;
    }
    private bool GetHayFromTractor(Tractor tractor)
    {
        if (tractor.GetHay())
        {
            state = PlayerState.HasHay;
			gameObject.GetComponent<PUN2_PlayerSync>().callPlayHayInteractionSound();
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(1);
			timeSincePickupObj = 0f;
			return true; 
        }
		return false;
    }

	private void checkForBrokenFence()
	{
        if (state != PlayerState.Empty)
		{
			return;
		}
		GameObject[] fences = GameObject.FindGameObjectsWithTag("Fence");
		for (int i = 0; i < fences.Length; i++)
		{
			Fence fence = (Fence)fences[i].GetComponent<Fence>();
			if (fence.team == team)
			{
				//need to get rotation vector of fence
				float yAngle = fence.gameObject.transform.rotation.eulerAngles.y;
				if (yAngle == 90) // check y range 
				{
					float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
						new Vector2(fences[i].transform.position.x, fences[i].transform.position.y));
					float playerZ = transform.position.z;
					float fenceZ = fences[i].transform.position.z;
					if (dist < 1.5f && (playerZ < (fenceZ + 5f) && playerZ > (fenceZ - 5f)))
					{
						FixFence(fences[i]);
					}
				}
				else //check x range
				{
					float dist = Vector2.Distance(new Vector2(transform.position.y, transform.position.z),
						new Vector2(fences[i].transform.position.y, fences[i].transform.position.z));
					float playerX = transform.position.x;
					float fenceX = fences[i].transform.position.x;
					if (dist < 1.5f && (playerX < (fenceX + 5f) && playerX > (fenceX - 5f)))
					{
						FixFence(fences[i]);
					}
				}

			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
        if (state == PlayerState.HasBale)
		{
			Collider[] hitColliders = Physics.OverlapSphere(transform.position, 4);

			GameObject closestAnimal = null;
			float closestDist = 1000f;
			for (int i = 0; i < hitColliders.Length; i++)
			{
				Collider otherAnimal = hitColliders[i];
				if (otherAnimal.gameObject.tag == "Animal")
				{
					float distance = Vector3.Distance(transform.position, otherAnimal.gameObject.transform.position);
					if (distance < closestDist)
					{
						closestDist = distance;
						closestAnimal = otherAnimal.gameObject;
					}
				}
			}

			if (closestAnimal != null && state == PlayerState.HasBale && Input.GetKeyDown(kbInteract))
			{
				state = PlayerState.Empty;
				gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(0);
				closestAnimal.gameObject.GetComponent<Animal>().FeedAnimal();
			}
		}

		//add time since last trigger to prevent multiple thefts

		float dist = Vector3.Distance(transform.position, other.gameObject.transform.position);
		if (dist < 1.5f && other.gameObject.tag == "Player" && timeSinceLastDisrupt >= maxTimeDisrupt
			&& Input.GetKeyDown(kbInteract))
		{
			//Debug.Log("CEASE AND DESIST!");
			Player otherPlayer = other.gameObject.GetComponent<Player>();
			PlayerState state = otherPlayer.state;


			if (state == PlayerState.Empty)
			{
				gameObject.GetComponent<PUN2_PlayerSync>().callCease(other.gameObject.GetComponent<PhotonView>().ViewID, 0);
			}
			else if (state == PlayerState.HasHay)
			{
				gameObject.GetComponent<PUN2_PlayerSync>().callCease(other.gameObject.GetComponent<PhotonView>().ViewID, 1);
			}
			else if (state == PlayerState.HasBale)
			{
				gameObject.GetComponent<PUN2_PlayerSync>().callCease(other.gameObject.GetComponent<PhotonView>().ViewID, 2);
			}
			else 
			{
				gameObject.GetComponent<PUN2_PlayerSync>().callCease(other.gameObject.GetComponent<PhotonView>().ViewID, 3);
			}

			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerActions(other.gameObject.GetComponent<PhotonView>().ViewID, 0,
																			   other.gameObject.GetComponent<Player>().performingAnAction);
			timeSinceLastDisrupt = 0f;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Animal")
		{
			other.gameObject.GetComponent<Animal>().Normal();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{

		if (collision.gameObject.tag == "Fence")
		{
            if (collision.gameObject.GetComponent<Fence>().team == team)
			{
				Physics.IgnoreCollision(collision.collider, gameObject.GetComponent<Collider>());
			}
		}

	}
}
