using UnityEngine;
using System.Collections;

public class CoroutineManager : MonoBehaviour
{
    // Singleton -------------------------------
    private static CoroutineManager _instance;

    public static CoroutineManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple CoroutineManager scripts. Bad!");
        }
        else
        {
            _instance = this;
        }
    }
    // Singleton -------------------------------

    public void Run(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public void Stop(IEnumerator coroutine)
    {
        StopCoroutine(coroutine);
    }
}
