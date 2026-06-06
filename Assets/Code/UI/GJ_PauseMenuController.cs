using EscapeVelocity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.FilePathAttribute;

public class GJ_PauseMenuController : MonoBehaviour
{
    public UITweening pauseMenuTween;
    public Button resumeButton;
    public Button settingsButton;
    public Button settingsBackButton;
    public UITweening settingsMenuTween;
    public Button quitButton;

    private bool paused;
    private InputAction pauseAction;
    private PlayerInput playerInput;

    void Start()
    {
        playerInput = GameObject.FindWithTag("Player").GetComponent<PlayerInput>();

        pauseAction = playerInput.actions.FindAction("Pause", true);
        pauseAction.performed += OnPause;
        settingsButton.onClick.AddListener(() => settingsMenuTween.TweenIn());
        settingsBackButton.onClick.AddListener(() => settingsMenuTween.TweenOut());
        quitButton.onClick.AddListener(OnQuit);
        resumeButton.onClick.AddListener(TogglePause);
    }

    private void OnQuit()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDisable()
    {
        if (pauseAction != null) pauseAction.performed -= OnPause;
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    private void TogglePause()
    {
        paused = !paused;

        if (paused)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            playerInput.SwitchCurrentActionMap("UI");
            pauseMenuTween.TweenIn();
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerInput.SwitchCurrentActionMap("Player");
            pauseMenuTween.TweenOut();
        }
    }
}
