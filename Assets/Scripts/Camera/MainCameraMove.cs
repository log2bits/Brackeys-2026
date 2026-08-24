using System.Collections;
using UnityEngine;

public class MainCameraMove : MonoBehaviour
{
    // Singleton -------------------------------
    private static MainCameraMove _instance;

    public static MainCameraMove Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple MainCameraMove scripts. Bad!");
        }
        else
        {
            _instance = this;
        }
    }
    // Singleton -------------------------------

    [SerializeField] private float cameraMoveTime;

    public void MoveCamera(Vector3 position)
    {
        CoroutineManager.Instance.Run(MoveCameraCoroutine(position));
    }

    private IEnumerator MoveCameraCoroutine(Vector3 position)
    {
        Vector3 startingPosition = transform.position;

        float i = 0;
        while (i < cameraMoveTime)
        {
            transform.position = Vector3.Lerp(startingPosition, position, Mathf.SmoothStep(0, 1, i / cameraMoveTime));
            yield return null;
            i += Time.deltaTime;
        }

        transform.position = position;
    }
}
