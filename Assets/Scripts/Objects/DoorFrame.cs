using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoorFrame : ClickableObject
{
    protected override void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (GameManager.Instance.state == GameManager.GameState.INNERROOM && transform.position.x - MainCameraMove.Instance.transform.position.x > 0.1f)
        {
            Vector3 finalPosition = new Vector3(transform.position.x, MainCameraMove.Instance.transform.position.y, GameManager.Instance.mainCameraZBeforeZoom);
            MainCameraMove.Instance.MoveCamera(finalPosition, GameManager.GameState.OUTERROOM);

            GameManager.Instance.state = GameManager.GameState.ZOOMING;
        }
    }

    protected override void OnMouseUp()
    {
        return;
    } 
}
