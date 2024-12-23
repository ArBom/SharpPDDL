![thumbnail](https://github.com/user-attachments/assets/541bf944-0334-4426-87b2-78ce19577ba9)


This is the class library based on PDDL intellection and in effect it's a implementation of GOAP (Goal Oriented Action Planning) algorithm. It uses only C# 7.3 standard library. Values inside classes using to find solution have to be ValueType only (most numeric, like: int, short etc., char, bool).

> [!WARNING]
> Library has many bugs, works unstable so is not to use, still.

One can to use previously defined classes which are using in other part of one's programm. At this version library can return the plan of doing to realize the goal. Examples of problems possible to solution by this algorithm:

<details> 
  <summary>Tower of Hanoi</summary>
Treatment the puzzle: [wiki](https://en.wikipedia.org/wiki/Tower_of_Hanoi)
    
```cs
public class HanoiObj //It cannot be abstract
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
```mermaid

classDiagram

namespace Legend {

    class Class{
        Its a block representant some class
    }

    class Object {
        Its a block representant some object / class instance
    }

}

    style Object fill:#391, stroke-style:..
    style Class fill:#139, stroke-style:..

namespace HanoiTower {

    class HanoiObj{
        +int HanoiObjSizeUpSide
        +bool IsEmptyUpSide
    }

    class HanoiBrick{
        +int Size
    }

    class HanoiTable {
        +int no
    }
}
    HanoiObj <|-- HanoiBrick
    HanoiObj <|-- HanoiTable

    style HanoiObj fill:#139, stroke-style:..
    style HanoiBrick fill:#139, stroke-style:..
    style HanoiTable fill:#139, stroke-style:..

namespace SharpPDDL {

    class Root_TreeNode{
        ~SingleTypeOfDomein Content
        ~List~TreeNode~ Children 
    }

    class HanoiObj_SingleTypeOfDomein {
        ~Type Type : BaseShapes.HanoiObj
        ~List~ValueOfThumbnail~ CumulativeValues 
    }

    class 0_TreeNode{
        ~SingleTypeOfDomein Content
        ~List~TreeNode~ Children 
    }

    class HanoiBrick_SingleTypeOfDomein {
        ~Type Type : BaseShapes.HanoiObj
        ~List~ValueOfThumbnail~ CumulativeValues 
    }

    class 1_TreeNode{
        ~SingleTypeOfDomein Content
        ~List~TreeNode~ Children 
    }

    class HanoiTable_SingleTypeOfDomein {
        ~Type Type : BaseShapes.HanoiObj
        ~List~ValueOfThumbnail~ CumulativeValues 
    }
}
    style Root_TreeNode fill:#391, stroke-style:..
    style 0_TreeNode fill:#391, stroke-style:..
    style 1_TreeNode fill:#391, stroke-style:..
    style HanoiObj_SingleTypeOfDomein fill:#391, stroke-style:..
    style HanoiBrick_SingleTypeOfDomein fill:#391, stroke-style:..
    style HanoiTable_SingleTypeOfDomein fill:#391, stroke-style:..
    
    Root_TreeNode --> "Children[0]" 0_TreeNode
    Root_TreeNode --> "Children[1]" 1_TreeNode
    0_TreeNode --> "Content" HanoiBrick_SingleTypeOfDomein
    1_TreeNode --> "Content" HanoiTable_SingleTypeOfDomein
    Root_TreeNode --> "Content" HanoiObj_SingleTypeOfDomein
    HanoiObj_SingleTypeOfDomein ..> "≙" HanoiObj
    HanoiBrick_SingleTypeOfDomein ..> "≙" HanoiBrick
    HanoiTable_SingleTypeOfDomein ..> "≙" HanoiTable

    note for HanoiObj_SingleTypeOfDomein "CumulativeValues:<br> 1: HanoiObSizeUpSide<br> 2: IsEmptyUpSide"
    note for HanoiTable_SingleTypeOfDomein "CumulativeValues:<br> 1: HanoiObSizeUpSide<br> 2: IsEmptyUpSide<br> // int:no is not use in any action"
    note for HanoiBrick_SingleTypeOfDomein "CumulativeValues:<br> 1: HanoiObSizeUpSide<br> 2: IsEmptyUpSide<br> 3: Size"

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
Transfer bricks onto table no. 3 determined!!! Total Cost: 7
Move brick on table: Place the 1-size brick onto table no 2.
Move brick on table: Place the 2-size brick onto table no 1.
Move brick onto another brick: Place the 1-size brick onto 2-size brick.
Move brick on table: Place the 3-size brick onto table no 2.
Move brick on table: Place the 1-size brick onto table no 0.
Move brick onto another brick: Place the 2-size brick onto 3-size brick.
Move brick onto another brick: Place the 1-size brick onto 2-size brick.
```
</details>
<details> 
  <summary>Water pouring puzzle</summary>
Treatment the puzzle: [wiki](https://en.wikipedia.org/wiki/Water_pouring_puzzle) 
    
  ```cs
public class WaterJug
{
    public readonly float Capacity;
    public float flood;

    public static int DecantedWater (float SourceFlood, float DestinationCapacity, float DestinationFlood)
    {
        if (SourceFlood + DestinationFlood > DestinationCapacity)
           return (int)(DestinationCapacity - DestinationFlood);
        else
           return (int)SourceFlood;
    }
    ⁝

}
```    
```cs
DomeinPDDL DecantingDomein = new DomeinPDDL("decanting problems");

ActionPDDL DecantWater = new ActionPDDL("Decant water");
WaterJug SourceJug = null;
WaterJug DestinationJug = null;

DecantWater.AddAssignedParametr(ref SourceJug, "from {0}-liter jug ", SJ => SJ.Capacity);
DecantWater.AddAssignedParametr(ref DestinationJug, "to the {0}-liter jug.", DJ => DJ.Capacity);

DecantWater.AddPrecondiction("Source Jug is not empty", ref SourceJug, Source_Jug => (Source_Jug.flood != 0));
DecantWater.AddPrecondiction("Destination Jug is not full", ref DestinationJug, Destination_Jug => Destination_Jug.flood < Destination_Jug.Capacity);

DecantWater.AddEffect(
    "Reduce source jug flood", 
    ref DestinationJug, 
    (Source_Jug, Destination_Jug) => Destination_Jug.flood + Source_Jug.flood >= Destination_Jug.Capacity ? Source_Jug.flood - Destination_Jug.Capacity + Destination_Jug.flood : 0,
    ref SourceJug, 
    Source_Jug => Source_Jug.flood );

DecantWater.AddEffect(
    "Increase destination jug flood",
    ref SourceJug,
    (Destination_Jug, Source_Jug) => Destination_Jug.flood + Source_Jug.flood >= Destination_Jug.Capacity ? Destination_Jug.Capacity : Destination_Jug.flood + Source_Jug.flood,
    ref DestinationJug,
    Destination_Jug => Destination_Jug.flood );

DecantWater.DefineActionCost(ref SourceJug, ref DestinationJug, (S, D) => WaterJug.DecantedWater(S.flood, D.Capacity, D.flood));

DecantingDomein.AddAction(DecantWater);
```
```
SharpPDDL : Divide in half determined!!! Total Cost: 22
Decant water: from 8-liter jug to the 5-liter jug. Action cost: 5
Decant water: from 5-liter jug to the 3-liter jug. Action cost: 3
Decant water: from 3-liter jug to the 8-liter jug. Action cost: 3
Decant water: from 5-liter jug to the 3-liter jug. Action cost: 2
Decant water: from 8-liter jug to the 5-liter jug. Action cost: 5
Decant water: from 5-liter jug to the 3-liter jug. Action cost: 1
Decant water: from 3-liter jug to the 8-liter jug. Action cost: 3
all states generated
```
</details>

---
License: [Creative Commons Attribution-NonCommercial-ShareAlike4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode)
