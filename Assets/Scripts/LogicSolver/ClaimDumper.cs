using UnityEngine;
using LogicSolver;

public class ClaimDumper : MonoBehaviour
{
	public int doorCount = 4;
	public string folder = "ClaimDump";

	[ContextMenu("Dump Claims")]
	private void Dump()
	{
		ClaimDump.WriteAll(folder, doorCount);
		Debug.Log("wrote claim files to " + folder);
	}
}