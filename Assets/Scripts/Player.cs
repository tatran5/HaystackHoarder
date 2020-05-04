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
    private Animator animator;

	public float timeSinceLastDisrupt = 0f;
	static float maxTimeDisrupt = 10f;

	public float timeSinceCease = 0f;
	static float maxCeaseTime = 200f;

	// TODO: Delete variables once finish testing
	private float testProgress = 0f;
    public Material testHasHayMaterial;
    public Material testPlayerMaterial;
	public Material tractorColor;

	GameObject refFence;

	public SphereCollider animalTrigger;
	public SphereCollider playerTrigger;

	public bool cease = false;
	public bool performingAnAction = false;

	// Start is called before the first frame update
	void Start()
    {
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
		} else if (Input.GetKeyDown(kbInteract))
		{
			InteractOnce();
		} else if (Input.GetKey(kbInteract))
		{
			InteractOverTime();
			checkForBrokenFence();
		} else
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
	}

	bool HandlePlayerMovement()
	{
		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		if (input != Vector3.zero)
		{
			targetRotation = Quaternion.LookRotation(input);
			transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y,
				targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);
			transform.position += speed * input * Time.deltaTime;
			return true;
		}
		else
			return false;
	}

	public override void InteractOnce()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position,
        transform.localScale + epsilon, transform.rotation);
        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject collidedObject = colliders[i].gameObject;
            if (collidedObject.tag.Equals("Tractor"))
                InteractOnceWithTractor(collidedObject.GetComponent<Tractor>());
            else if (collidedObject.tag.Equals("Barn"))
                InteractOnceWithBarn(collidedObject.GetComponent<Barn>());
            else if (collidedObject.tag.Equals("FuelStation"))
                GetFuelFromStation(collidedObject.GetComponent<FuelStation>());
        }
    }

    /* Player and fuel station must be on the same team for the player to get fuel from the station*/
    private void GetFuelFromStation(FuelStation fuelStation) { 
        if (state == PlayerState.Empty && team == fuelStation.team)
		{
			state = PlayerState.HasFuel;
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(3);
		}
    } 

    // Handle give hay & take bale
    private void InteractOnceWithBarn(Barn barn)
    {
        if (state == PlayerState.HasHay && barn.state == BarnState.Empty && team == barn.team)
        {
            barn.StartProcessingHay();
            state = PlayerState.Empty;
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(0);
			gameObject.GetComponent<MeshRenderer>().material = testPlayerMaterial; // TODO: delete after finishing debugging with hasHay
        }
        else if (state == PlayerState.Empty && barn.GetBale())
        {
            state = PlayerState.HasBale;
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(2);
			gameObject.GetComponent<MeshRenderer>().material = testHasHayMaterial; // TODO: delete after finishing debugging with hasHay
        }
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
					} else
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
		refFence = fence;
        if (state == PlayerState.Empty)
		{
            if (fence.GetComponent<Fence>().timeToBreak >= fence.GetComponent<Fence>().totalTimeToBreak)
			{
				fence.GetComponent<Fence>().BreakFence();
				fence.GetComponent<PUN2_FenceSync>().updateStats(0, false);
			} else
			{
				if (timeSinceCease >= maxCeaseTime)
				{
					fence.GetComponent<PUN2_FenceSync>().updateStats(fence.GetComponent<Fence>().timeToBreak + Time.fixedDeltaTime, true);
				} else
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

    public void InteractOnceWithTractor(Tractor tractor)
    {
        if (state == PlayerState.HasFuel)
        {
            state = PlayerState.Empty;
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(0);
			tractor.RefillFuel();
            Debug.Log("Refilled");
        } else if (state == PlayerState.Empty)
        {
            GetHayFromTractor(tractor);
        }
    }
    private void GetHayFromTractor(Tractor tractor)
    {
        if (tractor.GetHay())
        {
            state = PlayerState.HasHay;
			gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(1);
			gameObject.GetComponent<MeshRenderer>().material = testHasHayMaterial; // TODO: delete after finishing debugging with hasHay
        }
    }

    private void checkForBrokenFence()
	{
		GameObject[] fences = GameObject.FindGameObjectsWithTag("Fence");
        for (int i = 0; i < fences.Length; i++)
		{
			Fence fence = (Fence) fences[i].GetComponent<Fence>();
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
				} else //check x range
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
		if (other.gameObject.tag == "Animal")
		{
			other.gameObject.GetComponent<Animal>().Highlighted();
			if (state == PlayerState.HasBale && Input.GetKeyDown(kbInteract))
			{
				state = PlayerState.Empty;
				gameObject.GetComponent<PUN2_PlayerSync>().callChangePlayerState(0);
				other.gameObject.GetComponent<Animal>().FeedAnimal();
			}
		}

        //add time since last trigger to prevent multiple thefts

		float dist = Vector3.Distance(transform.position, other.gameObject.transform.position);
		if (dist < 1.5f && other.gameObject.tag == "Player" && timeSinceLastDisrupt >= maxTimeDisrupt
            && Input.GetKeyDown(kbInteract))
		{
			//Debug.Log("CEASE AND DESIST!");

			PlayerState state = other.gameObject.GetComponent<Player>().state;


			if (state == PlayerState.Empty)
			{
				gameObject.GetComponent<PUN2_PlayerSync>().callCease(other.gameObject.GetComponent<PhotonView>().ViewID, 0);
			} else if (state == PlayerState.HasHay)
			{
				gameObject.GetComponent<PUN2_PlayerSync>().callCease(other.gameObject.GetComponent<PhotonView>().ViewID, 1);
			} else if (state == PlayerState.HasBale)
			{
				gameObject.GetComponent<PUN2_PlayerSync>().callCease(other.gameObject.GetComponent<PhotonView>().ViewID, 2);
			} else
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
}
