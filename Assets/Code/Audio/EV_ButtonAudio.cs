using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class EV_ButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip hoverClip;
    [SerializeField]
    private AudioClip pressClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        audioSource.PlayOneShot(hoverClip);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        audioSource.PlayOneShot(pressClip);
    }
}
