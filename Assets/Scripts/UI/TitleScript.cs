using UnityEngine;
using UnityEngine.UI;

public class TitleScript : MonoBehaviour
{
    public static bool loaded = false;

    private GUIStyle buttonStyle;
    public Text title;
    public Image img;
    public Button playButton;
    public Button quitButton;
    
    float imgFade;
    float imgDifference;

    float textFade;
    float textDifference;

    int waitTimer;
    int waitTimerLength;

    // Use this for initialization
    void Start () {
        if (!loaded)
        {
            Color temp = title.color;
            temp.a = 0.0f;
            title.color = temp;

            imgFade = 1;
            imgDifference = 0.3f;

            textFade = 0;
            textDifference = 0.6f;

            waitTimer = 0;
            waitTimerLength = 100;

            playButton.gameObject.SetActive(false);
            quitButton.gameObject.SetActive(false);

            loaded = true;
        }
    }


    // Update is called once per frame
    void FixedUpdate ()
    {
        if (waitTimer < waitTimerLength)
        {
            waitTimer += 1;
        }
        else if (img.color.a > 0.01f)
        {
            imgFade -= imgDifference * Time.deltaTime;
            if (imgFade < 0)
            {
                imgFade = 0;
            }

            Color temp = img.color;
            temp.a = imgFade;
            img.color = temp;
        }

        if (img.color.a < 0.01f)
        {
            textFade += textDifference * Time.deltaTime;
            if (textFade > 1)
            {
                textFade = 1;
            }

            Color temp = title.color;
            temp.a = textFade;
            title.color = temp;
        }

        if (title.color.a > 0.99f) {
            playButton.gameObject.SetActive(true);
            quitButton.gameObject.SetActive(true);
        }
    }   
    
}