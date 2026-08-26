using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;
    
    private void OnEnable()
    {
        EventBus.Instance.Register(EventBus.EventName.LostLife, UpdateHeartImages);
    }

    private void OnDisable()
    {
        EventBus.Instance.Deregister(EventBus.EventName.LostLife, UpdateHeartImages);
    }

    private void UpdateHeartImages()
    {
        Debug.Log("alksdfbalkjsdf");
        int lives = GameManager.Instance.lives;
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < lives)
            {
                heartImages[i].sprite = heartFull;
            }
            else
            {
                heartImages[i].sprite = heartEmpty;
            }
        }
    }
}
