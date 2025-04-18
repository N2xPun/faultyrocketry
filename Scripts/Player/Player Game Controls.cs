using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerGameControls : MonoBehaviour
{
    [SerializeField] private GameObject pauseScreen;
    private PlayerMovement pmv;

    private PlayerInput playerInput;
    private InputAction pause;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        pmv = GetComponent<PlayerMovement>();

        Cursor.lockState = CursorLockMode.Locked;

        pause = playerInput.actions.FindAction("Pause");
    }

    // Update is called once per frame
    private void Update()
    {
        if (pause.triggered)
        {
            if (pauseScreen.activeInHierarchy)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }
    }

    private void Pause()
    {
        pauseScreen.SetActive(true);
        Time.timeScale = 0f;
        pmv.enabled = false;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Unpause()
    {
        pauseScreen.SetActive(false);
        Time.timeScale = 1f;
        pmv.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Continue()
    {
        Unpause();
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
