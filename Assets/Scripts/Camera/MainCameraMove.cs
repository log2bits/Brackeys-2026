using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    [Header("References")]
    [SerializeField] private GameObject cameraShakeGameobject;

    [Header("Parameters")]
    [SerializeField] private bool movementEnabled = true;
    [SerializeField] private float cameraWallMargin;
    [SerializeField] private float cameraMoveTime;
    [SerializeField] private float cameraDragStrength = 0.2f;

    private bool inputEnabled = true;
    private bool mouseHeldLastFrame = false;
    private IEnumerator lastShakeCoroutine;

    private void OnEnable()
    {
        EventBus.Instance.Register(EventBus.EventName.LostLife, LostLifeShake);
    }

    private void OnDisable()
    {
        EventBus.Instance.Deregister(EventBus.EventName.LostLife, LostLifeShake);
    }

    private void LostLifeShake()
    {
        // Magic numbers go brrrrr
        ShakeCamera(0.35f, 0.3f, 0.015f, 0.85f);
    }

    private void Update()
    {
        if (!inputEnabled || !movementEnabled)
        {
            return;
        }

        if (Time.timeScale == 0)
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (GameManager.Instance.state != GameManager.GameState.OUTERROOM)
        {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        if (mouseHeldLastFrame)
        {
            transform.localPosition = new Vector3(transform.localPosition.x + (mouseDelta.x * cameraDragStrength), transform.localPosition.y, transform.localPosition.z);
        }

        mouseHeldLastFrame = Mouse.current.leftButton.isPressed;

        // Clamp camera within room bounds
        int currentRoom = GameManager.Instance.currentRoom;
        if (currentRoom >= GameManager.Instance.worldState.roomStates.Count)
        {
            currentRoom = GameManager.Instance.worldState.roomStates.Count - 1;
        }

        RoomState currentRoomState = GameManager.Instance.worldState.roomStates[currentRoom];
        if (currentRoomState == null)
        {
            throw new System.Exception("MainCameraMove: Game currently inside an invalid room!");
        }
        float clampedX = Mathf.Clamp(transform.localPosition.x, currentRoomState.globalPosition.x + cameraWallMargin - (currentRoomState.roomWidth/2), currentRoomState.globalPosition.x - cameraWallMargin + (currentRoomState.roomWidth/2));
        transform.localPosition = new Vector3(clampedX, transform.localPosition.y, transform.localPosition.z);
    }

    public void MoveCamera(Vector3 position, GameManager.GameState finalState)
    {
        CoroutineManager.Instance.Run(MoveCameraCoroutine(position, finalState));
    }

    private IEnumerator MoveCameraCoroutine(Vector3 position, GameManager.GameState finalState)
    {
        inputEnabled = false;
        Vector3 startingPosition = transform.position;

        float i = 0;
        while (i < cameraMoveTime)
        {
            transform.position = Vector3.Lerp(startingPosition, position, Mathf.SmoothStep(0, 1, i / cameraMoveTime));
            yield return null;
            i += Time.deltaTime;
        }

        transform.localPosition = position;
        inputEnabled = true;

        GameManager.Instance.state = finalState;
    }

    public void ShakeCamera(float shakeDuration, float shakeAmount, float timeBetweenShakes, float shakeDecay)
	{
        if (lastShakeCoroutine != null)
        {
		    CoroutineManager.Instance.Stop(lastShakeCoroutine);
        }

        lastShakeCoroutine = ShakeCameraCoroutine(shakeDuration, shakeAmount, timeBetweenShakes, shakeDecay);
        CoroutineManager.Instance.Run(lastShakeCoroutine);
	}

    private IEnumerator ShakeCameraCoroutine(float shakeDuration, float shakeAmount, float timeBetweenShakes, float shakeDecay)
	{
		float shakeStartTime = Time.time;
        float lastShakeTime = 0f;
        while (Time.time - shakeStartTime < shakeDuration)
		{
            if (Time.time - lastShakeTime > timeBetweenShakes)
			{
				cameraShakeGameobject.transform.position = Random.insideUnitSphere * shakeAmount;
                shakeAmount *= shakeDecay;
                lastShakeTime = Time.time;
			}
			yield return null;
		}
        cameraShakeGameobject.transform.position = Vector3.zero;
	}
}
