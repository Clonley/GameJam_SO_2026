using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EscapeVelocity
{
    public class EV_UIController_MainMenu : MonoBehaviour
    {
        public UITweening settingsTween;
        public Button settingsAudioButton;
        public Button playButton;
        public GameObject menuButtons;
        public void QuitGame()
        {
            Application.Quit();
        }

        public void StartGame()
        {
            StartCoroutine(LoadLevel());
        }

        public void OpenSettings()
        {
            menuButtons.SetActive(false);
            settingsTween.TweenIn();
            EventSystem.current.SetSelectedGameObject(settingsAudioButton.gameObject);
        }
        public void CloseSettings()
        {
            menuButtons.SetActive(true);
            settingsTween.TweenOut();
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        IEnumerator LoadLevel()
        {
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("GameplayScene");
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }
}
