using System;
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

        basePosition = transform.localPosition;
        lastBasePosition = basePosition;
        baseRotation = transform.localRotation;
    }
    // Singleton -------------------------------

    [Header("References")]
    [SerializeField] private GameObject cameraShakeGameobject;

    [Header("Parameters")]
    [SerializeField] private bool movementEnabled = true;
    [SerializeField] private float cameraWallMargin;
    [SerializeField] private float cameraMoveTime;
    [SerializeField] private float cameraDragStrength = 0.2f;

    [Header("Bobbing Parameters")]
    [SerializeField] private float bobbingAmplitude = 0.15f;
    [SerializeField] private float swayAmplitude = 0.5f;
    [SerializeField] private float bobCyclesPerUnit = 0.075f;
    [SerializeField] private float bobReferenceSpeed = 8f;
    [SerializeField] private float bobBlendSharpness = 8f;

    private bool inputEnabled = true;
    private bool isSliding = false;
    private bool mouseHeldLastFrame = false;
    private IEnumerator lastShakeCoroutine;

    private Vector3 basePosition;
    private Vector3 lastBasePosition;
    private Quaternion baseRotation;
    private float bobPhase;
    private float bobWeight;

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
        ShakeCamera(0.45f, 0.3f, 0.015f, 0.8f);
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

        isSliding = false;
        if (mouseHeldLastFrame)
        {
            basePosition = new Vector3(basePosition.x + (mouseDelta.x * cameraDragStrength), basePosition.y, basePosition.z);
            isSliding = true;
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
        float clampedX = Mathf.Clamp(basePosition.x, currentRoomState.globalPosition.x + cameraWallMargin - (currentRoomState.roomWidth / 2), currentRoomState.globalPosition.x - cameraWallMargin + (currentRoomState.roomWidth / 2));
        basePosition = new Vector3(clampedX, basePosition.y, basePosition.z);
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f)
        {
            return;
        }

        float distance = Vector3.Distance(basePosition, lastBasePosition);
        float speed = distance / dt;

        bobPhase = Mathf.Repeat(bobPhase + distance * bobCyclesPerUnit / (isSliding ? 2.0f : 1.0f) * 2f * Mathf.PI, 2f * Mathf.PI);

        float target = Mathf.Clamp01(speed / bobReferenceSpeed);
        bobWeight = Mathf.Lerp(bobWeight, target, 1f - Mathf.Exp(-bobBlendSharpness * dt));

        if (bobWeight < 0.01f)
        {
            bobPhase = 0f;
        }

        float bob = bobbingAmplitude * Mathf.Sin(bobPhase * 2f) * bobWeight;
        float sway = swayAmplitude * Mathf.Sin(bobPhase) * bobWeight;

        transform.localPosition = basePosition + Vector3.up * bob;
        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, sway);

        lastBasePosition = basePosition;
    }

    public void MoveCamera(Vector3 position, GameManager.GameState finalState)
    {
        CoroutineManager.Instance.Run(MoveCameraCoroutine(position, finalState));
    }

    private IEnumerator MoveCameraCoroutine(Vector3 position, GameManager.GameState finalState)
    {
        inputEnabled = false;
        Vector3 startingPosition = basePosition;

        float i = 0f;
        while (i < cameraMoveTime)
        {
            basePosition = Vector3.Lerp(startingPosition, position, Mathf.SmoothStep(0f, 1f, i / cameraMoveTime));
            yield return null;
            i += Time.deltaTime;
        }

        basePosition = position;
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
                cameraShakeGameobject.transform.position = UnityEngine.Random.insideUnitSphere * shakeAmount;
                shakeAmount *= shakeDecay;
                lastShakeTime = Time.time;
            }
            yield return null;
        }
        cameraShakeGameobject.transform.position = Vector3.zero;
    }
}