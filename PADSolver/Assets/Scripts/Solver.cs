﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum Direction
{
	Right,
	Down,
	Left,
	Up
}

public class Solver : MonoBehaviour {
	
	/* Coordinate class - holds row and column information */
	public class Coords
	{
		public int row, col;

		// Constructor
		public Coords(int x, int y)
		{
			row = x;
			col = y;
		}

		// Copy Constructor
		public Coords(Coords coord)
		{
			row = coord.row;
			col = coord.col;
		}

		// Equals operator
		public bool Equals(Coords other)
		{
			return (row == other.row && col == other.col);
		}

		// String format
		public override string ToString()
		{
			 return "(" + col + ", " + row + ")";
		}
	}

	/* Match Data class - holds relevant data about a match */
	public class MatchData
	{
		public OrbType orbType;
		public int numOrbs;

		// Constructor
		public MatchData(OrbType orb, int count)
		{
			orbType = orb;
			numOrbs = count;
		}

		// Copy Constructor
		public MatchData(MatchData match)
		{
			orbType = match.orbType;
			numOrbs = match.numOrbs;
		}

		// Equals operator
		public bool Equals(MatchData other)
		{
			return (orbType == other.orbType && numOrbs == other.numOrbs);
		}

		// String format
		public override string ToString()
		{
			return "(" + orbType.ToString() + ", " + numOrbs + ")";
		}
	}

	/* Solution Data class - holds relevant data for each solution */
	public class SolutionData : System.IComparable<SolutionData>
	{
		public OrbType[,] solutionBoard;
		public Coords currentCursorPos;
		public Coords startOrbPos;
		public float totalWeight;
		public List<Direction> path;
		public List<MatchData> matches;
		public bool isChecked;

		// Constructor
		public SolutionData(OrbType[,] board)
		{
			solutionBoard = (OrbType[,])board.Clone();
			currentCursorPos = new Coords(0,0);
			startOrbPos = new Coords(0,0);
			totalWeight = 0;
			path = new List<Direction>();
			matches = new List<MatchData>();
			isChecked = false;
		}

		// Copy Constructor
		public SolutionData(SolutionData solution)
		{
			solutionBoard = (OrbType[,])solution.solutionBoard.Clone();
			currentCursorPos = new Coords(solution.currentCursorPos);
			startOrbPos = new Coords(solution.startOrbPos);
			totalWeight = solution.totalWeight;
			path = new List<Direction>(solution.path);
			matches = new List<MatchData>(solution.matches);
			isChecked = solution.isChecked;
		}

		// Comparsion function
		public int CompareTo(SolutionData other)
		{
			return (int)((other.totalWeight - totalWeight)*1000);
		}

		// Equals operator
		public bool Equals(SolutionData other)
		{
			if (!startOrbPos.Equals(other.startOrbPos)) { return false; }
			if (matches.Count != other.matches.Count) { return false; }
			for (int i = 0; i < matches.Count; i++)
			{
				if (!matches[i].Equals(other.matches[i])) { return false; }
			}
			return true;
		}

		// String format
		public override string ToString()
		{
			return "(W = " + totalWeight + ", M = " + matches.Count + ", S = " + startOrbPos.ToString() + ", P = " + path.Count + ")";
		}

		// String format (verbose version)
		public string ToStringV()
		{
			string output = "(Weight = " + totalWeight + ", Matches(" + matches.Count + ") = [ ";
			foreach (MatchData match in matches)
			{
				output += match.ToString() + " ";
			}
			output += "], Start Position = " + startOrbPos.ToString() + ", Path(" + path.Count + ") = [ ";
			foreach (Direction dir in path)
			{
				output += dir.ToString() + " ";
			}
			output += "])";
			return (output);
		}

		// String format (button version)
		public string ToStringB()
		{
			string output = "W = " + totalWeight.ToString("F2") + ", C = " + matches.Count + ", S = " + startOrbPos.ToString() + ", P = " + path.Count + "\n[ ";
			foreach (MatchData match in matches)
			{
				output += match.ToString() + " ";
			}
			output += "]";
			return output;
		}
	};

	//---------------------------------/
	/* Constants and public variables */
	//---------------------------------/
	public const int ORB_SIZE = 80;
	public const float ORB_MULTIPLIER = 0.25f;
	public const float COMBO_MULTIPLIER = 0.25f;
	public const int MAX_NUM_SOLUTIONS = 30;

	[Header("Path Variables")]
	[SerializeField]
	private Transform _pathRoot;
	[SerializeField]
	private GameObject _arrowRight;
	[SerializeField]
	private GameObject _arrowDown;
	[SerializeField]
	private GameObject _arrowUp;
	[SerializeField]
	private GameObject _arrowLeft;
	[SerializeField]
	private GameObject _start;
	[SerializeField]
	private GameObject _end;
	[Header("Board Variables")]
	[SerializeField]
	private Transform boardRoot;
	[Header("Solutions Variables")]
	[SerializeField]
	private Transform _solutionsRoot;
	[SerializeField]
	private GameObject _solutionChoice;

	public static Solver Instance;

	//--------------------/
	/* Private Variables */
	//--------------------/
	private int _rows = 5;
	private int _cols = 6;
	private int _maxLength = 24;
	private OrbType[,] _board;
	private OrbType[,] _initialBoard;
	private float[] _orbWeights;
	private List<SolutionData> _solutions;
	private SolutionData _selectedSolution;

	//-----------------/
	/* Initialization */
	//-----------------/
	void Awake()
	{
		_board = new OrbType[_rows, _cols];
		_initialBoard = new OrbType[_rows, _cols];
		_orbWeights = new float[6] { 1, 1, 1, 1, 1, 0.3f };
		_solutions = new List<SolutionData>();
		_selectedSolution = null;
		Instance = this;
	}

	//----------------------/
	/* Debugging Functions */
	//----------------------/

	void PrintBoard(OrbType[,] board, string name)
	{
		string output = name + " = [ ";
		for (int r = 0; r < _rows; r++)
		{
			output += "[ ";
			for (int c = 0; c < _cols; c++)
			{
				output += board[r, c] + " ";
			}
			output += "]\n";
		}
		output += "]";
		Debug.Log(output);
	}

	void PrintSolutions(List<SolutionData> solutions, string name)
	{
		string solutionsOutput = name + "(" + solutions.Count + ") = [ ";
		foreach (SolutionData solution in solutions)
		{
			solutionsOutput += solution.ToString() + "\n";
		}
		solutionsOutput += "]";
		Debug.Log(solutionsOutput);
	}

	//--------------------/
	/* Private Functions */
	//--------------------/
	/// <summary>
	/// Updates internal representation of puzzle board
	/// </summary>
	void GetBoard()
	{
		for (int i = 0, r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++, i++)
				_board[r, c] = boardRoot.GetChild(i).GetComponent<ScrollImage>().State;
	}

	/// <summary>
	/// Checks if all orbs on the board are assigned
	/// </summary>
	bool CheckFilledBoard()
	{
		for (int r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++)
				if (_board[r, c] == OrbType.None)
				{
					Debug.Log("Cannot have empty orbs when solving. (Empty Orb located at [" + r + ", " + c + "])");
					return false;
				}
		return true;
	}

	/// <summary>
	/// Creates an empty board
	/// </summary>
	OrbType[,] CreateEmptyBoard()
	{
		OrbType[,] rBoard = new OrbType[_rows, _cols];
		for (int r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++)
				rBoard[r, c] = OrbType.Undefined;
		return rBoard;
	}

	/// <summary>
	/// Finds and collects all matches on the given board and saves them into a solution data object
	/// </summary>
	SolutionData FindMatches(OrbType[,] board)
	{
		//Create board for only the matches
		OrbType[,] matchesBoard = CreateEmptyBoard();

		//Check For Horizontal Matches
		for (int i = 0; i < _rows; i++)
		{
			OrbType previousOrb1 = OrbType.None;
			OrbType previousOrb2 = OrbType.None;
			for (int j = 0; j < _cols; j++)
			{
				OrbType currentOrb = board[i, j];
				//If there is a horizontal match of 3 orbs, add the orbs to the match board
				if (previousOrb1 == previousOrb2 && previousOrb2 == currentOrb && currentOrb != OrbType.None)
				{
					matchesBoard[i, j] = currentOrb;
					matchesBoard[i, j - 1] = currentOrb;
					matchesBoard[i, j - 2] = currentOrb;
				}
				//Advance orbs
				previousOrb1 = previousOrb2;
				previousOrb2 = currentOrb;
			}
		}

		//Check for Vertical Matches
		for (int j = 0; j < _cols; j++)
		{
			OrbType previousOrb1 = OrbType.None;
			OrbType previousOrb2 = OrbType.None;
			for (int i = 0; i < _rows; i++)
			{
				OrbType currentOrb = board[i, j];
				//If there is a vertical match of 3 orbs, add the orbs to the match board
				if (previousOrb1 == previousOrb2 && previousOrb2 == currentOrb && currentOrb != OrbType.None)
				{
					matchesBoard[i, j] = currentOrb;
					matchesBoard[i - 1, j] = currentOrb;
					matchesBoard[i - 2, j] = currentOrb;
				}
				//Advance orbs
				previousOrb1 = previousOrb2;
				previousOrb2 = currentOrb;
			}
		}

		//Use a flood fill algorithm to create a "path" for the orbs to follow
		OrbType[,] boardCopy = (OrbType[,])matchesBoard.Clone();
		List<MatchData> matches = new List<MatchData>();
		for (int i = 0; i < _rows; i++)
		{
			for (int j = 0; j < _cols; j++)
			{
				OrbType currentOrb = boardCopy[i, j];
				if (currentOrb == OrbType.Undefined) { continue; }
				List<Coords> stack = new List<Coords>();
				stack.Add(new Coords(i, j));
				int numOrbs = 0;
				while (stack.Count > 0)
				{
					Coords lastRC = stack[stack.Count - 1];
					stack.RemoveAt(stack.Count - 1);
					if (boardCopy[lastRC.row, lastRC.col] != currentOrb) { continue; }
					numOrbs++;
					boardCopy[lastRC.row, lastRC.col] = OrbType.Undefined;
					if (lastRC.row > 0) { stack.Add(new Coords(lastRC.row - 1, lastRC.col)); }
					if (lastRC.row < _rows - 1) { stack.Add(new Coords(lastRC.row + 1, lastRC.col)); }
					if (lastRC.col > 0) { stack.Add(new Coords(lastRC.row, lastRC.col - 1)); }
					if (lastRC.col < _cols - 1) { stack.Add(new Coords(lastRC.row, lastRC.col + 1)); }
				}
				matches.Add(new MatchData(currentOrb, numOrbs));
			}
		}
		SolutionData returnData = new SolutionData(matchesBoard);
		returnData.matches = matches;
		return returnData;
	}

	/// <summary>
	/// Drops all remaining orbs down after removed matches
	/// </summary>
	OrbType[,] DropRemainingOrbs(OrbType[,] currentBoard)
	{
		for (int c = 0; c < _cols; c++)
		{
			int newRow = _rows - 1;
			for (int r = _rows - 1; r >= 0; r--)
			{
				if (currentBoard[r,c] != OrbType.None)
				{
					currentBoard[newRow, c] = currentBoard[r, c];
					newRow--;
				}
			}
			while (newRow >= 0)
			{
				currentBoard[newRow, c] = OrbType.None;
				newRow--;
			}
		}
		return currentBoard;
	}

	/// <summary>
	/// Removes found matches on the given board
	/// </summary>
	OrbType[,] RemoveMatches(OrbType[,] currentBoard, OrbType[,] matchesBoard)
	{
		for (int r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++)
				if (matchesBoard[r, c] != OrbType.Undefined)
					currentBoard[r, c] = OrbType.None;
		currentBoard = DropRemainingOrbs(currentBoard);
		return currentBoard;
	}

	/// <summary>
	/// Computes the total weight of the givin matches in a solution
	/// </summary>
	float ComputeTotalWeight(List<MatchData> matches)
	{
		float totalWeight = 0;
		foreach (MatchData match in matches)
		{
			float weight = _orbWeights[(int)match.orbType - 1];
			float orbBonus = (match.numOrbs - 3) * ORB_MULTIPLIER + 1f;
			totalWeight += weight * orbBonus;
		}
		float comboBonus = (matches.Count - 1) * COMBO_MULTIPLIER + 1f;
		return totalWeight * comboBonus;
	}

	/// <summary>
	/// Evaluates the solution of the board by getting the matches and its total weight
	/// </summary>
	SolutionData EvaluateSolution(SolutionData solution)
	{
		OrbType[,] currBoard = (OrbType[,])solution.solutionBoard.Clone();
		List<MatchData> allMatches = new List<MatchData>();
		do
		{
			SolutionData matchesBoardData = FindMatches(currBoard);
			List<MatchData> foundMatches = new List<MatchData>(matchesBoardData.matches);
			if (foundMatches.Count <= 0) { break; }
			currBoard = RemoveMatches(currBoard, matchesBoardData.solutionBoard);
			allMatches.AddRange(foundMatches);
		} while (true);
		solution.totalWeight = ComputeTotalWeight(allMatches);
		solution.matches = new List<MatchData>(allMatches);
		return solution;
	}
	
	/// <summary>
	/// Make a copy of the given board but move the current cursor to the given coordinates (r,c)
	/// </summary>
	SolutionData MakeSolutionWithCursor(SolutionData solution, int r, int c)
	{
		SolutionData rSolution = new SolutionData(solution.solutionBoard);
		rSolution.currentCursorPos = new Coords(r, c);
		rSolution.startOrbPos = new Coords(r, c);
		return rSolution;
	}

	/// <summary>
	/// Checks if the current orb can be move in given direction 
	/// </summary>
	bool CanMoveOrb(SolutionData solution, Direction dir)
	{
		if (solution.path.Count > 0 && (int)solution.path[solution.path.Count-1] == ((int)dir + 2) % 4) { return false; }
		else
		{

			switch(dir)
			{
				case Direction.Right: return solution.currentCursorPos.col < _cols - 1;	
				case Direction.Down: return solution.currentCursorPos.row < _rows - 1;	
				case Direction.Left: return solution.currentCursorPos.col > 0;			
				case Direction.Up: return solution.currentCursorPos.row > 0;          
				default: return false;
			}
		}
	}

	/// <summary>
	/// Swaps the two orbs after moving in given direction
	/// </summary>
	SolutionData SwapOrbs(SolutionData solution, Direction dir)
	{
		Coords oldCursorPos = new Coords(solution.currentCursorPos);
		switch(dir)
		{
			case Direction.Right: solution.currentCursorPos.col++; break;
			case Direction.Down: solution.currentCursorPos.row++; break;
			case Direction.Left: solution.currentCursorPos.col--; break;
			case Direction.Up: solution.currentCursorPos.row--; break;
		}
		OrbType orbTemp = solution.solutionBoard[oldCursorPos.row, oldCursorPos.col];
		solution.solutionBoard[oldCursorPos.row, oldCursorPos.col] = solution.solutionBoard[solution.currentCursorPos.row, solution.currentCursorPos.col];
		solution.solutionBoard[solution.currentCursorPos.row, solution.currentCursorPos.col] = orbTemp;
		solution.path.Add(dir);
		return solution;
	}

	/// <summary>
	/// Steps trough each solution and does a check for a better solution
	/// </summary>
	List<SolutionData> StepSolutions()
	{
		List<SolutionData> newSolutions = new List<SolutionData>();
		foreach (SolutionData solution in _solutions)
		{
			if (solution.isChecked) { continue; }
			for (int dir = 0; dir < 4; dir++)
			{
				if(!CanMoveOrb(solution,(Direction)dir)) { continue; }
				SolutionData newSolution = new SolutionData(solution);
				newSolution = EvaluateSolution(SwapOrbs(newSolution, (Direction)dir));
				newSolutions.Add(new SolutionData(newSolution));
			}
			solution.isChecked = true;
		}
		_solutions.AddRange(newSolutions);
		_solutions.Sort();
		return _solutions.GetRange(0, (_solutions.Count < MAX_NUM_SOLUTIONS*_maxLength) ? _solutions.Count : MAX_NUM_SOLUTIONS*_maxLength); 
	}

	/// <summary>
	/// Shows the board on the front-end side
	/// </summary>
	void ShowBoard(OrbType[,] board)
	{
		for (int i = 0, r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++, i++)
				boardRoot.GetChild(i).GetComponent<ScrollImage>().SetOrb(board[r,c]);
	}

	/// <summary>
	/// Remove duplicate solutions
	/// </summary>1
	List<SolutionData> SimplifySolutions(List<SolutionData> solutions)
	{
		List<SolutionData> simplifiedSolutions = new List<SolutionData>();
		foreach (SolutionData solution in solutions)
		{
			bool found = false;
			foreach (SolutionData simplifiedSolution in simplifiedSolutions)
			{
				if (solution.Equals(simplifiedSolution))
				{
					found = true;
					break;
				}
			}
			if (found) { continue; }
			else { simplifiedSolutions.Add(solution); }
		}
		return simplifiedSolutions;
	}

	/// <summary>
	/// Clears the Solutions List of all choices
	/// </summary>
	void ClearSolutionsList()
	{
		if (_solutionsRoot.childCount > 0)
		{
			for (int i = 0; i < _solutionsRoot.childCount; i++)
			{
				Destroy(_solutionsRoot.GetChild(i).gameObject);
			}
		}
	}

	//-------------------/ 
	/* Public Functions */
	//-------------------/
	/// <summary>
	/// Main function that runs search algorithm for solutions
	/// </summary>
	public void SolveBoard()
	{
		GetBoard();
		if (!CheckFilledBoard()) { return; }
		_initialBoard = (OrbType[,])_board.Clone();
		_solutions.Clear();
		SolutionData baseSolution = new SolutionData(_board);
		baseSolution = EvaluateSolution(baseSolution);
		for (int r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++)
				_solutions.Add(MakeSolutionWithCursor(baseSolution, r, c));
		int step = 0;
		while (step < _maxLength)
		{
			step++;
			_solutions = StepSolutions();
			Debug.Log(step + ", " + _solutions.Count);
		}
		_solutions = SimplifySolutions(_solutions);
		PrintSolutions(_solutions, "solutionsFinal"); //Debug
		ClearSolutionsList();
		for (int i = 0; i < MAX_NUM_SOLUTIONS; i++) 
		{
			GameObject solutionChoice = Instantiate(_solutionChoice) as GameObject;
			solutionChoice.transform.SetParent(_solutionsRoot);
			solutionChoice.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -40 - i * 60, 0);
			solutionChoice.GetComponentInChildren<Text>().text = (i+1) + ". " + _solutions[i].ToStringB();
			solutionChoice.GetComponent<SolutionButton>().SetIndex(i);
		}
	}

	/// <summary>
	/// Randomizes the orbs on the board
	/// </summary>
	public void RandomizeBoard()
	{
		TogglePath("false");
		ClearSolutionsList();
		do
		{
			for (int i = 0, r = 0; r < _rows; r++)
				for (int c = 0; c < _cols; c++, i++)
				{
					OrbType randomOrb = (OrbType)Random.Range(1, 7);
					boardRoot.GetChild(i).GetComponent<ScrollImage>().SetOrb(randomOrb);
					_board[r, c] = randomOrb;
					_initialBoard[r, c] = randomOrb;
				}
		} while (FindMatches(_board).matches.Count != 0);
	}

	/// <summary>
	/// Resets the orbs on the board to be unknown
	/// </summary>
	public void ClearBoard()
	{
		TogglePath("false");
		ClearSolutionsList();
		_selectedSolution = null;
		for (int i = 0, r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++, i++)
			{
				boardRoot.GetChild(i).GetComponent<ScrollImage>().SetOrb(OrbType.None);
				_board[r, c] = OrbType.None;
			}
	}

	/// <summary>
	/// Resets the board to its inital state
	/// </summary>
	public void ResetBoard()
	{
		TogglePath("false");
		for (int i = 0, r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++, i++)
			{
				OrbType oldOrb = _initialBoard[r, c];
				boardRoot.GetChild(i).GetComponent<ScrollImage>().SetOrb(oldOrb);
				_board[r, c] = oldOrb;
			}
	}

	/// <summary>
	/// Shows the solution board
	/// </summary>
	public void ShowFinalBoard()
	{
		if (_selectedSolution == null)
		{
			Debug.Log("No solution given.");
			return;
		}
		TogglePath("false");
		ShowBoard((OrbType[,])_selectedSolution.solutionBoard.Clone());
	}

	/// <summary>
	/// Remove the matcheso n the current board and drop the remaining orbs
	/// </summary>
	public void DropMatches()
	{
		TogglePath("false");
		GetBoard();
		OrbType[,] currBoard = (OrbType[,])_board.Clone();
		do
		{
			SolutionData matchesBoardData = FindMatches(currBoard);
			List<MatchData> foundMatches = matchesBoardData.matches;
			if (foundMatches.Count <= 0) { break; }
			currBoard = RemoveMatches(currBoard, matchesBoardData.solutionBoard);
		} while (true);
		for (int r = 0; r < _rows; r++)
			for (int c =0; c < _cols; c++)
				_board = (OrbType[,])currBoard.Clone();
		ShowBoard(_board);
	}

	/// <summary>
	/// Sets the selection solution to the solution at index i
	/// </summary>
	public void SetSolution(int i)
	{
		_selectedSolution = _solutions[i];
	}

	/// <summary>
	/// Show path on board
	/// </summary>
	public void ShowPath()
	{
		if(_selectedSolution != null)
		{
			//* DEBUG
			Debug.Log(_selectedSolution.ToStringV());
			// END DEBUG */
			for (int i = 0; i < _pathRoot.childCount; i++)
			{
				Destroy(_pathRoot.GetChild(i).gameObject);
			}
			Vector3 spawnPosition = new Vector3( (45.5f + _selectedSolution.startOrbPos.col * 75 + _selectedSolution.startOrbPos.col * 5), (-42.5f - _selectedSolution.startOrbPos.row * 75 - _selectedSolution.startOrbPos.row * 5), -1);
			Vector3 startPosition = spawnPosition;
			for (int i = 0; i < _selectedSolution.path.Count; i++)
			{
				GameObject arrow = null;
				switch (_selectedSolution.path[i])
				{
					case Direction.Left:
						arrow = Instantiate(_arrowLeft) as GameObject;
						arrow.transform.SetParent(_pathRoot);
						arrow.GetComponent<RectTransform>().anchoredPosition = spawnPosition;
						spawnPosition.x -= 80;
						break;
					case Direction.Right:
						arrow = Instantiate(_arrowRight) as GameObject;
						arrow.transform.SetParent(_pathRoot);
						arrow.GetComponent<RectTransform>().anchoredPosition = spawnPosition;
						arrow.transform.localRotation = Quaternion.identity;
						spawnPosition.x += 80;
						break;
					case Direction.Down:
						arrow = Instantiate(_arrowDown) as GameObject;
						arrow.transform.SetParent(_pathRoot);
						arrow.GetComponent<RectTransform>().anchoredPosition = spawnPosition;
						spawnPosition.y -= 80;
						break;
					case Direction.Up:
						arrow = Instantiate(_arrowUp) as GameObject;
						arrow.transform.SetParent(_pathRoot);
						arrow.GetComponent<RectTransform>().anchoredPosition = spawnPosition;
						spawnPosition.y += 80;
						break;
				}
				if (i % 3 == 0) { arrow.GetComponent<Image>().color = Color.black; }
				else if (i % 3 == 1) { arrow.GetComponent<Image>().color = Color.gray; }
				else { arrow.GetComponent<Image>().color = Color.white; }
			}
			GameObject start = Instantiate(_start) as GameObject;
			start.transform.SetParent(_pathRoot);
			start.GetComponent<RectTransform>().anchoredPosition = startPosition;
			GameObject end = Instantiate(_end) as GameObject;
			end.transform.SetParent(_pathRoot);
			end.GetComponent<RectTransform>().anchoredPosition = spawnPosition;
		}
	}

	/// <summary>
	/// Toggles the visibility of the path depending on argument
	/// </summary>
	/// <param name="value">"true": shows path, "false": hides path</param>
	public void TogglePath(string value = "none")
	{
		if (_pathRoot.childCount > 0)
		{
			for (int i = 0; i < _pathRoot.childCount; i++)
			{
				if (value.Equals("true")) { _pathRoot.GetChild(i).gameObject.SetActive(true); }
				else if (value.Equals("false")) { _pathRoot.GetChild(i).gameObject.SetActive(false); }
				else { _pathRoot.GetChild(i).gameObject.SetActive(!_pathRoot.GetChild(i).gameObject.activeSelf); }
			}
		}
	}

	public void ChangeFireWeight(string weight)
	{
		_orbWeights[0] = float.Parse(weight);
	}
	public void ChangeWaterWeight(string weight)
	{
		_orbWeights[0] = float.Parse(weight);
	}
	public void ChangeWoodWeight(string weight)
	{
		_orbWeights[0] = float.Parse(weight);
	}
	public void ChangeLightWeight(string weight)
	{
		_orbWeights[0] = float.Parse(weight);
	}
	public void ChangeDarkWeight(string weight)
	{
		_orbWeights[0] = float.Parse(weight);
	}
	public void ChangeHealWeight(string weight)
	{
		_orbWeights[0] = float.Parse(weight);
	}

	public void SetMaxLength(float length)
	{
		_maxLength = (int)length;
	}
}
