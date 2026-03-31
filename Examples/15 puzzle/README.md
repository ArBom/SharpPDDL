Treatment the game: [wiki](https://en.wikipedia.org/wiki/15_puzzle)

Due to many steps essential to solve the puzzle and complexity of problem final goal was divided for several smaller.

The part of the code responsible for adding next sub-goals:
```cs
    static void AddTileXGoal(object a, object b)
    {
        GoalPDDL goalPDDL = (GoalPDDL)a;
            int c = int.Parse(goalPDDL.Name);
            AddTileXGoal(c);
    }

    static void AddTileXGoal(int i)
    {
        if (i < 15)
        {
            GoalPDDL Tile1Goal = new GoalPDDL((i + 1).ToString());

            for (int j = 0; j < i + 1; j++)
                Tile1Goal.AddExpectedObjectState(ExpressionsOfXTile(j));

            Tile1Goal.GoalRealized += AddTileXGoal;
            GemPuzzleDomein.AddGoal(Tile1Goal);
        }
    }
```

In the beginning algorithm realize solution for tile no. 1 and add goal to put correct tiles no 1 and 2. In the end of sub-goal realize is add goal of one tile goal more.

Solution of 1st sub-goal:
```
 ╔══╤══╤═┉
 ║ 1│ ?│ 
 ╟──┼──┼─┉
 ⁞ ?⁞ ?⁞
```

Solution of 2nd sub-goal:
```
 ╔══╤══╤══╤══╗
 ║ 1│ 2│ ?│ ?║
 ╟──┼──┼──┼──╢
 ⁞ ?⁞ ?⁞ ?⁞ ?⁞
```

Solution of 5th sub-goal:
```
 ╔══╤══╤══╤══╗
 ║ 1│ 2│ 3│ 4║
 ╟──┼──┼──┼──╢
 ║ 5│ ?│ ?│ ?║
 ╟──┼──┼──┼──╢
 ⁞ ?⁞ ?⁞ ?⁞ ?⁞
```

Final goal of all tile is reach in time of less then 3 mins:
```
 ╔══╤══╤══╤══╗
 ║ 1│ 2│ 3│ 4║
 ╟──┼──┼──┼──╢
 ║ 5│ 6│ 7│ 8║
 ╟──┼──┼──┼──╢
 ║ 9│10│11│12║
 ╟──┼──┼──┼──╢
 ║13│14│15│  ║
 ╚══╧══╧══╧══╝
```