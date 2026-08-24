using UnityEngine;
using FMODUnity;

public class FmodEvents : MonoBehaviour
{
    // Singleton -------------------------------
    private static FmodEvents _instance;

    public static FmodEvents Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple FmodEvents scripts. Bad!");
        }
        else
        {
            _instance = this;
        }
    }
    // Singleton -------------------------------

    [field: Header("Door SFX")]
    [field: SerializeField] public EventReference openDoor { get ; private set;}

    [field: Header("VCAs")]
    [field: SerializeField] public string sfxVCAPath;
    [field: SerializeField] public string musicVCAPath;



}
