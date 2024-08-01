![Logo](https://onedrive.live.com/embed?resid=5B6E90429D9C8454%21346129&authkey=%21AIQv_XycrJiLVlI&width=1127&height=319)

This is the class library based on PDDL intellection. It uses only C# 7.0 standard library. The library "translate" one's non-abstract, public classes to own format which is use to make simulation of possible actions. Values inside classes using to find solution have to be ValueType only (most numeric, like int, short etc., char, bool).

> [!WARNING]
> Library has many bugs, works unstable so is not to use, still.

One can to use previously defined classes which are useing in other part of one's programm, like:
```cs
public class HanoiObj //Sorry it cannot be abstract
{
    public int HanoiObjSizeUpSide = 0;
    public bool IsEmptyUpSide;
}

public class HanoiBrick : HanoiObj
{
    readonly public int Size;
}

public class HanoiTable : HanoiObj
{
    public readonly int no;
}
```
Instances of class used to define action shouldn't be use in other part of program. In time of create actions library create class instance excluding use the class constructor.

For these classes one can define rules in library like "Move brick onto another brick" or "Move brick on table". Preconditions, effect etc. are phrased by library's user as Expressions (System.Linq.Expressions):
```cs
DomeinPDDL newDomein = new DomeinPDDL("Hanoi");

HanoiBrick MovedBrick = null;
HanoiObj ObjBelowMoved = null;
HanoiBrick NewStandB = null;
HanoiTable NewStandT = null;

Expression<Predicate<HanoiObj>> MovedBrickIsNoUp = (HO => HO.IsEmptyUpSide);
Expression<Predicate<HanoiBrick, HanoiBrick>> PutSmallBrickAtBigger = ((MB, NSB) => (MB.Size < NSB.Size));
Expression<Predicate<HanoiBrick, HanoiObj>> FindObjBelongMovd = ((MB, OBM) => (MB.Size == OBM.HanoiObjSizeUpSide));

ActionPDDL moveBrickOnBrick = new ActionPDDL("Move brick onto another brick");

moveBrickOnBrick.AddAssignedParametr(ref MovedBrick, "Place the {0}-size brick ", MB => MB.Size);
moveBrickOnBrick.AddAssignedParametr(ref NewStandB, "onto {0}-size brick.", MB => MB.Size);

moveBrickOnBrick.AddPrecondiction("Moved brick is no up", ref MovedBrick, MovedBrickIsNoUp);
moveBrickOnBrick.AddPrecondiction("New stand is empty", ref NewStandB, MovedBrickIsNoUp);
moveBrickOnBrick.AddPrecondiction("Small brick on bigger one", ref MovedBrick, ref NewStandB, PutSmallBrickAtBigger);
moveBrickOnBrick.AddPrecondiction("Find brick bottom moved one", ref MovedBrick, ref ObjBelowMoved, FindObjBelongMovd);

moveBrickOnBrick.AddEffect("New stand is full", false, ref NewStandB, NS => NS.IsEmptyUpSide);
moveBrickOnBrick.AddEffect("Old stand is empty", true, ref ObjBelowMoved, NS => NS.IsEmptyUpSide);
moveBrickOnBrick.AddEffect("UnConsociate Objs", 0, ref ObjBelowMoved, OS => OS.HanoiObjSizeUpSide);
moveBrickOnBrick.AddEffect("Consociate Bricks", ref MovedBrick, MB => MB.Size, ref NewStandB, NSB => NSB.HanoiObjSizeUpSide);

newDomein.AddAction(moveBrickOnBrick);

ActionPDDL moveBrickOnTable = new ActionPDDL("Move brick on table");

moveBrickOnTable.AddAssignedParametr(ref MovedBrick, "Place the {0}-size brick ", MB => MB.Size);
moveBrickOnTable.AddAssignedParametr(ref NewStandT, "onto table no {0}.", NS => NS.no);

moveBrickOnTable.AddPrecondiction("Moved brick is no up", ref MovedBrick, MovedBrickIsNoUp);
moveBrickOnTable.AddPrecondiction("New table is empty", ref NewStandT, MovedBrickIsNoUp);
moveBrickOnTable.AddPrecondiction("Find brick bottom moved one", ref MovedBrick, ref ObjBelowMoved, FindObjBelongMovd);

moveBrickOnTable.AddEffect("New stand is full", false, ref NewStandT, NS => NS.IsEmptyUpSide);
moveBrickOnTable.AddEffect("Old stand is empty", true, ref ObjBelowMoved, NS => NS.IsEmptyUpSide);
moveBrickOnTable.AddEffect("UnConsociate Objs", 0, ref ObjBelowMoved, OS => OS.HanoiObjSizeUpSide);
moveBrickOnTable.AddEffect("Consociate Bricks", ref MovedBrick, MB => MB.Size, ref NewStandT, NST => NST.HanoiObjSizeUpSide);

newDomein.AddAction(moveBrickOnTable);
```

Solution output for 3-bricks-hanoi-tower problem:
```
Transfer bricks onto table no. 3 determined!!!
Move brick on table: Place the 1-size brick onto table no 2.
Move brick on table: Place the 2-size brick onto table no 1.
Move brick onto another brick: Place the 1-size brick onto 2-size brick.
Move brick on table: Place the 3-size brick onto table no 2.
Move brick on table: Place the 1-size brick onto table no 0.
Move brick onto another brick: Place the 2-size brick onto 3-size brick.
Move brick onto another brick: Place the 1-size brick onto 2-size brick.
```
---
License: [Creative Commons Attribution-NonCommercial-ShareAlike 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode)
