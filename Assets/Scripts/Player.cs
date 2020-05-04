using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

	// SOUND VARIABLES START HERE ------------------
	public AudioClip hayInteractionAC;
	public float hayInteractionVolume;
	AudioSource hayInteractionAS;

	public AudioClip getFuelAC;
	public float getFuelVolume;
	AudioSource getFuelAS;

	public AudioClip refillFuelAC;
	public float refillFuelVolume;
	AudioSource refillFuelAS;
	// SOUND VARIABLES END HERE ------------------

	// OBJECTS HELD START HERE -------------------
	public GameObject gasCanHeld;
	public GameObject hayHeld;
	// OBJECTS HELD END HERE -------------------

	// TODO: Delete variables once finish testing
	private float testProgress = 0f;
    public Material testHasHayMaterial;
    public Material testPlayerMaterial;
	public Material tractorColor;
	public Material P1;
	public Material P2;
	public Material P3;
	public Material P1HasHay;
	public Material P2HasHay;
	public Material P1HasFuel;
	public Material P2HasFuel;

	GameObject refFence;

	// Start is called before the first frame update
	void Start()
    {
		animator = GetComponent<Animator>();
        progressBar.SetActive(false);
        speed = 4f;
		SetupObjectHeld();
		SetupSound();
    }
	
	void SetupObjectHeld()
	{
		gasCanHeld.SetActive(false);
		hayHeld.SetActive(false);
	}
	void SetupSound()
	{
		hayInteractionAS = gameObject.AddComponent<AudioSource>();
		hayInteractionAS.clip = hayInteractionAC;
		hayInteractionAS.volume = hayInteractionVolume;

		getFuelAS = gameObject.AddComponent<AudioSource>();
		getFuelAS.clip = getFuelAC;
		getFuelAS.volume = getFuelVolume;

		refillFuelAS = gameObject.AddComponent<AudioSource>();
		refillFuelAS.clip = refillFuelAC;
		refillFuelAS.volume = refillFuelVolume;
	}

    // Update is called once per frame
    void Update()
    {
		if (team == 1)
		{
			if (state == PlayerState.HasHay || state == PlayerState.HasBale)
			{
				gameObject.GetComponent<MeshRenderer>().material = P1HasHay;
			}
			else if (state == PlayerState.HasFuel)
			{
				gameObject.GetComponent<MeshRenderer>().material = P1HasFuel;
			}
			else
			{
				gameObject.GetComponent<MeshRenderer>().material = P1;
			}
		}
		else if (team == 2)
		{
			if (state == PlayerState.HasHay || state == PlayerState.HasBale)
			{
				gameObject.GetComponent<MeshRenderer>().material = P2HasHay;
			}
			else if (state == PlayerState.HasFuel)
			{
				gameObject.GetComponent<MeshRenderer>().material = P2HasFuel;
			} else 
			{
				gameObject.GetComponent<MeshRenderer>().material = P2;
			}
		}
		else
		{
			gameObject.GetComponent<MeshRenderer>().material = P3;
		}

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
		} else
		{
            if (refFence)
			{
				refFence.GetComponent<PUN2_FenceSync>().updateStats(0, false);
				refFence = null;
			}
		}
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
        Collider[] colliders = Physics.OverlapBox(transform.position,
        transform.localScale + epsilon, transform.rotation);
        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject collidedObject = colliders[i].gameObject;
			if ((collidedObject.tag.Equals("Tractor") && InteractOnceWithTractor(collidedObject.GetComponent<Tractor>())) ||
				(collidedObject.tag.Equals("Barn") && InteractOnceWithBarn(collidedObject.GetComponent<Barn>())) ||
				(collidedObject.tag.Equals("FuelStation") && GetFuelFromStation(collidedObject.GetComponent<FuelStation>())) ||
				(collidedObject.tag.Equals("Animal") && InteractOnceWithAnimal(collidedObject.GetComponent<Animal>())))
			{
				break;
			}
        }
		
		// Drop whatever object that the player has in front of the player
		
    }

    /* Player and fuel station must be on the same team for the player to get fuel from the station*/
    private bool GetFuelFromStation(FuelStation fuelStation) { 
        if (state == PlayerState.Empty && team == fuelStation.team)
		{
			state = PlayerState.HasFuel;
			getFuelAS.Play();
			gasCanHeld.SetActive(true);
			return true;
		}
		return false;
	} 

    // Handle give hay & take bale
    private bool InteractOnceWithBarn(Barn barn)
    {
        if (state == PlayerState.HasHay && barn.state == BarnState.Empty && team == barn.team)
        {
			hayInteractionAS.Play();
            barn.StartProcessingHay();
            state = PlayerState.Empty;
			hayHeld.SetActive(false);
			return true;
        }
        else if (state == PlayerState.Empty && barn.GetBale())
        {
			hayInteractionAS.Play();
			state = PlayerState.HasBale;
			return true;
		}
		return false;
    }
  
	public bool InteractOnceWithAnimal(Animal animal)
	{
		if (state != PlayerState.HasBale) // if player holds bale, feed animal. Otherwise, bring animal back inside fences
		{
			animal.SetFollowingPlayer(this);
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
				//fence.GetComponent<Fence>().timeToBreak = 0;
				//fence.GetComponent<PUN2_FenceSync>().beingBroken = false;
				fence.GetComponent<PUN2_FenceSync>().updateStats(0, false);
			} else
			{
				//fence.GetComponent<Fence>().timeToBreak += Time.fixedDeltaTime;
				//fence.GetComponent<PUN2_FenceSync>().beingBroken = true;
				fence.GetComponent<PUN2_FenceSync>().updateStats(fence.GetComponent<Fence>().timeToBreak + Time.fixedDeltaTime, true);
				//Debug.Log("Interacting!!");
			}
		}
	}

    public void FixFence(GameObject fence)
	{
		Debug.Log("Hellloooo");
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
				//Destroy(gameObject);
				gameObject.GetComponent<PUN2_PlayerSync>().destroy = true;
				//Destroy(tractor);
				//gameObject.GetComponent<MeshRenderer>().material = tractorColor;
			}
        }
    }

    public bool InteractOnceWithTractor(Tractor tractor)
    {
        if (state == PlayerState.HasFuel)
        {
			refillFuelAS.Play();
            state = PlayerState.Empty;
            tractor.RefillFuel();
			gasCanHeld.SetActive(false);
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
			hayInteractionAS.Play();
            state = PlayerState.HasHay;
			hayHeld.SetActive(true);
			return true; 
        }
		return false;
    }

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.tag == "Animal")
		{
			other.gameObject.GetComponent<Animal>().Highlighted();
			if (state == PlayerState.HasBale && Input.GetKeyDown(kbInteract))
			{
				state = PlayerState.Empty;
				other.gameObject.GetComponent<Animal>().FeedAnimal();
			}
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
