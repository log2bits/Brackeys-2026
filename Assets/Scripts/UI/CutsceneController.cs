using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private FlashbangEffect flashbangEffect;

    private int currentLine = 0;

    private void Start()
    {
        dialogueText.text = dialogueLines[currentLine];
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            NextLine();
        }
    }

    private void NextLine()
    {
        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            EndCutscene();
            return;
        }

        dialogueText.text = dialogueLines[currentLine];
    }

    private void EndCutscene()
    {
        gameObject.SetActive(false);
        flashbangEffect.TriggerFlashbang();
    }
}