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

    [Header("Parameters")]
    [SerializeField] private float cameraWallMargin;
    [SerializeField] private float cameraMoveTime;
    [SerializeField] private float cameraDragStrength = 0.2f;

    private bool inputEnabled = true;
    private bool mouseHeldLastFrame = false;

    private void Update()
    {
        if (!inputEnabled)
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
            transform.position = new Vector3(transform.position.x + (mouseDelta.x * cameraDragStrength), transform.position.y, transform.position.z);
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
        float clampedX = Mathf.Clamp(transform.position.x, currentRoomState.globalPosition.x + cameraWallMargin - (currentRoomState.roomWidth/2), currentRoomState.globalPosition.x - cameraWallMargin + (currentRoomState.roomWidth/2));
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
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

        transform.position = position;
        inputEnabled = true;

        GameManager.Instance.state = finalState;
    }
}
