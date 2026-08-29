using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
	private static string SaveFolderPath()
	{
		return "SaveData" + Path.DirectorySeparatorChar;
	}

	// Options --------------------------------------------------------

	private static string OptionsPath()
	{
		return SaveFolderPath() + "Options.json";
	}

	public static void SaveOptions(OptionsMenu.OptionsData optionsData)
	{
		string json = JsonUtility.ToJson(optionsData);

		string path = Path.Combine(Application.persistentDataPath, OptionsPath());
		Directory.CreateDirectory(Path.GetDirectoryName(path));

		File.WriteAllText(path, json);
	}

	public static OptionsMenu.OptionsData GetOptions()
	{
		string path = Path.Combine(Application.persistentDataPath, OptionsPath());
		if (!File.Exists(path))
		{
			return null;
		}

		string json = File.ReadAllText(path);

		OptionsMenu.OptionsData optionsData = JsonUtility.FromJson<OptionsMenu.OptionsData>(json);
		return optionsData;
	}

	// Highest Difficulty --------------------------------------------------------

	private static string HighestBeatenDifficultyPath()
	{
		return SaveFolderPath() + "HighestBeatenDifficulty.json";
	}

	public static void SaveHighestBeatenDifficulty(MainMenu.HighestBeatenDifficulty highestBeatenDifficulty)
	{
		string json = JsonUtility.ToJson(highestBeatenDifficulty);

		string path = Path.Combine(Application.persistentDataPath, HighestBeatenDifficultyPath());
		Directory.CreateDirectory(Path.GetDirectoryName(path));

		File.WriteAllText(path, json);
	}

	public static MainMenu.HighestBeatenDifficulty GetHighestBeatenDifficulty()
	{
		string path = Path.Combine(Application.persistentDataPath, HighestBeatenDifficultyPath());
		if (!File.Exists(path))
		{
			return null;
		}

		string json = File.ReadAllText(path);

		MainMenu.HighestBeatenDifficulty highestBeatenDifficulty = JsonUtility.FromJson<MainMenu.HighestBeatenDifficulty>(json);
		return highestBeatenDifficulty;
	}
}
