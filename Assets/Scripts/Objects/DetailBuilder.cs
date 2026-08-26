using System.Collections.Generic;
using UnityEngine;
using LogicSolver;

namespace ProceduralHelperGen
{
	public static class DetailBuilder
	{
		// turn on to see what each room was handed, and what was true
		public static bool logDetails = true;

		// Fills the room's details from everything the player walked past, then solves
		public static RoomSolution SolveRoom(int room, List<ObjectDataTemplate> catalogue, int detailMentions)
		{
			WorldState world = GameManager.Instance.worldState;
			RoomSettings settings = world.roomStates[room].roomSettings;
			settings.details.Clear();
			settings.details.AddRange(ForRoomsBefore(world, room, catalogue));
			settings.detailMentions = detailMentions;
			if (logDetails) Log(room, settings);
			return Solver.Solve(settings);
		}

		public static List<Detail> ForRoomsBefore(WorldState world, int room, List<ObjectDataTemplate> catalogue)
		{
			List<Detail> details = new List<Detail>();
			for (int past = 0; past < room && past < world.roomStates.Count; past++)
			{
				RoomState state = world.roomStates[past];
				string where = past == room - 1 ? "the last room" : "room " + (past + 1);
				foreach (ObjectDataTemplate kind in catalogue)
				{
					MemorableObjectTemplate found = FindIn(state, kind);
					string about = kind.name + " in room " + past;
					string absent = kind.GetRoomTemplate();
					if (!string.IsNullOrEmpty(absent)) details.Add(new Detail(Fill(absent, where, null), found == null, about));
					foreach (ObjectPropertyData property in kind.GetObjectPropertyDatas())
					{
						string actual = found == null ? null : found.GetActualValue(property.propertyName);
						foreach (string value in property.values) details.Add(new Detail(Fill(property.template, where, value), actual == value, about));
					}
				}
			}

			return details;
		}

		private static MemorableObjectTemplate FindIn(RoomState state, ObjectDataTemplate kind)
		{
			foreach (GameObject placed in state.objects)
			{
				if (placed == null) continue;
				MemorableObjectTemplate memObject = placed.GetComponent<MemorableObjectTemplate>();
				if (memObject != null && memObject.WasBuiltFrom(kind.GetObjectPropertyDatas())) return memObject;
			}
			return null;
		}

		private static void Log(int room, RoomSettings settings)
		{
			int trueOnes = 0;
			System.Text.StringBuilder text = new System.Text.StringBuilder();
			text.AppendLine("room " + room + " was handed " + settings.details.Count + " details");
			foreach (Detail detail in settings.details)
			{
				if (detail.isTrue) trueOnes++;
				text.AppendLine("   " + detail);
			}
			text.AppendLine(trueOnes + " true, " + (settings.details.Count - trueOnes) + " false");
			Debug.Log(text.ToString());
		}

		// {0} is the value, {1} is the room
		private static string Fill(string phrase, string where, string value)
		{
			string text = phrase.Replace("{1}", where);
			return value == null ? text : text.Replace("{0}", value);
		}
	}
}