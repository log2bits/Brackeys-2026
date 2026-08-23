using UnityEngine;

public abstract class ClickableObject : MonoBehaviour
{
    protected abstract void OnMouseDown();

    protected abstract void OnMouseUp();

}
