using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private bool musicOn;
    private bool fxOn;

    public bool MusicOn
    {
        get => musicOn;
        set => musicOn = value;
    }

    public bool FxOn
    {
        get => fxOn;
        set => fxOn = value;
    }

    void Awake()
    {
        loadSound();
    }

    public void loadSound()
    {
        if (ES3.KeyExists("musicOn"))
        {
            MusicOn = ES3.Load<bool>("musicOn");
        }
        else
        {
            saveBool("musicOn", true);
        }

        if (ES3.KeyExists("fxOn"))
        {
            FxOn = ES3.Load<bool>("fxOn");
        }
        else
        {
            saveBool("fxOn", true);
        }
    }

    public void saveBool(string key, bool value)
    {
        ES3.Save(key, value);
    }
}