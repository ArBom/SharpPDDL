Treatment the puzzle: [wiki](https://en.wikipedia.org/wiki/Wolf,_goat_and_cabbage_problem)

Putting a thing to the boat:
```cs
    ActionPDDL TakingCabbage = new ActionPDDL("TakingCabbage");
    TakingCabbage.AddPartOfActionSententia("Take the cabbage.");
    TakingCabbage.AddPrecondition("Boat is near the bank", ref nextToBank, b => b.IsBoat);
    TakingCabbage.AddPrecondition("Cabbage is at the bank", ref nextToBank, b => b.IsCabbage);
    TakingCabbage.AddPrecondition("Boat is empty", ref boat, b => !b.IsCabbage && !b.IsGoat && !b.IsWolf);
    TakingCabbage.AddEffect("Remove the cabbage from the bank", ref nextToBank, b => b.IsCabbage, false);
    TakingCabbage.AddEffect("Put the cabbage on the boat", ref boat, b => b.IsCabbage, true);
    RiverCrossing.AddAction(TakingCabbage);
```

Putting a thing away:
```cs
    ActionPDDL PutCabbageAway = new ActionPDDL("PuttingCabbageAway");
    PutCabbageAway.AddPartOfActionSententia("Put the cabbage away.");
    PutCabbageAway.AddPrecondition("Boat is near the bank", ref nextToBank, b => b.IsBoat);
    PutCabbageAway.AddPrecondition("Goat is on the bank", ref boat, b => b.IsCabbage);
    PutCabbageAway.AddEffect("Remove the goat from the bank", ref nextToBank, b => b.IsCabbage, true);
    PutCabbageAway.AddEffect("Add the goat to the boat", ref boat, b => b.IsCabbage, false);
    RiverCrossing.AddAction(PutCabbageAway);
```

One need to use the above 3 times. For the cabbage, goat and wolf.

Going to the other river bank:
```cs
    ActionPDDL CrossTheRiver = new ActionPDDL("CrossingTheRiver");
    CrossTheRiver.AddPartOfActionSententia("Cross the river.");
    CrossTheRiver.AddPrecondition("Boat is near the bank", ref nextToBank, b => b.IsBoat);
    CrossTheRiver.AddPrecondition("Nothing won't be eaten", ref nextToBank, b => b.IsGoat ? (!b.IsCabbage && !b.IsWolf) : true );
    RiverBank SecendBank = null;
    CrossTheRiver.AddEffect("Leave the river bank", ref nextToBank, b => b.IsBoat, false);
    CrossTheRiver.AddEffect("Go to the other bank", ref SecendBank, b => b.IsBoat, true);
    RiverCrossing.AddAction(CrossTheRiver);
```

Generated plan:
```
1: Take the goat.
2: Cross the river.
3: Put the goat away.
4: Cross the river.
5: Take the wolf.
6: Cross the river.
7: Put the wolf away.
8: Take the goat.
9: Cross the river.
10: Put the goat away.
11: Take the cabbage.
12: Cross the river.
13: Put the cabbage away.
14: Cross the river.
15: Take the goat.
16: Cross the river.
17: Put the goat away.
```