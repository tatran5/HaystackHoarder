using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialState { Welcome, CharacterMovement, EnterExitTractor, RefillFuel, HarvestHay, 
    ProcessHay, FeedAnimals, AdditionalMessage, None }
public class GlobalTutorial : MonoBehaviour
{
    public TutorialState lastState = TutorialState.None;
    public TutorialState state = TutorialState.Welcome;
    public Text tutorialText;
    private KeyCode readyKey = KeyCode.R;

    public Material highlightMaterial;
    private Material tempMaterial;
    private bool textIsUpdated = false;

    // Start is called before the first frame update
    void Start()
    { }

    void HandleEndState() { }
    void HandleAdditionalMessageState() { }
    void HandleEndTutorial() { }
    void HandleProcessHayState() 
    {
        if (!textIsUpdated)
        {
            tutorialText.text = "Get out of your tractor. Then, unload hay from the tractor by using " + 
                ControllableObject.kbInteract.ToString() + " key, and carry hay to the barn. Once you're there, press " +
                ControllableObject.kbInteract.ToString() + " again to process hay into bale. Wait a bit for the process to complete. " +
                "Press " + readyKey.ToString() + " once it's done.";
        }
        else if (Input.GetKeyDown(readyKey))
        {
            textIsUpdated = false;
            UnhighlightObject(GameObject.FindGameObjectsWithTag("FuelStation")[0]);
            UpdateStates(state, TutorialState.HarvestHay);
        }
    }
    void HandleRefillFuelState() 
    {
        if (!textIsUpdated)
        {
            textIsUpdated = true;
            tutorialText.text = "Can't move your tractor? Your tractor is out of fuel. Get out of the tractor and go to the fuel station, press "
                + ControllableObject.kbInteract.ToString() + " to get fuel, then run back to your tractor and press the same key to refill its fuel.\n"
                + "Press " + readyKey.ToString() + " once you have done so.";
            HighlightObject(GameObject.FindGameObjectsWithTag("FuelStation")[0]);
        }
        else if (Input.GetKeyDown(readyKey))
        {
            textIsUpdated = false;
            UnhighlightObject(GameObject.FindGameObjectsWithTag("FuelStation")[0]);
            UpdateStates(state, TutorialState.HarvestHay);
        }
    }
    
    void HandleHarvestHayState()
    {
        if (!textIsUpdated)
        {
            textIsUpdated = true;
            tutorialText.text = "Let's harvest some hay with the tractor. Go to the haystack and press " + 
                ControllableObject.kbInteract + ". Don't release it until the bar is full and disappear! You will lose the hay being collected in the process." +
                "Press " + readyKey.ToString() + " once you're up for the next piece of advice.";
            HighlightObject(GameObject.FindGameObjectsWithTag("Haystack")[0]);
        }
        else if (Input.GetKeyDown(readyKey))
        {
            textIsUpdated = false;
            UnhighlightObject(GameObject.FindGameObjectsWithTag("Haystack")[0]);
            UpdateStates(state, TutorialState.ProcessHay);
        }
    }
    void HandleEnterExitTractorState()
    {
        if (!textIsUpdated)
        {
            textIsUpdated = true;
            HighlightObject(GameObject.FindGameObjectsWithTag("Tractor")[0]);
            tutorialText.text = "Let's go to the tractor and press "
               + ControllableObject.kbEnterExitTractor.ToString() + ". "
               + "You can enter and exit the tractor by pressing this key, and go around in the tractor by using the same keys to move the player.\n"
               + "One thing to note, you cannot use a tractor not belonged to your team.\n"
               + "Press " + readyKey.ToString() + " once you're ready to explore more!";
        }
        else if (Input.GetKeyDown(readyKey))
        {
            textIsUpdated = false;
            GameObject tractorGO = GameObject.FindGameObjectsWithTag("Tractor")[0];
            UnhighlightObject(tractorGO);

            Tractor tractor = tractorGO.GetComponent<Tractor>();
            if (tractor.HasFuel()) UpdateStates(state, TutorialState.HarvestHay);
            else UpdateStates(TutorialState.HarvestHay, TutorialState.RefillFuel);
        }
    }
   
    void HandleCharacterMovementState()
    {
        if (!textIsUpdated)
        {
            HighlightObject(GameObject.FindGameObjectsWithTag("Player")[0]);
            tutorialText.text = "To move your character around, press "
                + ControllableObject.kbMoveLeft.ToString() + ", "
                + ControllableObject.kbMoveRight.ToString() + ", "
                + ControllableObject.kbMoveForward.ToString() + ", "
                + ControllableObject.kbMoveBackward.ToString() + "!"
                + "\nOnce you're familiar with the character movement, press "
                + readyKey.ToString() + " again to move to the next part!";
            textIsUpdated = true;
        }
        else if (Input.GetKeyDown(readyKey))
        {
            UpdateStates(state, TutorialState.EnterExitTractor);
            UnhighlightObject(GameObject.FindGameObjectsWithTag("Player")[0]);
            textIsUpdated = false;
        }
    }

    void HandleWelcomeState()
    {
        if (!textIsUpdated)
        {
            tutorialText.text = "Welcome to Haystack Hoarder! Let's familarize ourselves with the game, shall we?\nPress "
                + readyKey.ToString() + " once you're ready!";
            textIsUpdated = true;
        }
        else if (Input.GetKeyDown(readyKey))
        {
            UpdateStates(state, TutorialState.CharacterMovement);
            textIsUpdated = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (state == TutorialState.Welcome)
            HandleWelcomeState();
        else if (state == TutorialState.CharacterMovement)
            HandleCharacterMovementState();
        else if (state == TutorialState.EnterExitTractor)
            HandleEnterExitTractorState();
        else if (state == TutorialState.RefillFuel)
            HandleRefillFuelState();
        else if (state == TutorialState.HarvestHay)
            HandleHarvestHayState();
        else if (state == TutorialState.ProcessHay)
            HandleProcessHayState();
        else if (state == TutorialState.AdditionalMessage)
            HandleAdditionalMessageState();
        else
            HandleEndState();
    }

    void UpdateStates(TutorialState lastState, TutorialState newState)
    {
        this.lastState = lastState;
        state = newState;
    }

    void HighlightObject(GameObject o)
    {
        tempMaterial = o.GetComponent<MeshRenderer>().material;
        o.GetComponent<MeshRenderer>().material = highlightMaterial;
    }

    void UnhighlightObject(GameObject o)
    {
        o.GetComponent<MeshRenderer>().material = tempMaterial;
    }

}
