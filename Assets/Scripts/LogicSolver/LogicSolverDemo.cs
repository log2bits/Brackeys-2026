using System.Text;
using UnityEngine;
using LogicSolver;

// Test for the script
public class LogicSolverDemo : MonoBehaviour
{
	[Header("Room")]
	public int doorCount = 4;

	[Header("How many rooms to generate")]
	public int roomsToGenerate = 3;

	[Header("Turn on to try solving them yourself")]
	public bool hideAnswers = false;

	// Hardcoded stuff
	[Header("What the previous room looked like")]
	public string potColour = "blue";
	public string clockTime = "two o'clock";
	
	private void Start()
	{
		for (int room = 0; room < roomsToGenerate; room++)
		{
			GenerateRoom(room);
		}
	}

	private void GenerateRoom(int roomNumber)
	{
		RoomSettings settings = new RoomSettings
		{
			doorCount = doorCount,
			seed = Random.Range(0, int.MaxValue)
		};

		// No memory claims with only 2 doors
		if (doorCount <= 2) settings.minMemoryMentions = 0;

		// Things the player is meant to remember
		settings.knownFacts.Add(new KnownFact
		{
			possibleValues = new string[] { "red", "blue", "yellow", "white" },
			template = "the flower pot in the last room held a {0} flower",
			actualValue = potColour
		});
		settings.knownFacts.Add(new KnownFact
		{
			possibleValues = new string[] { "one o'clock", "two o'clock", "three o'clock" },
			template = "the clock in the last room read {0}",
			actualValue = clockTime
		});

		RoomSolution room = Solver.Solve(settings);
		if (room == null)
		{
			Debug.LogWarning("Room " + roomNumber + ": nothing could be built, try raising maxAttempts");
			return;
		}

		Debug.Log(Describe(roomNumber, room));
	}

	private string Describe(int roomNumber, RoomSolution room)
	{
		StringBuilder text = new StringBuilder();
		text.AppendLine("ROOM " + roomNumber + "  (" + doorCount + " doors)");
		text.AppendLine("You remember: pot was " + potColour + ", clock read " + clockTime);
		text.AppendLine();

		for (int guard = 0; guard < room.statements.Length; guard++)
		{
			text.AppendLine("Guard " + (guard + 1) + ": \"" + room.statements[guard] + "\"");
		}

		if (!hideAnswers)
		{
			text.AppendLine();
			text.AppendLine("ANSWER: door " + (room.safeDoor + 1)
				+ ", lying guards: " + Join(room.liars));
		}
		return text.ToString();
	}

	// Guards are shown to the player starting at 1
	private static string Join(int[] guards)
	{
		StringBuilder text = new StringBuilder();
		for (int i = 0; i < guards.Length; i++)
		{
			if (i > 0) text.Append(", ");
			text.Append(guards[i] + 1);
		}
		return text.ToString();
	}
}
