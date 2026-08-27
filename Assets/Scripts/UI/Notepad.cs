using UnityEngine;
using TMPro;
using System.Collections;
using System;
using UnityEngine.UI;

public class Notepad : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject notepadHolder;
    [SerializeField] private RectTransform notepadHolderTransform;
    [SerializeField] private Image notepadButtonImage;
    [SerializeField] private Sprite notepadButtonOpen;
    [SerializeField] private Sprite notepadButtonClose;

    [Header("Parameters")]
    [SerializeField] private float notepadMoveDistance = 300;
    [SerializeField] private float notepadMoveTime = 0.5f;

    private bool transitioning = false;
    private bool notepadOpen = false;

    private void Start()
    {
        notepadHolder.SetActive(false);
    }

    public void ToggleNotepad()
    {
        if (transitioning)
        {
            return;
        }

        notepadOpen = !notepadOpen;

        notepadButtonImage.sprite = notepadOpen ? notepadButtonClose : notepadButtonOpen;
        
        if (notepadOpen)
        {
            notepadHolder.SetActive(true);
            inputField.ActivateInputField();
            inputField.selectionAnchorPosition = inputField.text.Length;
            inputField.selectionFocusPosition = inputField.text.Length;
            inputField.caretPosition = inputField.text.Length;
        }
        else
        {
            inputField.DeactivateInputField();
        }

        Vector2 finalPosition = new Vector2(notepadHolderTransform.anchoredPosition.x, notepadHolderTransform.anchoredPosition.y + (notepadOpen ? notepadMoveDistance : -1 * notepadMoveDistance));
        CoroutineManager.Instance.Run(MoveNotepad(finalPosition, UpdateNotepadHolderActive));
    }

    private void UpdateNotepadHolderActive()
    {
        notepadHolder.SetActive(notepadOpen);
    }

    private IEnumerator MoveNotepad(Vector2 position, Action finished)
    {
        transitioning = true;
        Vector3 startingPosition = notepadHolderTransform.anchoredPosition;

        float i = 0;
        while (i < notepadMoveTime)
        {
            notepadHolderTransform.anchoredPosition = Vector2.Lerp(startingPosition, position, Mathf.SmoothStep(0, 1, i / notepadMoveTime));
            yield return null;
            i += Time.deltaTime;
        }

        notepadHolderTransform.anchoredPosition = position;
        transitioning = false;
        finished.Invoke();
    }

    public void ClearNotepad()
    {
        inputField.text = "";
    }
}