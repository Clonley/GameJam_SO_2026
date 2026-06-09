using UnityEngine;

public class GJ_AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource ambianceSource;
    public AudioClip musicClip;
    public AudioClip ambianceClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(musicSource != null) 
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }

        if(ambianceClip != null )
        {
            ambianceSource.clip = ambianceClip;
            ambianceSource.loop = true;
            ambianceSource.Play();
        }
    }
}
