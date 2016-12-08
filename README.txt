##PADSolver

#Project by Johnny Chen and Leah Magrane

#What is PADSolver?
PADSolver is a puzzle board solver for the mobile game Puzzle & Dragons.
The application can be used to find the optimal path the player should take to reach the most number of combos.
The player can also adjust certain parameters for their solution such as the length of the optimal path and adjusting what weight each orb has in the solution.
PADSolver also gives the top 30 results so the user may select whichever solution they find best.

#Usage
The user inputs the board by clicking on each empty orb to scroll through the different orb types. [Fire(Red) -> Water(Blue) -> Wood(Green) -> Light(Yellow) -> Dark(Purple)]

**Solve** - Runs the solver and displays the solutions in the *Solution List*
**Show Matches** - Shows the board with all the matches lined up of the selected solution
**Drop Matches** - Shows the board with all the matches gone of the selected solution
**Reset** - Resets the board to display the original unsolved board
**Randomize** - Randomizes the board (good for learning)
**Clear** - Empties the board
**Toggle Path** - Show/Hides the optimal path from the selected solution

*Orb Weights* - Allows the user to input the weights of each orb which determines which orbs to prioritize in matching when finding the solution(s)
*Maximum Path Length* - Allows the user to adjust how many orb movements can be done in the solution (longer path lengths = longer computational time for solution)
*Solution List* - Displays the top 30 solutions PADSolver found and clicking on a solution displays the path associated with the solution on the board

#What is the algorithm?
*Note: The code for the algorithm is spread between multiple functions in the Solver script
The algorithm is a heuristic search on a 2d array of Solution objects.
Each Solution object has a board state (solutionBoard), the weight of the solution (totalWeight), the solution path (path), and the list of matches (matches).
The search starts with 30 solutions with the starting orb of each solution being each of the 30 different orbs on the standard 6x5 board.
The algorithm steps by moving the start orb in each solution in each of the available directions (Up, Down, Left, and Right) then checking the board state. Diagonal directions are ignored as they are unreliable in the game.
When checking the board state, the algorithm recalculates the new weight of the solution.
After checking all direction, the solution list is sorted by weight and then pruned to 98 solutions:
-48 solutions where 12 orbs can move only 4 directions
-42 solutions where 14 orbs can move only 3 directions
-8 solutions where 4 orbs can move only 2 directions
Visual Diagram of board with possible number of movements:
------------
|2 3 3 3 3 2|
|3 4 4 4 4 3|
|3 4 4 4 4 3|
|3 4 4 4 4 3|
|2 3 3 3 3 2|
-------------
The algorithm steps up to the maximum path length.
Once the algorithm finishes stepping, it once again prunes the solution list to 30 solution as that is the starting size of the solution list.

#Was it a success?
We think the project is very successful.
The algorithm works and solves the board better than, if not, just as well as, a human player.
A random board in the game gives an average of 6 combos and the algorithm produces solutions with at least 6 combos every time it runs on a random board.
