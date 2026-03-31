Treatment the game: [wiki](https://en.wikipedia.org/wiki/Peg_solitaire)

In this example used triangular, 15-holes variant as easiest to solve.

Due to shape of game board it needs to use 3 possible action of jump. One of it, the horizontal move is shown below:
```cs
Expression<Predicate<Spot>> FullSpot = S => S.Full;
Expression<Predicate<Spot>> EmptySpot = S => !S.Full;

ActionPDDL HorizontalJump = new ActionPDDL("Horizontal jump");

HorizontalJump.AddPrecondition<Spot, Spot>("Jumping peg exists", ref JumpingPeg, FullSpot);
HorizontalJump.AddPrecondition<Spot, Spot>("Remove peg exists", ref RemovePeg, FullSpot);
HorizontalJump.AddPrecondition<Spot, Spot>("Final position of peg is empty", ref FinalPegPos, EmptySpot);

Expression<Predicate<Spot, Spot, Spot>> Horizontalcollinear = ((JP, RP, FPP) => (JP.Row == RP.Row && RP.Row == FPP.Row));
HorizontalJump.AddPrecondition("The same vertical line", ref JumpingPeg, ref RemovePeg, ref FinalPegPos, Horizontalcollinear);

Expression<Predicate<Spot, Spot>> VerticalClose = ((S1, S2) => ((S1.Column - S2.Column) == 1 || (S1.Column - S2.Column) == -1));
HorizontalJump.AddPrecondition("Jumper is close", ref JumpingPeg, ref RemovePeg, VerticalClose);
HorizontalJump.AddPrecondition("Hole is close", ref FinalPegPos, ref RemovePeg, VerticalClose);

HorizontalJump.AddEffect("Jumping Peg Spot is empty", ref JumpingPeg, JP => JP.Full, false);
HorizontalJump.AddEffect("Remove Peg Spot is empty", ref RemovePeg, RP => RP.Full, false);
HorizontalJump.AddEffect("Final Peg Spot is full", ref FinalPegPos, RP => RP.Full, true);
```
The execution of it uses effects from  above and static voids of Spot class.
```cs
HorizontalJump.AddExecution("Reset colours", () => Reset(), false);
HorizontalJump.AddExecution("Jumping Peg Spot is empty");
HorizontalJump.AddExecution("Remove Peg Spot is empty");
HorizontalJump.AddExecution("Final Peg Spot is full");
HorizontalJump.AddExecution("Draw it", () => Board.Draw(spots), true);
HorizontalJump.AddExecution("Wait", () => Thread.Sleep(1500), true);
```
At this case it's possible to reach 3016 possible states, which is generated in time of about 13s.

![Peg_solitaire_solution](https://github.com/user-attachments/assets/4c7a440f-be36-4bcc-a737-d9266bb88809)
