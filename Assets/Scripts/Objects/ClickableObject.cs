using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    private void OnMouseDown(){
        Debug.Log("down");
    }

    private void OnMouseUp(){
        Debug.Log("up");
    }
}
