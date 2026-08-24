using System;
using System.Collections;
using UnityEngine;

public class Door : ClickableObject
{
    [Header("References")]
    [SerializeField] private Sprite[] doorNumbers;
    [SerializeField] private Transform doorSpriteTransform;
    [SerializeField] private SpriteRenderer doorNumbersRenderer;

    [Header("Parameters")]
    [SerializeField] private float doorRotateTime;
    [SerializeField] private float doorRotateAngle;

    private bool open; 

    protected override void OnMouseDown()
    {
        if (open)
        {
            return;
        }

        AudioManager.Instance.PlayOneShot(FmodEvents.Instance.openDoor, transform.position);
        MainCameraMove.Instance.MoveCamera(transform.position);
        open = true;

        CoroutineManager.Instance.Run(RotateDoor(doorRotateAngle));
    }

    protected override void OnMouseUp()
    {
        return;
    }

    public void SetNumber(int number)
    {
        if (number >= doorNumbers.Length)
        {
            throw new Exception("Not enough door numbers on door prefab!");
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
