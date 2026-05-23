Treatment the puzzle: [wiki](https://en.wikipedia.org/wiki/Wolf,_goat_and_cabbage_problem)

To model this puzzle used some types of classes. On a boat is a room for only one good (like cabbage, goat or wolf), but on a river bank are 3 rooms for every good type one one.

![River Crossing Obj](https://github.com/user-attachments/assets/962efc14-2b02-4679-a2ca-06e125fda1e3)

There are 7 actions to solve it - 6 relevant to transhipment and 1 to cross a river.

Putting a thing to the boat:
```cs
ActionPDDL TakingCabbage = new ActionPDDL("Taking cabbage");
TakingCabbage.AddPartOfActionSententia("Take the cabbage.");
TakingCabbage.AddPrecondition<RiverBank, RiverBank, Boat, Boat>("Boat is near the bank", ref nextToBank, ref boat, (bank, b) => bank.boat == b);
TakingCabbage.AddPrecondition<RiverBank, RiverBank, Cabbage, Cabbage>("Cabbage is at the bank", ref nextToBank, ref cabbage, (bank, c) => bank.cabbage == c);
TakingCabbage.AddPrecondition("Boat is empty", ref boat, b => b.good == null);
TakingCabbage.AddEffect("Remove the cabbage from the bank", ref nextToBank, b => b.cabbage, null);
TakingCabbage.AddEffect("Put the cabbage on the boat", ref boat, b => b.good, ref cabbage);
```

Putting a thing away:
```cs
ActionPDDL PutCabbageAway = new ActionPDDL("Putting cabbage away");
PutCabbageAway.AddPartOfActionSententia("Put the cabbage away.");
PutCabbageAway.AddPrecondition<RiverBank, RiverBank, Boat, Boat>("Boat is near the bank", ref nextToBank, ref boat, (n, b) => n.boat == b);
PutCabbageAway.AddPrecondition<Boat, Boat, Cabbage, Cabbage>("There is the cabbage on the boat", ref boat, ref cabbage, (b, c) => b.good == c);
PutCabbageAway.AddEffect("Empty the boat", ref boat, b => b.good, null);
PutCabbageAway.AddEffect("Add the cabbage to the bank", ref nextToBank, b => b.cabbage, ref cabbage);
```

One needs to use the above 3 times. For the cabbage, goat and wolf.

Going to the other river bank:
```cs
ActionPDDL CrossTheRiver = new ActionPDDL("Crossing the river");
CrossTheRiver.AddPartOfActionSententia("Cross the river.");
CrossTheRiver.AddPrecondition<RiverBank, RiverBank, Boat, Boat>("Boat is near the bank", ref nextToBank, ref boat, (ba, bo) => ba.boat == bo);
CrossTheRiver.AddPrecondition("Nothing won't be eaten", ref nextToBank, b => b.goat != null ? (b.cabbage == null && b.wolf == null) : true);
CrossTheRiver.AddEffect("Leave the river bank", ref nextToBank, b => b.boat, null);
CrossTheRiver.AddEffect("Go to the other bank", ref SecendBank, b => b.boat, ref boat);
```

It all can be shown in one "case use diagram" of whole domain.

![River case use diagram](https://github.com/user-attachments/assets/052e543b-27e8-4ec2-a46f-bad41336e4f1)

Goal is described as every goods' slot at the final river bank is not empty.
```cs
var ExpectedState = new List<Expression<Predicate<RiverBank>>>
{
    RB => 
        RB.cabbage != null && 
        RB.goat != null && 
        RB.wolf != null
};

GoalPDDL crossTheRiver = new GoalPDDL("Cross the river");
crossTheRiver.AddExpectedObjectState(NorthVistulaBank, ExpectedState);
```

Generated plan:
```
1: Take the goat.
2: Cross the river.
3: Put the goat away.
4: Cross the river.
5: Take the cabbage.
6: Cross the river.
7: Put the cabbage away.
8: Take the goat.
9: Cross the river.
10: Put the goat away.
11: Take the wolf.
12: Cross the river.
13: Put the wolf away.
14: Cross the river.
15: Take the goat.
16: Cross the river.
17: Put the goat away.
```

There are 2 optimal solutions, as you can see in generated diagram below. At first case one needs to transport cabbage after transport of goat. At the other - a wolf. Scenario being at realization is marked by orange colour.

![River state diagram](https://github.com/user-attachments/assets/090a541e-fe88-4a3f-b958-757f11fcbd30)