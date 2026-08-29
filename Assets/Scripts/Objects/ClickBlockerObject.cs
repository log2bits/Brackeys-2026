using UnityEngine;
using UnityEngine.EventSystems;

public class ClickBlockerObject : MonoBehaviour, IClickableObject
{
    public void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (GameManager.Instance.state == GameManager.GameState.INNERROOM)
        {
            Dialogue.Instance.DeferClickCheckToDialogue();
        }
    }

    public void OnMouseUp()
    {
        return;
    }

    public void OnMouseOver()
    {
        return;
    }

    public void OnMouseExit()
    {
        return;
    }
}
