using UnityEngine;

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
        if (!playerClicked && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
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