using System.Collections.Generic;
using LogicSolver;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private GameObject statementDropdownPrefab;

    private DoorStatement[] doorStatements;
    private List<TMP_Dropdown>[] statementDropdowns;

    private void Start()
    {
        doorStatements = new DoorStatement[GameManager.Instance.currentDifficulty.maxDoors];
        statementDropdowns = new List<TMP_Dropdown>[GameManager.Instance.currentDifficulty.maxDoors];
        ClearLog();
    }

    public void AddToLog(DoorStatement doorStatement, int doorID)
    {
        doorStatements[doorID] = doorStatement;
        UpdateLogVisual();
    }

    public void ClearLog()
    {
        for (int i = 0; i < doorStatements.Length; i++)
        {
            if (statementDropdowns[i] == null)
            {
                continue;
            }

            foreach (TMP_Dropdown dropdown in statementDropdowns[i])
            {
                Destroy(dropdown.gameObject);
            }
            statementDropdowns[i] = null;
        }
        System.Array.Clear(doorStatements, 0, doorStatements.Length);
        System.Array.Clear(statementDropdowns, 0, statementDropdowns.Length);
        UpdateLogVisual();
    }

    private void UpdateLogVisual()
    {
        string finalGuardLogText = "";
        // Loop each door's dialogue, in number ascending order
        for (int doorID = 0; doorID < doorStatements.Length; doorID++)
        {
            DoorStatement statement = doorStatements[doorID];
            if (statement == null)
            {
                continue;
            }

            finalGuardLogText += "Door " + (doorID + 1) + ": ";
            string[] doorStatementStringParts = statement.sentence.Split("|");

            // Loop part of the door's dialogue, split by the bars
            for (int partIndex = 0; partIndex < doorStatementStringParts.Length; partIndex++)
            {
                string part = doorStatementStringParts[partIndex];

                if (partIndex % 2 == 0)
                {
                    finalGuardLogText += part;
                }
                else
                {
                    // Find longest string in possibilities
                    int longestLength = int.MinValue;
                    string longestString = "";
                    foreach (string dropdownStatement in doorStatements[doorID].dropdownContents[(partIndex % 2) - 1])
                    {
                        if (dropdownStatement.Length > longestLength)
                        {
                            longestLength = dropdownStatement.Length;
                            longestString = dropdownStatement;
                        }
                    }
                    //Vector2 longestStringSize = guardLogText.GetPreferredValues(longestString);

                    Vector2 beforeLastCharacterPosition = GetLastCharacterPosition(guardLogText);
                    finalGuardLogText += longestString.Replace(" ", "_");
                    guardLogText.text = finalGuardLogText;
                    guardLogText.ForceMeshUpdate();
                    Vector2 afterLastCharacterPosition = GetLastCharacterPosition(guardLogText);

                    float addedStringWidth;
                    addedStringWidth = afterLastCharacterPosition.x - beforeLastCharacterPosition.x;
                    if (addedStringWidth < 1)
                    {
                        addedStringWidth = afterLastCharacterPosition.x - guardLogText.rectTransform.anchoredPosition.x;
                    }

                    TMP_Dropdown statementDropdown = GetOrMakeDropdown(doorID, (partIndex % 2) - 1);

                    RectTransform statementDropdownParentRectTransform = statementDropdown.transform.parent.GetComponent<RectTransform>();
                    statementDropdownParentRectTransform.anchoredPosition = afterLastCharacterPosition;

                    RectTransform statementDropdownRectTransform = statementDropdown.GetComponent<RectTransform>();
                    statementDropdownRectTransform.sizeDelta = new Vector2(addedStringWidth, statementDropdownRectTransform.sizeDelta.y);
                }
            }
            finalGuardLogText += "\n";
        }
        
        guardLogText.text = finalGuardLogText;      
    }

    private TMP_Dropdown GetOrMakeDropdown(int doorID, int dropdownIndex)
    {
        TMP_Dropdown statementDropdown = null;
        if (statementDropdowns[doorID]?.Count > dropdownIndex)
        {
            return statementDropdowns[doorID][dropdownIndex];
        }

        GameObject statementDropdownGameObject = Instantiate(statementDropdownPrefab, transform.position, Quaternion.identity, transform);
        statementDropdown = statementDropdownGameObject.GetComponentInChildren<TMP_Dropdown>();
        if (statementDropdown == null)
        {
            throw new System.Exception("GuardLog: statementDropdownPrefab missing dropdown component!");
        }

        if (statementDropdowns[doorID] == null)
        {
            statementDropdowns[doorID] = new List<TMP_Dropdown>();
        }
        statementDropdowns[doorID].Add(statementDropdown);
        
        statementDropdown.AddOptions(doorStatements[doorID].dropdownContents[dropdownIndex]);
        
        return statementDropdown;
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
