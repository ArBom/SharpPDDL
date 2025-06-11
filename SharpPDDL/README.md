This is the class library based on PDDL intellection and in effect it's a implementation of GOAP (Goal Oriented Action Planning) algorithm. Algorithm was made as decision-meker and optimalizer for IIoT (as part of MES) small environment, but it could be use as other problem solver too. It uses only C# 7.2 standard library. Values inside classes using to find solution have to be ValueType only (most numeric, like: int, short etc., char, bool). One can to use previously defined classes which are using in other part of one's programm. At this version you can use single instance of algorithm to solve single problem.

> **WARNING** 
> Library has some bugs. It's beta version still.

## How to use:
Include the library namespace with `using SharpPDDL`.

| Method | What is it doing? |
|---|---|
| `new DomeinPDDL()` | Creates the instance of algorithm. |
| DomeinPDDL.`AddAction()` | Adds action to domain. |
| DomeinPDDL.`domainObjects` | Objects' collection manned by library. |
| DomeinPDDL.`AddGoal()` | Adds goal to do. |
| DomeinPDDL.`DefineTrace()` | Defines TraceSource to do trace the code execution. |
| DomeinPDDL.`planGenerated` | delegate of List<List<string>> type. It shows a generated plan. |
| DomeinPDDL.`SetExecutionOptions()` | Defines options of plan realization |
| DomeinPDDL.`Start()` | Starts the algorithm. |
| `new ActionPDDL()` | Creates the action to use in domein. |
| ActionPDDL.`AddPrecondiction()` | Adds precondition of action doing. |
| ActionPDDL.`AddEffect()` | Adds effect of action doing. |
| ActionPDDL.`DefineActionCost()` | Defines action cost. |
| ActionPDDL.`AddPartOfActionSententia()` | Adds description of action in generated plan. |
| ActionPDDL.`AddExecution()` | Adds action execution of algorithm realization. |
| `new GoalPDDL()` | Creates the goal of algorithm run. |
| GoalPDDL.`AddExpectedObjectState()` | Defines a state of one of obj. manned by library as alg. goal. |

### Possible applications:

> **TIP**
> First use of it could seems a little unintuitive. Get familiar with ready examples.

* [Tower of Hanoi](https://github.com/ArBom/SharpPDDL/blob/master/Hanoi%20Tower/Program.cs)
* [River crossing puzzle](https://github.com/ArBom/SharpPDDL/blob/master/River%20crossing%20puzzle/Program.cs)
* [Water pouring puzzle](https://github.com/ArBom/SharpPDDL/blob/master/Water%20pouring%20puzzle/Program.cs)
* [Travelling salesman problem](https://github.com/ArBom/SharpPDDL/blob/master/Travelling%20Salesman%20Problem/Program.cs)

---
License: [Creative Commons Attribution-NonCommercial-ShareAlike4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode)