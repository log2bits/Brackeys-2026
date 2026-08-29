using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using LogicSolver;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class Door : ClickableObject
{
    [Header("References")]
    [SerializeField] private Transform doorSpriteTransform;
    [SerializeField] private TextMeshProUGUI doorNumbersText;

    [SerializeField] private Toggle honestToggle;
    [SerializeField] private Toggle lyingToggle;

    [Header("Parameters")]
    [SerializeField] private float doorRotateTime;
    [SerializeField] private float doorRotateAngle;
    [SerializeField] private float doorZoomZDistance;

    private DoorStatement doorStatement;
    private bool safe = false;
    private bool hasTalkedBefore = false;
    private bool open; 
    private int doorNumber;


    private enum HonestyStatus
    {
        Honest,
        Lying,
        Undecided
    }

    private HonestyStatus honestyStatus = HonestyStatus.Undecided;


    protected override void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (open)
        {
            return;
        }

        // Zoom into door and start dialogue
        if (GameManager.Instance.state == GameManager.GameState.OUTERROOM)
        {
            GameManager.Instance.mainCameraZBeforeZoom = MainCameraMove.Instance.transform.position.z;

            Vector3 finalPosition = new Vector3(transform.position.x, MainCameraMove.Instance.transform.position.y, transform.position.z + doorZoomZDistance);
            MainCameraMove.Instance.MoveCamera(finalPosition, GameManager.GameState.INNERROOM);

            GameManager.Instance.state = GameManager.GameState.ZOOMING;

            Dialogue.Instance.StartDialogue(ExtraFormatDialogue(doorStatement.sentence), !hasTalkedBefore, ZoomOut);
            if (!hasTalkedBefore)
            {
                GuardLog.Instance.AddToLog(doorStatement, doorNumber);
            }
            hasTalkedBefore = true;
        }

        else if (GameManager.Instance.state == GameManager.GameState.INNERROOM)
        {
            // Exit/advance dialogue if clicking on a door that isn't focused on
            if (Mathf.Abs(transform.position.x - MainCameraMove.Instance.transform.position.x) > 0.1f)
            {
                Dialogue.Instance.DeferClickCheckToDialogue();
                return;
            }

            // You lose a life
            if (!safe)
            {
                int lives = GameManager.Instance.lives;
                lives -= 1;
                GameManager.Instance.lives = lives;
                if (lives < 1)
                {
                    GameManager.Instance.currentSeed = GameManager.StringToRandomInt(GameManager.GenerateRandomString());
                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    EventBus.Instance.DoLostLife();
                }
                return;
            }

            // Move to next room
            AudioManager.Instance.PlayOneShot(FmodEvents.Instance.openDoor, transform.position);
            MainCameraMove.Instance.MoveCamera(transform.position + new Vector3(0, 0, 0.1f), GameManager.GameState.OUTERROOM);
            open = true;

            CoroutineManager.Instance.Run(RotateDoor(doorRotateAngle));

            GameManager.Instance.state = GameManager.GameState.TRANSITIONROOM;
            GameManager.Instance.currentRoom += 1;

            Dialogue.Instance.EndDialogue(false);
            GuardLog.Instance.ClearLog();

            
            // Cutscenes or whatever will go here
            if (GameManager.Instance.currentRoom >= GameManager.Instance.worldState.roomStates.Count)
            {
                CoroutineManager.Instance.Run(EndGame());
            }
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(5f);

        MainMenu.HighestBeatenDifficulty getHighestBeatenDifficulty = SaveSystem.GetHighestBeatenDifficulty();
        int highestDifficulty = -1;
        if (getHighestBeatenDifficulty != null)
        {
            highestDifficulty = getHighestBeatenDifficulty.highestBeatenDifficulty;
        }
        if (highestDifficulty >= GameManager.Instance.currentDifficulty.solverDifficulty)
        {
            yield break;
        }

        MainMenu.HighestBeatenDifficulty highestBeatenDifficulty = new MainMenu.HighestBeatenDifficulty(GameManager.Instance.currentDifficulty.solverDifficulty);
        SaveSystem.SaveHighestBeatenDifficulty(highestBeatenDifficulty);

        SceneManager.LoadSceneAsync("MainMenu");
    }

    private void ZoomOut()
    {
        Vector3 finalPosition = new Vector3(transform.position.x, MainCameraMove.Instance.transform.position.y, GameManager.Instance.mainCameraZBeforeZoom);
        MainCameraMove.Instance.MoveCamera(finalPosition, GameManager.GameState.OUTERROOM);

        GameManager.Instance.state = GameManager.GameState.ZOOMING;
    }

    protected override void OnMouseUp()
    {
        return;
    }

    public void SetDialogue(DoorStatement doorStatement)
    {
        this.doorStatement = doorStatement;
        this.doorStatement.sentence = FormatDialogue(this.doorStatement.sentence);
    }

    public void SetIsSafe(bool IsSafe = false)
    {
        this.safe = IsSafe;
    }

    public void SetNumber(int number)
    {
        this.doorNumber = number;

        string doorNumber = "<b>" + (number + 1).ToString();
        doorNumbersText.text = doorNumber;
    }

    public void ToggleHonest(bool honest)
    {
        if (honest)
        {
            honestyStatus = HonestyStatus.Honest;
            lyingToggle.isOn = false;
        }
        else if (honestyStatus == HonestyStatus.Honest)
        {
            honestyStatus = HonestyStatus.Undecided;
        }
    }

    public void ToggleLying(bool lying)
    {
        if (lying)
        {
            honestyStatus = HonestyStatus.Lying;
            honestToggle.isOn = false;
        }
        else if (honestyStatus == HonestyStatus.Lying)
        {
            honestyStatus = HonestyStatus.Undecided;
        }
    }

    private IEnumerator RotateDoor(float rotateAmount)
    {
        Vector3 startRotation = doorSpriteTransform.eulerAngles;
        Vector3 finalRotation = new Vector3(doorSpriteTransform.eulerAngles.x, doorSpriteTransform.eulerAngles.y + rotateAmount, doorSpriteTransform.eulerAngles.z);
        
        float i = 0;
        while (i < doorRotateTime)
        {
            doorSpriteTransform.eulerAngles = Vector3.Lerp(startRotation, finalRotation, Mathf.SmoothStep(0, 1, i / doorRotateTime));
            yield return null;
            i += Time.deltaTime;
        }

        doorSpriteTransform.eulerAngles = finalRotation;
    }

    private string ExtraFormatDialogue(string dialogue)
    {
        int barCount = 0;
        int barIndex = dialogue.IndexOf('|');
        while (barIndex != -1 && barCount < 1000)
        {
            if (barCount % 2 == 0)
            {
                dialogue = dialogue.Insert(barIndex + 1, "<b>");
            }
            else
            {
                dialogue = dialogue.Insert(barIndex + 1, "</b>");
            }
            dialogue = dialogue.Remove(barIndex, 1);

            barIndex = dialogue.IndexOf('|');
            barCount += 1;
        }

        return dialogue;
    }

    private string FormatDialogue(string dialogue)
    {
        return CapitalizeFirstLetter(dialogue) + ".";
    }

    private string CapitalizeFirstLetter(string dialogue)
    {
        StringBuilder stringBuilder = new StringBuilder(dialogue);
        string sampleString = "abcdefghijklmnopqrstuvwxyz";
        for (int i = 0; i < dialogue.Length; i++)
        {
            if (sampleString.Contains(dialogue[i]))
            {
                stringBuilder[i] = char.ToUpper(dialogue[i]);
                return stringBuilder.ToString();
            }
        }
        return dialogue;
    }
    
}
