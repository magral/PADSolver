using UnityEngine;
using System.Collections;

public class SolutionButton : MonoBehaviour {

	private int index = -1;

	public void SetIndex(int i)
	{
		index = i;
	}

	public void SetPath()
	{
		if (index == -1) { Debug.Log("Index not set for solution"); return; } 
		Solver.Instance.SetSolution(index);
		Solver.Instance.ShowPath();
	}
}
