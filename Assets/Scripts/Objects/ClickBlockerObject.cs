using UnityEngine.EventSystems;

public class ClickBlockerObject : ClickableObject
{
    protected override void OnMouseDown()
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

    protected override void OnMouseUp()
    {
        return;
    } 
}
