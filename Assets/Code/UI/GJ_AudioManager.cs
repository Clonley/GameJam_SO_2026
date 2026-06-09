using UnityEngine;

public class GJ_AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource ambianceSource;
    public AudioSource oneShotSource;
    public AudioClip musicClip;
    public AudioClip ambianceClip;
    
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


    public void PlayOneshot(AudioClip clip)
    {
        if (clip != null)
        {
            oneShotSource.PlayOneShot(clip);
        }
    }
}
