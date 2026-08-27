using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class GuardLog : MonoBehaviour
{
    // Singleton -------------------------------
    private static GuardLog _instance;

    public static GuardLog Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple GuardLog scripts. Bad!");
        }
        else
        {
            _instance = this;
        }
    }
    // Singleton -------------------------------
    
    [Header("References")]
    [SerializeField] private TextMeshProUGUI guardLogText;
    [SerializeField] private GameObject logInputFieldPrefab;

    private List<GameObject> currentLogInputFields = new List<GameObject>();
    private string currentLogString = "";

    private void Start()
    {
        ClearLog();
    }

    public void AddToLog(string message)
    {
        currentLogString += message + "\n";
        UpdateLogVisual();
    }

    public void ClearLog()
    {
        currentLogString = "";
        foreach (GameObject gameObject in currentLogInputFields)
        {
            Destroy(gameObject);
        }
        currentLogInputFields.Clear();
        UpdateLogVisual();
    }

    private void UpdateLogVisual()
    {
        string[] currentLogStringParts = currentLogString.Split("|");

        int inputFieldNum = 0;
        guardLogText.text = "";
        for (int i = 0; i < currentLogStringParts.Length; i++)
        {
            string part = currentLogStringParts[i];

            if (i % 2 == 0)
            {
                guardLogText.text += part;
            }
            else
            {
                guardLogText.text += ".................";
                inputFieldNum += 1;

                if (currentLogInputFields.Count >= inputFieldNum)
                {
                    continue;
                }

                Vector2 lastCharacterPosition = GetLastCharacterPosition(guardLogText);
                //Debug.Log("lastCharacterPosition: " + lastCharacterPosition);
                GameObject logInputField = Instantiate(logInputFieldPrefab, transform.position, Quaternion.identity, transform);
                RectTransform logInputRectTransform = logInputField.GetComponent<RectTransform>();
                logInputRectTransform.anchoredPosition = lastCharacterPosition;
                currentLogInputFields.Add(logInputField);
            }
        }        
    }

    private Vector2 GetLastCharacterPosition(TextMeshProUGUI textMeshPro)
    {
        textMeshPro.ForceMeshUpdate();

        TMP_TextInfo textInfo = textMeshPro.textInfo;
        int numCharacters = textInfo.characterCount;
        if (numCharacters <= 0)
        {
            return textMeshPro.rectTransform.anchoredPosition;
        }

        TMP_CharacterInfo characterInfo = textInfo.characterInfo[numCharacters - 1];
        return textMeshPro.rectTransform.anchoredPosition + (Vector2)characterInfo.bottomRight;
    }
}
