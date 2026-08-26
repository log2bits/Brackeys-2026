using UnityEngine;
using TMPro;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class Dialogue : MonoBehaviour
{
    // Singleton -------------------------------
    private static Dialogue _instance;

    public static Dialogue Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple Dialogue scripts. Bad!");
        }
        else
        {
            _instance = this;
        }
    }
    // Singleton -------------------------------

    [Header("References")]
    [SerializeField] private RectTransform dialogueBoxTransform;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Parameters")]
    [SerializeField] private float dialogueBoxMoveTime;
    [SerializeField] private float dialogueBoxVisibleY;
    [SerializeField] private float dialogueBoxHiddenY;
    [SerializeField] private float dialogueTextShowDelay;
    
    private string currentDialogueText;
    private Action currentDialogueCompleteAction;
    private bool inDialogue;
    private bool dialogueBoxMoving;
    private bool dialogueTextAppearing;
    private int charactersShown;
    private float timeLastCharacterShown;
    
    private void Start()
    {
        dialogueBoxTransform.anchoredPosition = new Vector2(dialogueBoxTransform.anchoredPosition.x, dialogueBoxHiddenY);
    }

    private void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (!inDialogue || dialogueBoxMoving)
        {
            return;
        }

        if (dialogueTextAppearing)
        {
            if (Time.time - timeLastCharacterShown > dialogueTextShowDelay)
            {
                timeLastCharacterShown += dialogueTextShowDelay;
                charactersShown += 1;
            }

            if (charactersShown >= currentDialogueText.Length)
            {
                dialogueTextAppearing = false;
            }

            dialogueText.maxVisibleCharacters = charactersShown;
            return;
        }
    }

    public void DeferClickCheckToDialogue()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ClickDialogue();
        }
    }
    
    public void ClickDialogue()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (!inDialogue || dialogueBoxMoving)
        {
            return;
        }

        if (dialogueTextAppearing)
        {
            charactersShown = currentDialogueText.Length;
            dialogueText.maxVisibleCharacters = charactersShown;
        }
        else
        {
            EndDialogue();
        }
    }

    public bool StartDialogue(string dialogue, bool graduallyShowText, Action dialogueComplete = null)
    {
        if (inDialogue || dialogueBoxMoving || dialogueTextAppearing)
        {
            return false;
        }

        currentDialogueCompleteAction = dialogueComplete;

        currentDialogueText = dialogue;
        dialogueText.text = currentDialogueText;
        charactersShown = graduallyShowText ? 0 : currentDialogueText.Length;
        dialogueText.maxVisibleCharacters = charactersShown;

        inDialogue = true;
        CoroutineManager.Instance.Run(MoveDialogueBox(dialogueBoxHiddenY, dialogueBoxVisibleY, dialogueBoxMoveTime, FinishStartDialogue));
        return true;
    }

    private void FinishStartDialogue()
    {
        if (charactersShown == 0)
        {
            dialogueTextAppearing = true;
            timeLastCharacterShown = Time.time;
        }
    }

    public void EndDialogue(bool moveCamera = true)
    {
        if (!inDialogue)
        {
            return;
        }

        inDialogue = false;
        dialogueTextAppearing = false;
        CoroutineManager.Instance.Run(MoveDialogueBox(dialogueBoxVisibleY, dialogueBoxHiddenY, dialogueBoxMoveTime));

        if (!moveCamera)
        {
            return;
        }
        
        currentDialogueCompleteAction?.Invoke();
    }

    private IEnumerator MoveDialogueBox(float startingY, float finishedY, float time, Action moveCompleted = null)
    {
        dialogueBoxMoving = true;
        float timeStartedMove = Time.time;
        float ySet;

        while (Time.time - timeStartedMove < time)
        {
            yield return null;

            ySet = Mathf.SmoothStep(startingY, finishedY, (Time.time - timeStartedMove) / time);
            dialogueBoxTransform.anchoredPosition = new Vector2(dialogueBoxTransform.anchoredPosition.x, ySet);
        }

        dialogueBoxMoving = false;
        moveCompleted?.Invoke();
    }
}
