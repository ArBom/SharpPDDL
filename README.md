![thumbnail](https://github.com/user-attachments/assets/541bf944-0334-4426-87b2-78ce19577ba9)

[![CodeFactor](https://www.codefactor.io/repository/github/arbom/sharppddl/badge/master)](https://www.codefactor.io/repository/github/arbom/sharppddl/overview/master)
[![.NET class library](https://github.com/ArBom/SharpPDDL/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ArBom/SharpPDDL/actions/workflows/dotnet.yml)
[![LoC](https://raw.githubusercontent.com/ArBom/SharpPDDL/refs/heads/loc/badge.svg)](https://github.com/ArBom/SharpPDDL/blob/master/.github/workflows/loc.yml)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/ArBom/SharpPDDL?style=plastic&logo&color=4bc721)
[![NuGet Version](https://img.shields.io/nuget/vpre/SharpPDDL?style=plastic&logo=nuget&label=NuGet&color=004880&cacheSeconds=7200)](https://www.nuget.org/packages/SharpPDDL)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SharpPDDL?style=plastic&color=004880)](https://nugettrends.com/packages?ids=SharpPDDL&months=6)

---

This is the class library based on PDDL intellection and in effect it's a implementation of GOAP (Goal Oriented Action Planning) algorithm. It uses only C# 7.1 standard library. Values inside classes using to find solution have to be ValueType only (most numeric, like: int, short etc., char, bool).

> [!WARNING]
> Library is in β version still, it may works little unstable.

One can to use previously defined classes which are using in other part of one's programm. At this version library can return the plan of doing and execute it to realize the goal. Examples of problems possible to solution by this algorithm:

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
HanoiBrick MovedBrick = null; //you can take brick...
HanoiObj ObjBelowMoved = null; //...from table or another brick... 
HanoiBrick NewStandB = null; //...and put it into bigger brick...
HanoiTable NewStandT = null; //...or empty table spot.

Expression<Predicate<HanoiObj>> ObjectIsNoUp = (HO => HO.IsEmptyUpSide); //Moved brick have to be empty up side
Expression<Predicate<HanoiBrick, HanoiBrick>> PutSmallBrickAtBigger = ((MB, NSB) => (MB.Size < NSB.Size)); //you can put smaller brick onto bigger one
Expression<Predicate<HanoiBrick, HanoiObj>> FindObjBelongMovd = ((MB, OBM) => (MB.Size == OBM.HanoiObjSizeUpSide));

ActionPDDL moveBrickOnBrick = new ActionPDDL("Move brick onto another brick"); //1st action with 3 parameters: MovedBrick, ObjBelowMoved, NewStandB

moveBrickOnBrick.AddPartOfActionSententia(ref MovedBrick, "Place the {0}-size brick ", MB => MB.Size);
moveBrickOnBrick.AddPartOfActionSententia(ref NewStandB, "onto {0}-size brick.", MB => MB.Size);

moveBrickOnBrick.AddPrecondiction("Moved brick is no up", ref MovedBrick, ObjectIsNoUp); //MovedBrick.IsEmptyUpSide == true
moveBrickOnBrick.AddPrecondiction("New stand is empty", ref NewStandB, ObjectIsNoUp); //NewStandB.IsEmptyUpSide == true
moveBrickOnBrick.AddPrecondiction("Small brick on bigger one", ref MovedBrick, ref NewStandB, PutSmallBrickAtBigger); //MovedBrick.Size < NewStandB.Size
moveBrickOnBrick.AddPrecondiction("Find brick bottom moved one", ref MovedBrick, ref ObjBelowMoved, FindObjBelongMovd); //MovedBrick.Size == ObjBelowMoved.HanoiObjSizeUpSide

moveBrickOnBrick.AddEffect("New stand is full", ref NewStandB, NS => NS.IsEmptyUpSide, false); //NewStandB.IsEmptyUpSide = false
moveBrickOnBrick.AddEffect("Old stand is empty", ref ObjBelowMoved, NS => NS.IsEmptyUpSide, true); //ObjBelowMoved.IsEmptyUpSide = true
moveBrickOnBrick.AddEffect("UnConsociate Objs", ref ObjBelowMoved, OS => OS.HanoiObjSizeUpSide, 0); //ObjBelowMoved.HanoiObjSizeUpSide = 0
moveBrickOnBrick.AddEffect("Consociate Bricks", ref NewStandB, NSB => NSB.HanoiObjSizeUpSide, ref MovedBrick, MB => MB.Size); //NewStandB.HanoiObjSizeUpSide = MovedBrick.Size

newDomein.AddAction(moveBrickOnBrick); //Putting empty brick onto bigger one

ActionPDDL moveBrickOnTable = new ActionPDDL("Move brick on table"); //2st action with 3 parameters: MovedBrick, ObjBelowMoved, NewStandT

moveBrickOnTable.AddPartOfActionSententia(ref MovedBrick, "Place the {0}-size brick ", MB => MB.Size);
moveBrickOnTable.AddPartOfActionSententia(ref NewStandT, "onto table no {0}.", NS => NS.no);

moveBrickOnTable.AddPrecondiction("Moved brick is no up", ref MovedBrick, ObjectIsNoUp); //MovedBrick.IsEmptyUpSide == true
moveBrickOnTable.AddPrecondiction("New table is empty", ref NewStandT, ObjectIsNoUp); //NewStandT.IsEmptyUpSide == true
moveBrickOnTable.AddPrecondiction("Find brick bottom moved one", ref MovedBrick, ref ObjBelowMoved, FindObjBelongMovd); //MovedBrick.Size == ObjBelowMoved.HanoiObjSizeUpSide

moveBrickOnTable.AddEffect("New stand is full", ref NewStandT, NS => NS.IsEmptyUpSide, false); //NewStandT.IsEmptyUpSide = false
moveBrickOnTable.AddEffect("Old stand is empty", ref ObjBelowMoved, NS => NS.IsEmptyUpSide, true); //ObjBelowMoved.IsEmptyUpSide = true
moveBrickOnTable.AddEffect("UnConsociate Objs", ref ObjBelowMoved, OS => OS.HanoiObjSizeUpSide, 0); //ObjBelowMoved.HanoiObjSizeUpSide = 0
moveBrickOnTable.AddEffect("Consociate Bricks", ref NewStandT, NST => NST.HanoiObjSizeUpSide, ref MovedBrick, MB => MB.Size); //NewStandT.HanoiObjSizeUpSide = MovedBrick.Size

newDomein.AddAction(moveBrickOnTable); //Putting empty brick onto empty table spot
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
  <summary>River crossing puzzle</summary>
  
Treatment the puzzle: [wiki](https://en.wikipedia.org/wiki/Wolf,_goat_and_cabbage_problem)

Putting a thing to the boat:
```cs
    ActionPDDL TakingCabbage = new ActionPDDL("TakingCabbage");
    TakingCabbage.AddPartOfActionSententia("Take the cabbage.");
    TakingCabbage.AddPrecondiction("Boat is near the bank", ref nextToBank, b => b.IsBoat);
    TakingCabbage.AddPrecondiction("Cabbage is at the bank", ref nextToBank, b => b.IsCabbage);
    TakingCabbage.AddPrecondiction("Boat is empty", ref boat, b => !b.IsCabbage && !b.IsGoat && !b.IsWolf);
    TakingCabbage.AddEffect("Remove the cabbage from the bank", ref nextToBank, b => b.IsCabbage, false);
    TakingCabbage.AddEffect("Put the cabbage on the boat", ref boat, b => b.IsCabbage, true);
    RiverCrossing.AddAction(TakingCabbage);
```

Putting a thing away:
```cs
    ActionPDDL PutCabbageAway = new ActionPDDL("PuttingCabbageAway");
    PutCabbageAway.AddPartOfActionSententia("Put the cabbage away.");
    PutCabbageAway.AddPrecondiction("Boat is near the bank", ref nextToBank, b => b.IsBoat);
    PutCabbageAway.AddPrecondiction("Goat is on the bank", ref boat, b => b.IsCabbage);
    PutCabbageAway.AddEffect("Remove the goat from the bank", ref nextToBank, b => b.IsCabbage, true);
    PutCabbageAway.AddEffect("Add the goat to the boat", ref boat, b => b.IsCabbage, false);
    RiverCrossing.AddAction(PutCabbageAway);
```

One need to use the above 3 times. For the cabbage, goat and wolf.

Going to the other river bank:
```cs
    ActionPDDL CrossTheRiver = new ActionPDDL("CrossingTheRiver");
    CrossTheRiver.AddPartOfActionSententia("Cross the river.");
    CrossTheRiver.AddPrecondiction("Boat is near the bank", ref nextToBank, b => b.IsBoat);
    CrossTheRiver.AddPrecondiction("Nothing won't be eaten", ref nextToBank, b => b.IsGoat ? (!b.IsCabbage && !b.IsWolf) : true );
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

</details>

<details> 
  <summary>Water pouring puzzle</summary>
  
Treatment the puzzle: [wiki](https://en.wikipedia.org/wiki/Water_pouring_puzzle) 
    
  ```cs
public class WaterJug
{
    public readonly float Capacity;
    public float flood;
    ⁝
}
```    
```cs
DomeinPDDL DecantingDomein = new DomeinPDDL("Decanting problems"); //In this problem...

ActionPDDL DecantWater = new ActionPDDL("Decant water"); //...you need one action with 2 arguments:
WaterJug SourceJug = null; //The jug from which you pour,
WaterJug DestinationJug = null; // and the jug you pour into.

DecantWater.AddPartOfActionSententia(ref SourceJug, "from {0}-liter jug ", SJ => SJ.Capacity);
DecantWater.AddPartOfActionSententia(ref DestinationJug, "to the {0}-liter jug.", DJ => DJ.Capacity);

//In the effect of decanting the level in the jug from which you pour is maked smaller after that,...
DecantWater.AddEffect( //SourceJug.flood = DestinationJug.flood + SourceJug.flood >= DestinationJug.Capacity ? SourceJug.flood - DestinationJug.Capacity + DestinationJug.flood : 0
    "Reduce source jug flood",
    ref SourceJug,
    Source_Jug => Source_Jug.flood,
    ref DestinationJug,
    (Source_Jug, Destination_Jug) => Destination_Jug.flood + Source_Jug.flood >= Destination_Jug.Capacity ? Source_Jug.flood - Destination_Jug.Capacity + Destination_Jug.flood : 0);

//...the level in the jug you pour into is maked bigger.
DecantWater.AddEffect( //DestinationJug.flood = DestinationJug.flood + SourceJug.flood >= DestinationJug.Capacity ? DestinationJug.Capacity : DestinationJug.flood + SourceJug.flood
    "Increase destination jug flood",
    ref DestinationJug,
    Destination_Jug => Destination_Jug.flood,
    ref SourceJug,
    (Destination_Jug, Source_Jug) => Destination_Jug.flood + Source_Jug.flood >= Destination_Jug.Capacity ? Destination_Jug.Capacity : Destination_Jug.flood + Source_Jug.flood);

//One need to do as fast as possible
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

<details> 
  <summary>Travelling salesman problem</summary>
   
Treatment the problem: [wiki](https://en.wikipedia.org/wiki/Travelling_salesman_problem)

Define the action:
```cs
ActionPDDL Travel = new ActionPDDL("Travel");
City From = null; //Salesman leaves "From" city,
City To = null; //and goes to "To" city.

Travel.AddPartOfActionSententia(ref To, "Go to {0}.", T => T.Name);

Travel.AddPrecondiction( // From.SalesmanHere == true
    "Salesnam is in FROM city now",
    ref From,
    F => F.SalesmanHere);

//Salesman visit city only one time
Travel.AddPrecondiction( // To.Visiting == false
    "Salesnam havent been in TO city",
    ref To,
    F => !F.Visited);

Travel.AddEffect( // From.SalesmanHere = false
    "Salesman leaves city",
    ref From,
    F => F.SalesmanHere,
    false);

Travel.AddEffect( // To.SalesmanHere = true
    "Salesman arrives new city",
    ref To,
    T => T.SalesmanHere,
    true);

Travel.AddEffect( // To.Visited = true
    "Salesman visit new city",
    ref To,
    T => T.Visited,
    true);

Travel.DefineActionCost(ref From, ref To, (F, T) => CitiesAPI.DistanceAPI(F.PostalCode, T.PostalCode));
```
Some DistanceMatrix / Travel action cost:

| Distance | Koszalin | Gniezno | Kraków | Płock | Poznań | Warszawa | Lublin |
| :---     | :---:    | :---:   | :---:  | :---: | :---:  | :---:    | :---:  |
| Koszalin | 0        | 245     | 700    | 372   | 250    | 520      | 687    |
| Gniezno  | 245      | 0       | 456    | 165   | 48     | 293      | 448    |
| Kraków   | 700      | 456     | 0      | 364   | 458    | 290      | 304    |
| Płock    | 372      | 165     | 364    | 0     | 227    | 109      | 295    |
| Poznań   | 250      | 48      | 458    | 227   | 0      | 311      | 478    |
| Warszawa | 520      | 293     | 290    | 109   | 311    | 0        | 173    |
| Lublin   | 687      | 448     | 304    | 295   | 478    | 173      | 0      |

```
SharpPDDL : Visit all cities determined!!! Total Cost: 1806
Travel: Go to Gniezno. Action cost: 245
Travel: Go to Poznan. Action cost: 48
Travel: Go to Plock. Action cost: 227
Travel: Go to Warszawa. Action cost: 109
Travel: Go to Lublin. Action cost: 173
Travel: Go to Kraków. Action cost: 304
Travel: Go to Koszalin. Action cost: 700
```

Make you sure about the solution with another program: [AtoZmath.com](https://cbom.atozmath.com/CBOM/Assignment.aspx?q=tsnn&q1=0%2C245%2C700%2C372%2C250%2C520%2C687%3B245%2C0%2C456%2C165%2C48%2C293%2C448%3B700%2C456%2C0%2C364%2C458%2C290%2C304%3B372%2C165%2C364%2C0%2C227%2C109%2C295%3B250%2C48%2C458%2C227%2C0%2C311%2C478%3B520%2C293%2C290%2C109%2C311%2C0%2C173%3B687%2C448%2C304%2C295%2C478%2C173%2C0%60MIN%60Koszalin%2CGniezno%2CKrak%C3%B3w%2CP%C5%82ock%2CPozna%C5%84%2CWarszawa%2CLublin%60Koszalin%2CGniezno%2CKrak%C3%B3w%2CP%C5%82ock%2CPozna%C5%84%2CWarszawa%2CLublin%60false%60false&do=1#tblSolution)

</details>

---
<img align="right" src="https://github.com/user-attachments/assets/85f24e2f-18b7-417f-bd34-4fef48890ee2">

License: [Creative Commons Attribution-NonCommercial-ShareAlike4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode)
