using UnityEngine;
using UnityEngine.InputSystem;

public class InitialMusicPlayer: MonoBehaviour
{
    private bool playerClicked = false;

    private void Start()
    {
        // If not web build play music immediately
        #if !UNITY_WEBGL
        PlayerHasClicked();
        #endif
    }

    private void Update()
    {
        bool pressed = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) || (Pointer.current != null && Pointer.current.press.wasPressedThisFrame);
        if (!playerClicked && pressed)
        {
            playerClicked = true;
            EventBus.Instance.DoPlayerClicked();
            this.enabled = false;
        }
    }

    private void PlayerHasClicked()
    {
        if (playerClicked) return;
        playerClicked = true;

        this.enabled = false;
    }

}