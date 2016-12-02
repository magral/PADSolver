using UnityEngine;
using System.Collections.Generic;

public class Solver : MonoBehaviour {

	/* Coordinate class - holds row and column information */
	public class Coords
	{
		public int row, col;

		public Coords(int x, int y)
		{
			row = x;
			col = y;
		}

		public Coords(Coords coord)
		{
			row = coord.row;
			col = coord.col;
		}
	}

	/* Solution Data class - holds relevant data for each solution */
	public class SolutionData
	{
		public OrbType[,] solutionBoard;
		public Coords currentCursorPos;
		public Coords startOrbPos;
		public float totalWeight;
		public List<int> path;
		public List<MatchData> matches;
		public bool isChecked;

		public SolutionData(OrbType[,] board)
		{
			solutionBoard = (OrbType[,])board.Clone();
			currentCursorPos = new Coords(0,0);
			startOrbPos = new Coords(0,0);
			totalWeight = 0;
			path = new List<int>();
			matches = new List<MatchData>();
			isChecked = false;
		}

		public SolutionData(SolutionData solution)
		{
			solutionBoard = (OrbType[,])solution.solutionBoard.Clone();
			currentCursorPos = new Coords(solution.currentCursorPos);
			startOrbPos = new Coords(solution.startOrbPos);
			totalWeight = solution.totalWeight;
			path = solution.path;
			matches = solution.matches;
			isChecked = solution.isChecked;
		}
	};

	/* Match Data class - holds relevant data about a match */
	public class MatchData
	{
		public OrbType orbType;
		public int numOrbs;

		public MatchData(OrbType orb, int count)
		{
			orbType = orb;
			numOrbs = count;
		}

		public MatchData(MatchData match)
		{
			orbType = match.orbType;
			numOrbs = match.numOrbs;
		}
	}

	/* Constants and public variables */
	public const int ORB_SIZE = 80;
	public const float ORB_MULTIPLIER = 0.25f;
	public const float COMBO_MULTIPLIER = 0.25f;
	public const int MAX_NUM_SOLUTIONS = 30;

	[SerializeField]
	private Transform root;

	/* Private Variables */
	private List<SolutionData> _solutions;
	private SolutionData _selectedSolution;
	private OrbType[,] _board;
	private OrbType[,] _initialBoard;
	private float[] _orbWeights;
	private int _rows = 5;
	private int _cols = 6;
	private int _maxLength = 25;
	private int _step = 0;

	/* Initialization */
	void Awake()
	{
		_solutions = new List<SolutionData>();
		_board = new OrbType[_rows, _cols];
		_orbWeights = new float[6];
	}

	/* Private Functions */
	/// <summary>
	/// Updates internal representation of puzzle board
	/// </summary>
	void GetBoard()
	{
		for (int i = 0, r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++, i++)
			{
				_board[r, c] = root.GetChild(i).GetComponent<ScrollImage>().State;
				//Debug.Log(i + ":" + _board[r, c]);
			}
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
	SolutionData FindMatches(SolutionData solution)
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
				OrbType currentOrb = solution.solutionBoard[i, j];
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
				OrbType currentOrb = solution.solutionBoard[i, j];
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
				if (matchesBoard[r, c] == OrbType.Undefined)
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
			float weight = _orbWeights[(int)match.orbType];
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
		while(true)
		{
			SolutionData matchesBoardData = FindMatches(solution);
			List<MatchData> foundMatches = matchesBoardData.matches;
			if (foundMatches.Count <= 0) { break; }
			currBoard = RemoveMatches(currBoard,matchesBoardData.solutionBoard);
			allMatches.AddRange(matchesBoardData.matches);
		}
		solution.totalWeight = ComputeTotalWeight(allMatches);
		solution.matches = allMatches;
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
	bool CanMoveOrb(SolutionData solution, int dir)
	{
		if (solution.path.Count > 0 && solution.path[solution.path.Count-1] == (dir + 2) % 4) { return false; }
		else
		{

			switch(dir)
			{
				case 0: return solution.currentCursorPos.col < _cols - 1;	//Down
				case 1: return solution.currentCursorPos.row < _rows - 1;	//Right
				case 2: return solution.currentCursorPos.col > 0;			//Up
				case 3: return solution.currentCursorPos.row > 0;           //Left
				default: return false;
			}
		}
	}

	/// <summary>
	/// Swaps the two orbs after moving in given direction
	/// </summary>
	SolutionData SwapOrbs(SolutionData solution, int dir)
	{
		Coords oldCursorPos = new Coords(solution.currentCursorPos);
		switch(dir)
		{
			case 0: solution.currentCursorPos.col++; break;
			case 1: solution.currentCursorPos.row++; break;
			case 2: solution.currentCursorPos.col--; break;
			case 3: solution.currentCursorPos.row--; break;
		}
		OrbType orbTemp = solution.solutionBoard[oldCursorPos.row, oldCursorPos.col];
		solution.solutionBoard[oldCursorPos.row, oldCursorPos.col] = solution.solutionBoard[solution.currentCursorPos.row, solution.currentCursorPos.col];
		solution.solutionBoard[solution.currentCursorPos.row, solution.currentCursorPos.col] = orbTemp;
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
			if (solution.isChecked) { break; }
			for (int dir = 0; dir < 4; dir++)
			{
				if(!CanMoveOrb(solution,dir)) { continue; }
				SolutionData newSolution = new SolutionData(solution);
				newSolution = EvaluateSolution(SwapOrbs(newSolution, dir));
				newSolutions.Add(newSolution);
			}
			solution.isChecked = true;
		}
		_solutions.AddRange(newSolutions);
		_solutions.Sort((a, b) => (int)(b.totalWeight - a.totalWeight));
		return _solutions.GetRange(0, MAX_NUM_SOLUTIONS); 
	}

	/// <summary>
	/// Shows the board on the front-end side
	/// </summary>
	void ShowBoard()
	{
		for (int i = 0, r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++, i++)
				root.GetChild(i).GetComponent<ScrollImage>().SetOrb(_board[r,c]);
	}

	/* Public Functions */
	/// <summary>
	/// Main function that runs search algorithm for solutions
	/// </summary>
	public void SolveBoard()
	{
		GetBoard();
		/* DEBUG
		string outputBoard = "_board = [ ";
		for (int r = 0; r < _rows; r++)
		{
			outputBoard += "[ ";
			for (int c = 0; c < _cols; c++)
			{
				outputBoard += _board[r, c] + " ";
			}
			outputBoard += "] ";
		}
		outputBoard += "]";
		Debug.Log(outputBoard);
		// END DEBUG */
		if (!CheckFilledBoard()) { return; }
		_initialBoard = (OrbType[,])_board.Clone();
		SolutionData baseSolution = new SolutionData(_board);
		baseSolution = EvaluateSolution(baseSolution);
		for (int r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++)
				_solutions.Add(MakeSolutionWithCursor(baseSolution, r, c));
		while(_step < _maxLength)
		{
			_step++;
			_solutions = StepSolutions();
		}
		_selectedSolution = _solutions[0];
		// /* DEBUG
		string output = "matches = [ ";
		foreach (MatchData match in _selectedSolution.matches)
		{
			output += "(" + match.orbType.ToString() + ", " + match.numOrbs.ToString() + ") ";
		}
		output += "]";
		Debug.Log(output);
		// END DEBUG */
	}

	/// <summary>
	/// Randomizes the orbs on the board
	/// </summary>
	public void RandomizeBoard()
	{
		do
		{
			for (int i = 0, r = 0; r < _rows; r++)
				for (int c = 0; c < _cols; c++, i++)
				{
					OrbType randomOrb = (OrbType)Random.Range(1, 7);
					root.GetChild(i).GetComponent<ScrollImage>().SetOrb(randomOrb);
					_board[r, c] = randomOrb;
				}
			/*
			// DEBUG
			string output = "matches = [ ";
			foreach(MatchData match in FindMatches(new SolutionData(_board)).matches)
			{
				output += "(" + match.orbType.ToString() + ", " + match.numOrbs.ToString() + ") ";
			}
			output += "]";
			Debug.Log(output);
			*/
		} while (FindMatches(new SolutionData(_board)).matches.Count != 0);
	}

	/// <summary>
	/// Resets the orbs on the board to be unknown
	/// </summary>
	public void ClearBoard()
	{
		for (int i = 0, r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++, i++)
			{
				root.GetChild(i).GetComponent<ScrollImage>().SetOrb(OrbType.None);
				_board[r, c] = OrbType.None;
			}
	}

	/// <summary>
	/// Resets the board to its inital state
	/// </summary>
	public void ResetBoard()
	{
		for (int i = 0, r = 0; r < _rows; r++)
			for (int c = 0; c < _cols; c++, i++)
			{
				OrbType oldOrb = _initialBoard[r, c];
				root.GetChild(i).GetComponent<ScrollImage>().SetOrb(oldOrb);
				_board[r, c] = oldOrb;
			}
	}

	/// <summary>
	/// Shows the solution board
	/// </summary>
	public void ShowFinalBoard()
	{
		_board = (OrbType[,])_selectedSolution.solutionBoard.Clone();
		ShowBoard();
	}
}
