using UnityEngine;
using UnityEngine.EventSystems;

public class GJ_SettingsButtonSelection : MonoBehaviour
{

    public GameObject fallbackGameObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    private void OnDisable()
    {
        EventSystem.current.SetSelectedGameObject(fallbackGameObject);
    }
}
