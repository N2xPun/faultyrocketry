using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Level0");
    }

    public void QuitGame()
    {
        Application.Quit(0);
    }
}
