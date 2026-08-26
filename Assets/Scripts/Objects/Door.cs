using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Door : ClickableObject
{
    [Header("References")]
    [SerializeField] private Sprite[] doorNumbers;
    [SerializeField] private Transform doorSpriteTransform;
    [SerializeField] private SpriteRenderer doorNumbersRenderer;

    [Header("Parameters")]
    [SerializeField] private float doorRotateTime;
    [SerializeField] private float doorRotateAngle;
    [SerializeField] private float doorZoomZDistance;

    private string dialogue = "";
    private bool safe = false;
    private bool hasTalkedBefore = false;
    private bool open; 

    protected override void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (open)
        {
            return;
        }

        // Zoom into door
        if (GameManager.Instance.state == GameManager.GameState.OUTERROOM)
        {
            GameManager.Instance.mainCameraZBeforeZoom = MainCameraMove.Instance.transform.position.z;

            Vector3 finalPosition = new Vector3(transform.position.x, MainCameraMove.Instance.transform.position.y, transform.position.z + doorZoomZDistance);
            MainCameraMove.Instance.MoveCamera(finalPosition, GameManager.GameState.INNERROOM);

            GameManager.Instance.state = GameManager.GameState.ZOOMING;

            Dialogue.Instance.StartDialogue(dialogue, !hasTalkedBefore, ZoomOut);
            hasTalkedBefore = true;
        }

        /*else if (GameManager.Instance.state == GameManager.GameState.INNERROOM)
        {
            // Zoom out if clicking on a door that isn't focused on
            if (Mathf.Abs(transform.position.x - MainCameraMove.Instance.transform.position.x) > 0.1f)
            {
                ZoomOut();
                return;
            }

            // Move to next room
            AudioManager.Instance.PlayOneShot(FmodEvents.Instance.openDoor, transform.position);
            MainCameraMove.Instance.MoveCamera(transform.position, GameManager.GameState.OUTERROOM);
            open = true;

            CoroutineManager.Instance.Run(RotateDoor(doorRotateAngle));
            GameManager.Instance.state = GameManager.GameState.TRANSITIONROOM;
        }*/
    }

    private void ZoomOut()
    {
        Vector3 finalPosition = new Vector3(transform.position.x, MainCameraMove.Instance.transform.position.y, GameManager.Instance.mainCameraZBeforeZoom);
        MainCameraMove.Instance.MoveCamera(finalPosition, GameManager.GameState.OUTERROOM);

        GameManager.Instance.state = GameManager.GameState.ZOOMING;
    }

    protected override void OnMouseUp()
    {
        return;
    }

    public void SetDialogue(string dialogue)
    {
        this.dialogue = dialogue;
    }
    public void SetIsSafe(bool IsSafe = false)
    {
        this.safe = IsSafe;
    }

    public void SetNumber(int number)
    {
        if (number >= doorNumbers.Length)
        {
            Debug.LogWarning("Not enough door numbers on door prefab!");
            return;
        }
        doorNumbersRenderer.sprite = doorNumbers[number];
    }

    private IEnumerator RotateDoor(float rotateAmount)
    {
        Vector3 startRotation = doorSpriteTransform.eulerAngles;
        Vector3 finalRotation = new Vector3(doorSpriteTransform.eulerAngles.x, doorSpriteTransform.eulerAngles.y + rotateAmount, doorSpriteTransform.eulerAngles.z);
        
        float i = 0;
        while (i < doorRotateTime)
        {
            doorSpriteTransform.eulerAngles = Vector3.Lerp(startRotation, finalRotation, Mathf.SmoothStep(0, 1, i / doorRotateTime));
            yield return null;
            i += Time.deltaTime;
        }

        doorSpriteTransform.eulerAngles = finalRotation;
    }
    
}
