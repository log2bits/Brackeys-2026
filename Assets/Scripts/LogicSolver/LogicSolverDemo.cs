using System.Text;
using UnityEngine;
using LogicSolver;

// Drop this on any GameObject and press Play. It prints a room at every size and band
public class LogicSolverDemo : MonoBehaviour
{
	[Header("Turn on to try solving them yourself")]
	public bool hideAnswers = false;

	private static readonly int[] DoorCounts = { 2, 3, 4, 5, 6 };

	private static readonly string[] BandNames =
		{ "EASY", "MEDIUM", "HARD", "EXTREME", "LOGICIAN" };

	private void Start()
	{
		StringBuilder text = new StringBuilder();
		for (int i = 0; i < DoorCounts.Length; i++)
		{
			if (i > 0) text.AppendLine();
			text.AppendLine("# " + DoorCounts[i] + " DOORS");
			for (int band = 0; band < BandNames.Length; band++)
			{
				text.AppendLine();
				text.AppendLine("**" + BandNames[band] + "**");
				text.Append(BuildRoom(DoorCounts[i], band));
			}
		}
		Debug.Log(text.ToString());
	}

	private string BuildRoom(int doorCount, int band)
	{
		RoomSettings settings = new RoomSettings
		{
			doorCount = doorCount,
			difficulty = band,
			seed = Random.Range(0, int.MaxValue)
		};
		settings.details.Add(new Detail("the flower pot in the last room was blue", true, "pot"));
		settings.details.Add(new Detail("the flower pot in the last room was red", false, "pot"));

		RoomSolution room = Solver.Solve(settings);
		if (room == null)
		{
			return "nothing could be built, try raising maxAttempts\n";
		}

		StringBuilder text = new StringBuilder();
		for (int guard = 0; guard < room.statements.Length; guard++)
		{
			text.AppendLine("Guard " + (guard + 1) + ": \"" + room.statements[guard] + "\"");
		}
		if (!hideAnswers)
		{
			text.AppendLine("ANSWER: ||door " + (room.safeDoor + 1)
				+ ", lying guards: " + Join(room.liars) + "||");
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