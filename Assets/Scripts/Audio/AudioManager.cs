using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    // Singleton -------------------------------
    private static AudioManager _instance;

    public static AudioManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple AudioManager scripts. Bad!");
        }
        else
        {
            _instance = this;
        }
    }
    // Singleton -------------------------------

    public void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
        RuntimeManager.PlayOneShot(sound, worldPosition);
    }
    public FMOD.Studio.EventInstance CreateInstance(EventReference sound)
    {
        FMOD.Studio.EventInstance instance = RuntimeManager.CreateInstance(sound);
        return instance;
    }

    public void PauseFmodSounds(bool pause)
    {        
        float pauseFloat = pause ? 1 : 0;
        RuntimeManager.StudioSystem.setParameterByName("PauseParameter", pauseFloat);
    }
    
    public void SetVCAVolume(string VCAPath, float volume)
    {
        RuntimeManager.GetVCA(VCAPath).setVolume(volume);
    }

}
