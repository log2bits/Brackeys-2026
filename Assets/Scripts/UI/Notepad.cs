using UnityEngine;
using TMPro;

public class Notepad : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    
    [SerializeField] private GameObject notepadPanel;

    [SerializeField] private GameObject notepadButton;

    public void Start()
    {
        notepadPanel.SetActive(false);
    }

    public void OpenNotepad()
    {
        notepadPanel.SetActive(true);
        notepadButton.SetActive(false); 
        
        inputField.ActivateInputField();
        inputField.selectionAnchorPosition = inputField.text.Length;
        inputField.selectionFocusPosition = inputField.text.Length;
        inputField.caretPosition = inputField.text.Length;
    }

    public void CloseNotepad()
    {
        notepadButton.SetActive(true);
        notepadPanel.SetActive(false);

        inputField.DeactivateInputField();
    }

    public void ClearNotepad()
    {
        inputField.text = "";
    }
}