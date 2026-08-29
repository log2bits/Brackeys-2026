using UnityEngine;
using TMPro;
using System.Collections;
using System;
using UnityEngine.UI;

public class Notepad : MonoBehaviour
{
    [Header("Notepad References")]
    [SerializeField] private TMP_InputField notepadInputField;
    [SerializeField] private RectTransform notepadHolderTransform;
    [SerializeField] private Image notepadButtonImage;

    [Header("Guard Log References")]
    [SerializeField] private RectTransform guardLogHolderTransform;
    [SerializeField] private Image guardLogButtonImage;

    [Header("Parameters")]
    [SerializeField] private float verticalMoveDistance = 300;
    [SerializeField] private float moveTime = 0.5f;

    private bool notepadTransitioning = false;
    private bool notepadOpen = false;
    private bool guardLogTransitioning = false;
    private bool guardLogOpen = false;

    private void Start()
    {
        notepadOpen = false;

        guardLogOpen = false;
    }

    public void ToggleNotepad()
    {
        if (notepadTransitioning)
        {
            return;
        }

        notepadOpen = !notepadOpen;
        
        if (notepadOpen)
        {
            notepadHolderTransform.gameObject.SetActive(true);
            notepadInputField.ActivateInputField();
            notepadInputField.interactable = true;
        }
        else
        {
            notepadInputField.DeactivateInputField();
            notepadInputField.interactable = false;
        }

        float finalYPosition = notepadHolderTransform.anchoredPosition.y + (notepadOpen ? verticalMoveDistance : -1 * verticalMoveDistance);
        notepadTransitioning = true;
        CoroutineManager.Instance.Run(MoveRectTransformYAxis(notepadHolderTransform, finalYPosition, EndNotepadTransition));

        // Move guard log left/right
        //float finalXPosition = guardLogHolderTransform.anchoredPosition.x + (notepadOpen ? horizontalMoveDistance : -1 * horizontalMoveDistance);
        //CoroutineManager.Instance.Run(MoveRectTransformXAxis(guardLogHolderTransform, finalXPosition));
    }

    private void EndNotepadTransition()
    {
        notepadTransitioning = false;
    }

    public void ToggleGuardLog()
    {
        if (guardLogTransitioning)
        {
            return;
        }

        guardLogOpen = !guardLogOpen;
        
        if (guardLogOpen)
        {
            guardLogHolderTransform.gameObject.SetActive(true);
        }

        float finalYPosition = guardLogHolderTransform.anchoredPosition.y + (guardLogOpen ? verticalMoveDistance : -1 * verticalMoveDistance);
        guardLogTransitioning = true;
        CoroutineManager.Instance.Run(MoveRectTransformYAxis(guardLogHolderTransform, finalYPosition, EndGuardLogTransition));
    }

    private void EndGuardLogTransition()
    {
        guardLogTransitioning = false;
    }

    private IEnumerator MoveRectTransformYAxis(RectTransform transform, float finalYPosition, Action finished = null)
    {
        float startingYPosition = transform.anchoredPosition.y;

        float i = 0;
        while (i < moveTime)
        {
            transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, Mathf.Lerp(startingYPosition, finalYPosition, Mathf.SmoothStep(0, 1, i / moveTime)));
            yield return null;
            i += Time.deltaTime;
        }

        transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, finalYPosition);;
        finished?.Invoke();
    }

    /*private IEnumerator MoveRectTransformXAxis(RectTransform transform, float finalXPosition, Action finished = null)
    {
        float startingXPosition = transform.anchoredPosition.x;

        float i = 0;
        while (i < moveTime)
        {
            transform.anchoredPosition = new Vector2(Mathf.Lerp(startingXPosition, finalXPosition, Mathf.SmoothStep(0, 1, i / moveTime)), transform.anchoredPosition.y);
            yield return null;
            i += Time.deltaTime;
        }

        transform.anchoredPosition = new Vector2(finalXPosition, transform.anchoredPosition.y);;
        finished?.Invoke();
    }*/

    public void ClearNotepad()
    {
        notepadInputField.text = "";
    }
}