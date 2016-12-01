using UnityEngine;
using System.Collections.Generic;

public enum Direction{
	Right,
	Left,
	Down,
	Up
}
public class Sort : MonoBehaviour {


	[SerializeField]
	private Transform root;

	/*s
	private List<OrbImage> orbs;

	private Dictionary<string, int> orbCounts;

	void Awake()
	{
		orbs = new List<OrbImage>();
		orbCounts = new Dictionary<string, int>();
	}
	public void CollectOrbs()
	{
		for (int i = 0; i < root.childCount; i++)
		{
			ScrollImage orb = root.GetChild(i).GetComponent<ScrollImage>();
			switch (orb.State)
			{
				case OrbImage.Red:
					AddKey("Red", 1);
					break;
				case OrbImage.Blue:
					AddKey("Blue", 1);
					break;
				case OrbImage.Green:
					AddKey("Green", 1);
					break;
				case OrbImage.Dark:
					AddKey("Dark", 1);
					break;
				case OrbImage.Light:
					AddKey("Light", 1);
					break;
				case OrbImage.Health:
					AddKey("Health", 1);
					break;
				default:
					throw new System.NotImplementedException();
			}
			orbs.Add(orb.State);
		}
		SortOrbs();
	}

	public void SortOrbs()
	{
		int index = 0;
		for(int i = 0; i < orbCounts["Red"]; i++)
		{
			root.GetChild(i).GetComponent<ScrollImage>().SetOrb(OrbImage.Red);
			index++;
		}
		for(int i = 0; i < orbCounts["Blue"]; i++)
		{	
			root.GetChild(index).GetComponent<ScrollImage>().SetOrb(OrbImage.Blue);
			index++;
		}
		for(int i = 0; i < orbCounts["Dark"]; i++)
		{	
			root.GetChild(index).GetComponent<ScrollImage>().SetOrb(OrbImage.Dark);
			index++;
		}
		for(int i = 0; i < orbCounts["Light"]; i++)
		{
			root.GetChild(index).GetComponent<ScrollImage>().SetOrb(OrbImage.Light);
			index++;
		}
		for (int i = 0; i < orbCounts["Green"]; i++)
		{
			root.GetChild(index).GetComponent<ScrollImage>().SetOrb(OrbImage.Green);
			index++;
		}
		for (int i = 0; i < orbCounts["Health"]; i++)
		{
			root.GetChild(index).GetComponent<ScrollImage>().SetOrb(OrbImage.Health);
			index++;
		}
	}

	private void AddKey(string key, int value)
	{
		if (orbCounts.ContainsKey(key))
		{
			orbCounts[key]++;
		}
		else
		{
			orbCounts.Add(key, value);
		}
	}*/

	private const int MAX_PATH_LENGTH = 10;
	private const int ROW_LENGTH = 6;
	private const int COLUMN_LENGTH = 5;

	private int p1;
	private int p2;
	private int current;

	private int[ , ] board = new int[ , ]
									{ {-1, -1, -1, -1, -1, -1},
                                    {-1, -1, -1, -1, -1, -1},
                                    {-1, -1, -1, -1, -1, -1},
                                    {-1, -1, -1, -1, -1, -1},
                                    {-1, -1, -1, -1, -1, -1} };

	void FillMatches()
	{
		//Board containing only the matches
		int[,] matchBoard = CreateEmptyBoard();

		//Check For Horizontal Match
		for (int i = 0; i < ROW_LENGTH; i++)
		{
			p1 = -1;
			p2 = -1;
			for (int j = 0; j < COLUMN_LENGTH; j++)
			{
				current = board[i, j];
				//If there is a vertical match of 3 orbs, add the orbs to the match board
				if (p1 == p2 && p2 == current && current != -1)
				{
					matchBoard[i, j] = current;
					matchBoard[i, j - 1] = current;
					matchBoard[i, j - 2] = current;
				}
				p1 = p2;
				p2 = current;
			}
		}

		//Check for Vertical matches
		for (int i = 0; i < COLUMN_LENGTH; i++)
		{
			p1 = -1;
			p2 = -1;
			for (int j = 0; j < ROW_LENGTH; j++)
			{
				current = board[i, j];
				//Check for a vertical match, if yes, add to the match board
				if (p1 == p2 && p2 == current && current != -1)
				{
					matchBoard[i, j] = current;
					matchBoard[i, j - 1] = current;
					matchBoard[i, j - 2] = current;
				}
				//Advance orbs
				p1 = p2;
				p2 = current;
			}
		}

		//Use a flood fill algorithm to create a "path" for the orbs to follow
		int[,] boardCopy = CopyBoard(matchBoard);
		List<Dictionary<string, int>> matches = new List<Dictionary<string, int>>();

		for (int i = 0; i < ROW_LENGTH; i++)
		{
			for (int j = 0; j < COLUMN_LENGTH; j++)
			{
				current = boardCopy[i, j];
				if (current == -1)
				{
					continue;
				}
				List<Dictionary<string, int>> stack = new List<Dictionary<string, int>>();
				stack.Add(MakeRowColumn(i, j));
				int fillPlacement = 0;
				int[,] thisMatch = new int[,] {     { 0, 0, 0, 0, 0, 0 },
													{ 0, 0, 0, 0, 0, 0 },
													{ 0, 0, 0, 0, 0, 0 },
													{ 0, 0, 0, 0, 0, 0 },
													{ 0, 0, 0, 0, 0, 0 } };
				while (stack.Count > 0)
				{
					Dictionary<string, int> lastRC = Pop(stack);
					if (boardCopy[lastRC["Row"], lastRC["Col"]] != current) {
						continue;
					}
					++fillPlacement;
					boardCopy[lastRC["Row"], lastRC["Col"]] = -1;
					thisMatch[lastRC["Row"], lastRC["Col"]] = 1;
					
					if (lastRC["Row"] > 0)
					{
						stack.Add(MakeRowColumn(lastRC["Row"] - 1, lastRC["Col"]));
					}
					if(lastRC["Row"] < ROW_LENGTH - 1)
					{
						stack.Add(MakeRowColumn(lastRC["Row"] + 1, lastRC["Col"]));
					}
					if(lastRC["Col"] > 0)
					{
						stack.Add(MakeRowColumn(lastRC["Row"], lastRC["Col"] - 1));
					}
					if(lastRC["Col"] < COLUMN_LENGTH - 1)
					{
						stack.Add(MakeRowColumn(lastRC["Row"], lastRC["COL"] + 1));
					}
				}

				bool isRow = false;
				for(int k = 0; k < ROW_LENGTH; k++)
				{
					if(thisMatch[k , 0] == 1 && thisMatch[k , 1] == 1 && thisMatch[k , 2] == 1 && thisMatch[k , 3] == 1 && thisMatch[k , 4] == 1 && thisMatch[k , 5] == 1)
					{
						isRow = true;
					}
				}
				matches.Add(MakeMatch(current, fillPlacement, isRow));
			}
		}
	}

	Dictionary<string, int> MakeRowColumn(int row, int col)
	{
		return new Dictionary<string, int>() { { "row", row }, { "col", col} };
	}

	int[ , ] CopyBoard(int[ , ] copy)
	{
		int[,] copiedBoard = new int[ROW_LENGTH, COLUMN_LENGTH];
		for(int i = 0; i < ROW_LENGTH; i++)
		{
			for(int j = 0; j < COLUMN_LENGTH; j++)
			{
				copiedBoard[i, j] = copy[i, j];
			}
		}

		return copiedBoard;
	}
	void CreateBoard()
	{
		int boardIndex = 0;
		for(int i = 0; i < ROW_LENGTH; i++)
		{
			for (int j = 0; j < COLUMN_LENGTH; i++)
			{
				boardIndex++;
				board[i , j] = (int)root.GetChild(boardIndex).GetComponent<ScrollImage>().State;
			}
		}
	}

	Dictionary<string, int> Pop(List<Dictionary<string,int>> list)
	{
		Dictionary<string, int> last = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return last; 
	}

	Dictionary<string, int> MakeMatch(int orb, int count, bool isRow)
	{
		Dictionary<string, int> match = new Dictionary<string, int>();
		match.Add("Orb", orb);
		match.Add("Count", count);
		match.Add("isRow", isRow ? 1 : 0);

		return match;
	}
	int[ , ] CreateEmptyBoard()
	{
		int[,] emptyBoard = new int[ROW_LENGTH, COLUMN_LENGTH ];
		for(int i = 0; i < ROW_LENGTH; i++)
		{
			for(int j = 0; j < COLUMN_LENGTH; j++)
			{
				emptyBoard[i, j] = 0;
			}
		}

		return emptyBoard;
	}

	void SwitchOrb(int np2, int c, int n)
	{
		p1 = np2;
		p2 = c;
		current = n;
	}

	bool CheckCanMove()
	{
		Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
		Vector3[] objectCorners = new Vector3[4];
		root.GetComponent<RectTransform>().GetWorldCorners(objectCorners);
		bool isOverFlowing = false;

		foreach( Vector3 corner in objectCorners)
		{
			if (!screenRect.Contains(corner))
			{
				isOverFlowing = true;
				break;
			}
		}

		return isOverFlowing;
	}
}
