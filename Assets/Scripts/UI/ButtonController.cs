using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public void LoadLobby()
    {
        SceneManager.LoadScene("GameLobby");
    }

    public void LoadTitle() {
        SceneManager.LoadScene("TitleScene");
    }

    public void Quit() {
        Application.Quit();
    }
}
