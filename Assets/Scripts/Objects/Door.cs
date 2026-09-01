using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using LogicSolver;
using System.Text;
using TMPro;

public class Door : MonoBehaviour, IClickableObject
{
    [Header("References")]
    [SerializeField] private Transform doorSpriteTransform;
    [SerializeField] private Animator honestyButtonAnimator;
    [SerializeField] private SpriteRenderer doorSpriteOverlay;
    [SerializeField] private TextMeshProUGUI doorNumbersText;
    [SerializeField] private SpriteRenderer wrongDoorSpriteWall;

    [Header("Parameters")]
    [SerializeField] private float doorLightUpAmount = 0.015f;
    [SerializeField] private float doorRotateTime;
    [SerializeField] private float safeDoorRotateAngle;
    [SerializeField] private float unsafeDoorRotateAngle;
    [SerializeField] private float doorZoomZDistance;

    private DoorStatement doorStatement;
    private bool safe = false;
    private bool hasTalkedBefore = false;
    private bool open; 
    private int doorNumber;

    private bool honestyStatus;

    private void Start()
    {
        doorSpriteOverlay.color = new Color(1f, 1f, 1f, 0f);
        wrongDoorSpriteWall.gameObject.SetActive(!safe);

        honestyStatus = false;
        ToggleHonesty();
    }

    public void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Zoom into door and start dialogue
        if (GameManager.Instance.state == GameManager.GameState.OUTERROOM && !Dialogue.Instance.GetInDialogue())
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
            // Exit/advance dialogue if clicking on a door that isn't focused on or door is already open
            if (open || Mathf.Abs(transform.position.x - MainCameraMove.Instance.transform.position.x) > 0.1f)
            {
                Dialogue.Instance.DeferClickCheckToDialogue();
                return;
            }
            
            open = true;

            // You lose a life
            if (!safe)
            {
                int lives = GameManager.Instance.lives;
                lives -= 1;
                AudioManager.Instance.PlayOneShot(FmodEvents.Instance.lockedDoor, new Vector3(0,0,0));
                GameManager.Instance.lives = lives;

                if (lives < 1)
                {
                    GameManager.Instance.currentSeed = GameManager.StringToRandomInt(GameManager.GenerateRandomString());
                    MainCameraMove.Instance.LostLifeShake();
                    SceneTransition.Instance.FadeToBlack(MainMenuFinish);
                    return;
                }
                
                EventBus.Instance.DoLostLife();
                Dialogue.Instance.EndDialogue(true);

                CoroutineManager.Instance.Run(RotateDoor(unsafeDoorRotateAngle));
                
                return;
            }

            // Move to next room
            CoroutineManager.Instance.Run(RotateDoor(safeDoorRotateAngle));

            AudioManager.Instance.PlayOneShot(FmodEvents.Instance.openDoor, transform.position);
            MainCameraMove.Instance.MoveCamera(transform.position + new Vector3(0, 0, 0.5f), GameManager.GameState.OUTERROOM);

            GameManager.Instance.state = GameManager.GameState.TRANSITIONROOM;
            GameManager.Instance.ChangeGameState(GameManager.GameState.TRANSITIONROOM);
            GameManager.Instance.currentRoom += 1;

            Dialogue.Instance.EndDialogue(false);
            GuardLog.Instance.ClearLog();
            
            // Final room
            if (GameManager.Instance.currentDifficulty.roomCount == GameManager.Instance.currentRoom)
            {
                CoroutineManager.Instance.Run(WaitForFinalDialogue());
            }

            // Cutscenes or whatever will go here
            if (GameManager.Instance.currentRoom >= GameManager.Instance.worldState.roomStates.Count)
            {
                EndingCutsceneController.Instance.EndGame();
            }
        }
    }

    private void ZoomOut()
    {
        Vector3 finalPosition = new Vector3(transform.position.x, MainCameraMove.Instance.transform.position.y, GameManager.Instance.mainCameraZBeforeZoom);
        MainCameraMove.Instance.MoveCamera(finalPosition, GameManager.GameState.OUTERROOM);

        GameManager.Instance.state = GameManager.GameState.ZOOMING;
    }

    private IEnumerator WaitForFinalDialogue()
    {
		while (!Dialogue.Instance.StartDialogue("Wow, hey friend! I didn't really anticipate you getting this far... but that's great! There's just one more door between you and freedom. Just trust me!", true))
		{
			yield return null;
		}
	}

    private void MainMenuFinish()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void OnMouseUp()
    {
        doorSpriteOverlay.color = new Color(1f, 1f, 1f, 0f);
    }

    public void OnMouseOver()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            doorSpriteOverlay.color = new Color(1f, 1f, 1f, 0f);
            return;
        }

        doorSpriteOverlay.color = new Color(1f, 1f, 1f, doorLightUpAmount);
    }

    public void OnMouseExit()
    {
        doorSpriteOverlay.color = new Color(1f, 1f, 1f, 0f);
    }

    public void SetDialogue(DoorStatement doorStatement)
    {
        this.doorStatement = doorStatement;
        this.doorStatement.sentence = FormatDialogue(this.doorStatement.sentence);
    }

    public void SetIsSafe(bool isSafe = false)
    {
        this.safe = isSafe;

        wrongDoorSpriteWall.gameObject.SetActive(!isSafe);
    }

    public void SetNumber(int number)
    {
        this.doorNumber = number;

        string doorNumber = "<b>" + (number + 1).ToString();
        doorNumbersText.text = doorNumber;
    }

    public void ToggleHonesty()
    {
        honestyStatus = !honestyStatus;
        if (honestyStatus)
        {
            honestyButtonAnimator.Play("ChangeToHonest", 0);
        }
        else
        {
            honestyButtonAnimator.Play("ChangeToLying", 0);
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
