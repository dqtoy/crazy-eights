using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCore : MonoBehaviour
{
    public AudioClip cardSlideSound;                 // The sound when a card is slided
    public AudioClip cardPlaceSound;                 // The sound when a card is being played

    private AudioSource audioSource;

    // Singleton Code
    private static SoundCore _instance;
    public static SoundCore Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        // Make sure this is a singleton
        if (_instance == null)
        {
            _instance = this;
        }

        DontDestroyOnLoad(this);

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayCardPlaceSound()
    {
        audioSource.clip = cardPlaceSound;
        audioSource.Play();
    }

    public void PlayCardSlideSound()
    {
        audioSource.clip = cardSlideSound;
        audioSource.Play();
    }
    
}
