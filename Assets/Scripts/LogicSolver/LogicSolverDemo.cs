using System.Text;
using UnityEngine;
using LogicSolver;

// Drop this on any GameObject and press Play. It prints rooms to the Console
public class LogicSolverDemo : MonoBehaviour
{
	[Header("Room")]
	public int doorCount = 4;

	[Header("How many rooms to generate")]
	public int roomsToGenerate = 3;

	[Header("Turn on to try solving them yourself")]
	public bool hideAnswers = false;

	[Header("Print how the room turned out")]
	public bool showStats = true;

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

		settings.details.Add(new Detail("the flower pot in the last room was blue", true, "pot"));

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
		text.AppendLine("You remember: pot was blue");
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

		if (showStats && room.stats != null)
		{
			text.AppendLine();
			text.AppendLine(room.stats.ToString());
		}
		return text.ToString();
	}

	// Guards are shown to the player starting at 1, not 0
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