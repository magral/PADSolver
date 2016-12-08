#PADSolver 
Project by Johnny Chen and Leah Magrane

##What is PADSolver?
PADSolver is a puzzle board solver for the mobile game Puzzle & Dragons.
The application can be used to find the optimal path the player should take to reach the most number of combos.
The player can also adjust certain parameters for their solution such as the length of the optimal path and adjusting what weight each orb has in the solution.
PADSolver also gives the top 30 results so the user may select whichever solution they find best.

##Usage
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

##Things to improve/add
* Allow user to input board through text
* Orb movement animation so path is easier to see
* Aesthetic improvements to UI elements
* Improve algorithm for solving board
