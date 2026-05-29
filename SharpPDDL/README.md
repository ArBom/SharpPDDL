This is the class library based on PDDL intellection and in effect it's a implementation of GOAP (Goal Oriented Action Planning) algorithm. Algorithm was made as decision-meker and optimalizer for IIoT (as part of MES) small environment, but it could be use as other problem solver too. It uses only C# 7.2 standard library. Values inside classes using to find solution have to be ValueType only (most numeric, like: int, short etc., char, bool), or be the another class added to domain problem. One can to use previously defined classes which are using in other part of one's programm. At this version you can use single instance of algorithm to solve single problem.

## How to use:
Include the library namespace with `using SharpPDDL`.

| Method | What is it doing? |
|---|---|
| `new DomainPDDL()` | Creates the instance of algorithm. |
| DomainPDDL.`AddAction()` | Adds action to domain. |
| DomainPDDL.`domainObjects` | Objects' collection manned by library. |
| DomainPDDL.`AddGoal()` | Adds goal to do. |
| DomainPDDL.`DefineTrace()` | Defines TraceSource to do trace the code execution. |
| DomainPDDL.`planGenerated` | delegate of List<List<string>> type. It shows a generated plan. |
| DomainPDDL.`SetExecutionOptions()` | Defines options of plan realization |
| DomainPDDL.`GenerateDiagrams()` | Types of diagram to generate and path of saving them |
| DomainPDDL.`Start()` | Starts the algorithm. |
| `new ActionPDDL()` | Creates the action to use in domein. |
| ActionPDDL.`AddPrecondition()` | Adds precondition of action doing. |
| ActionPDDL.`AddEffect()` | Adds effect of action doing. |
| ActionPDDL.`DefineActionCost()` | Defines action cost. |
| ActionPDDL.`AddPartOfActionSententia()` | Adds description of action in generated plan. |
| ActionPDDL.`AddExecution()` | Adds action execution of algorithm realization. |
| `new GoalPDDL()` | Creates the goal of algorithm run. |
| GoalPDDL.`AddExpectedObjectState()` | Defines a state of one of obj. manned by library as alg. goal. |

### Possible applications:

> **TIP**
> First time of use of it would seem to be a little unintuitive. Get familiar with ready examples.

* [15 puzzle](https://github.com/ArBom/SharpPDDL/tree/ce383af1c5fae2b43e919244990bbfa8100dfbc8/Examples/15%20puzzle)
* [River crossing puzzle](https://github.com/ArBom/SharpPDDL/tree/ce383af1c5fae2b43e919244990bbfa8100dfbc8/Examples/River%20crossing%20puzzle)
* [Tower of Hanoi](https://github.com/ArBom/SharpPDDL/tree/ce383af1c5fae2b43e919244990bbfa8100dfbc8/Examples/Hanoi%20Tower)
* [Travelling salesman problem](https://github.com/ArBom/SharpPDDL/tree/ce383af1c5fae2b43e919244990bbfa8100dfbc8/Examples/Travelling%20Salesman%20Problem)
* [Triangular peg solitaire](https://github.com/ArBom/SharpPDDL/tree/ce383af1c5fae2b43e919244990bbfa8100dfbc8/Examples/Peg%20solitaire)
* [Water pouring puzzle](https://github.com/ArBom/SharpPDDL/tree/ce383af1c5fae2b43e919244990bbfa8100dfbc8/Examples/Water%20pouring%20puzzle)

---
License: [Creative Commons Attribution-NonCommercial-ShareAlike4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode)