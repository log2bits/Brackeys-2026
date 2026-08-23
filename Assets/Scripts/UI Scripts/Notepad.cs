using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Notepad : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject notepadPanel;  

    private bool isOpen = false;

    void Update()
    {
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            ToggleNotepad();
            inputField.DeactivateInputField();
        }
    }

    void ToggleNotepad()
    {
        isOpen = !isOpen;
        notepadPanel.SetActive(isOpen);

        if (isOpen)
        {
            inputField.ActivateInputField();
        }
    }

    public void ClearNotepad()
    {
        inputField.text = "";
    }
}