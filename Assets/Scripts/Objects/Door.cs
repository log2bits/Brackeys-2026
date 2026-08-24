using System;
using UnityEngine;

public class Door : ClickableObject
{
    [Header("References")]
    [SerializeField] private Sprite[] doorNumbers;
    [SerializeField] private SpriteRenderer doorNumbersRenderer;

    protected override void OnMouseDown()
    {
        AudioManager.Instance.PlayOneShot(FmodEvents.Instance.openDoor, transform.position);
        MainCameraMove.Instance.MoveCamera(transform.position);
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
}
