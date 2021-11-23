using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public bool fxOn;
    public bool musicOn;

    private AudioSource audioSource;

    public AudioClip beenHitSound;
    public AudioClip bigHitSound;
    public AudioClip massiveHitSound;
    public AudioClip lilHitSound;
    public AudioClip hitSound;
    public AudioClip dashRenew;
    public AudioClip stepSound;
    public AudioClip mainTheme;
    public AudioClip bossTheme;
    public Text fxToggleText;
    public Text musicToggleText;
    private SaveSystem saveSystem;

    void Start()
    {
        saveSystem = FindObjectOfType<SaveSystem>().GetComponent<SaveSystem>();
        audioSource = GetComponent<AudioSource>();
        musicOn = saveSystem.MusicOn;
        fxOn = saveSystem.FxOn;
        playMainTheme();
        updateFxText();
        updateMusicText();
    }

    void playMainTheme()
    {
        if (musicOn)
        {
            audioSource.clip = mainTheme;
            audioSource.Play();
        }
    }

    public void playBossTheme()
    {
        if (musicOn)
        {
            audioSource.clip = bossTheme;
            audioSource.Play();
        }
    }

    void updateFxText()
    {
        fxToggleText.text = fxOn ? "Fx On" : "Fx Off";
    }

    public void toggleFx()
    {
        if (saveSystem.FxOn)
        {
            saveSystem.FxOn = false;
        }
        else
        {
            saveSystem.FxOn = true;
        }

        saveSystem.saveBool("fxOn", saveSystem.FxOn);
        fxOn = saveSystem.FxOn;
        updateFxText();
    }

    void updateMusicText()
    {
        musicToggleText.text = musicOn ? "Music On" : "Music Off";
    }

    public void toggleMusic()
    {
        if (musicOn)
        {
            audioSource.Stop();
            saveSystem.MusicOn = false;
        }
        else
        {
            audioSource.Play();
            saveSystem.MusicOn = true;
        }

        saveSystem.saveBool("musicOn", saveSystem.MusicOn);
        musicOn = saveSystem.MusicOn;
        updateMusicText();
    }

    public void playDashRenewSound()
    {
        audioSource.PlayOneShot(dashRenew);
    }

    public void playBigHitSound()
    {
        audioSource.PlayOneShot(bigHitSound);
    }

    public void playLilHitSound()
    {
        audioSource.PlayOneShot(lilHitSound);
    }

    public void playMassiveHitSound()
    {
        audioSource.PlayOneShot(massiveHitSound);
    }

    public void playHitSound()
    {
        audioSource.PlayOneShot(hitSound);
    }

    public void playStepSound()
    {
        audioSource.volume = 0.25f;
        audioSource.PlayOneShot(stepSound);
        audioSource.volume = 0.5f;
    }
}