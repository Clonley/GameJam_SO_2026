using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GJ_PlayerStateScript : MonoBehaviour
{
    [Header("Death Animation")]
    public CanvasGroup m_deathImage;
    public CanvasGroup m_blackScreen;
    public float blackAnimationDuration = 1f;
    public float imageAnimationDuration = 2f;

    [Header("Win Animation")]
    public CanvasGroup m_winImage;
    public CanvasGroup m_whiteScreen;
    public float whiteAnimationDuration = 1f;
    public float WinImageAnimationDuration = 2f;

    private void Start()
    {
        m_deathImage.alpha = 0f;
        m_blackScreen.alpha = 0f;

        m_blackScreen.gameObject.SetActive(false);
        m_deathImage.gameObject.SetActive(false);
    }
    public void FuckingDie()
    {
        Debug.Log("Player has died");
        StartCoroutine(DeathSequence());
    }

    public void Win_willNeverDoThatInLife()
    {
        Debug.Log("Player has won, which I cannot ever in life...");
        StartCoroutine(WinSequence());
    }


    IEnumerator DeathSequence()
    {
        Time.timeScale = 0;

        m_blackScreen.gameObject.SetActive(true);
        m_deathImage.gameObject.SetActive(true);

        m_deathImage.alpha = 0f;
        m_blackScreen.alpha = 0f;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");

        asyncLoad.allowSceneActivation = false;

        float counter = 0f;

        while (counter < Mathf.Max(blackAnimationDuration, imageAnimationDuration))
        {
            counter += Time.unscaledDeltaTime;
            m_deathImage.alpha = Mathf.Lerp(0f, 1f, counter / imageAnimationDuration);
            m_blackScreen.alpha = Mathf.Lerp(0f, 1f, counter / blackAnimationDuration);

            yield return null;
        }
        while (asyncLoad.progress < 0.9f) yield return null;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        asyncLoad.allowSceneActivation = true;
    }

    IEnumerator WinSequence()
    {
        Time.timeScale = 0;

        m_whiteScreen.gameObject.SetActive(true);
        m_winImage.gameObject.SetActive(true);

        m_winImage.alpha = 0f;
        m_whiteScreen.alpha = 0f;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");

        asyncLoad.allowSceneActivation = false;

        float counter = 0f;

        while (counter < Mathf.Max(whiteAnimationDuration, WinImageAnimationDuration))
        {
            counter += Time.unscaledDeltaTime;
            m_winImage.alpha = Mathf.Lerp(0f, 1f, counter / WinImageAnimationDuration);
            m_whiteScreen.alpha = Mathf.Lerp(0f, 1f, counter / whiteAnimationDuration);

            yield return null;
        }
        while (asyncLoad.progress < 0.9f) yield return null;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        asyncLoad.allowSceneActivation = true;
    }
}
