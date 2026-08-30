using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CutsceneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private FlashbangEffect flashbangEffect;
    [SerializeField] private RectTransform friendlyFace;

    [Header("Parameters")]
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private float textShowDelay = 0.04f;
    [SerializeField] private float friendlyFaceRotateSpeed = 1f;
    [SerializeField] private float friendlyFaceRotateAmplitude = 1f;

    private int currentLine = 0;
    private int charactersShown = 0;
    private float timeLastCharacterShown;
    private bool dialogueTextAppearing;
    private bool cutsceneEnded = false;

    private void Start()
    {
        ShowLine();
    }

    private void Update()
    {
        if (cutsceneEnded)
        {
            return;
        }

        friendlyFace.localEulerAngles = new Vector3(0, 0, friendlyFaceRotateAmplitude * Mathf.Sin(Time.time * friendlyFaceRotateSpeed));

        // Slowly reveal the current line
        if (dialogueTextAppearing)
        {
            if (Time.time - timeLastCharacterShown > textShowDelay)
            {
                timeLastCharacterShown += textShowDelay;
                charactersShown++;
            }

            if (charactersShown >= dialogueText.text.Length)
            {
                charactersShown = dialogueText.text.Length;
                dialogueTextAppearing = false;
            }

            dialogueText.maxVisibleCharacters = charactersShown;
        }

        // Left or right mouse click
        if (Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        // If text is still appearing, finish the line immediately
        if (dialogueTextAppearing)
        {
            charactersShown = dialogueText.text.Length;
            dialogueText.maxVisibleCharacters = charactersShown;
            dialogueTextAppearing = false;

            return;
        }

        // Otherwise, go to the next line
        NextLine();
    }

    private void ShowLine()
    {
        dialogueText.text = dialogueLines[currentLine];

        // Start with no characters visible
        charactersShown = 0;
        dialogueText.maxVisibleCharacters = 0;
        dialogueTextAppearing = true;

        timeLastCharacterShown = Time.time;
    }

    private void NextLine()
    {
        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            EndCutscene();
            return;
        }

        ShowLine();
    }

    private void EndCutscene()
    {
        cutsceneEnded = true;
        flashbangEffect.TriggerFlashbang();
    }
}