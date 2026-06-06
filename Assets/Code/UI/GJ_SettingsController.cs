using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UI;

public class GJ_SettingsController : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Controls Settings")]
    public InputActionReference lookAction;
    public Slider sensitivitySlider;
    public Toggle invertYToggle;

    public void SetMasterVolume()
    {
        float volume = masterSlider.value;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    void Start()
    {
        if(PlayerPrefs.HasKey("MasterVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMasterVolume();
            SetSFXVolume();
            SetMusicVolume();
        }
        masterSlider.onValueChanged.AddListener(delegate { SetMasterVolume(); });
        musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(); });
        sfxSlider.onValueChanged.AddListener(delegate { SetSFXVolume(); });

        sensitivitySlider.onValueChanged.AddListener(delegate { SetSensitivity(); });
        invertYToggle.onValueChanged.AddListener(delegate { SetYInvert(); });
        if (PlayerPrefs.HasKey("Sensitivity"))
        {
            LoadSensitivity();
        }
        else
        {
            SetSensitivity();
        }

        if (PlayerPrefs.HasKey("InvertY"))
        {
            LoadYInvert();
        }
        else
        {
            SetYInvert();
        }
    }
    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
        sensitivitySlider.onValueChanged.RemoveAllListeners();
        invertYToggle.onValueChanged.RemoveAllListeners();
    }

    private void SetSensitivity()
    {
        float sensitivity = sensitivitySlider.value;
        lookAction.action.ApplyParameterOverride((ScaleVector2Processor p) => p.x, sensitivity);
        lookAction.action.ApplyParameterOverride((ScaleVector2Processor p) => p.y, sensitivity);
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
    }

    private void LoadSensitivity()
    {
        sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1f);
        SetSensitivity();
    }

    private void SetYInvert()
    {
        bool invertY = invertYToggle.isOn;
        lookAction.action.ApplyParameterOverride((InvertVector2Processor p) => p.invertY, invertY);
        PlayerPrefs.SetString("InvertY", invertY.ToString());
    }

    private void LoadYInvert()
    {
        bool invertY = bool.Parse(PlayerPrefs.GetString("InvertY", "False"));
        invertYToggle.isOn = invertY;
        SetYInvert();
    }

    private void LoadVolume()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        SetMasterVolume();
        SetMusicVolume();
        SetSFXVolume();
    }
}
