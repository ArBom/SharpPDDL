## Examples of popular games and puzzles where SharpPDDL could be used to find a solution:

### 7 Bridges of Königsberg:
* detection of problem that cannot be solved and generates finite possible state count

### [15 puzzles:](https://github.com/ArBom/SharpPDDL/tree/master/Examples/15%20puzzle)
* queue of goals (goals are added one-by-one, after reach the previous one)
* added implementation of the execution of solution

### [Peg solitaire:](https://github.com/ArBom/SharpPDDL/tree/master/Examples/Peg%20solitaire)
* added implementation of the execution of solution

### [River crossing puzzle:](https://github.com/ArBom/SharpPDDL/tree/master/Examples/River%20crossing%20puzzle)
* a few classes with different parents in domain
* goal defined as non-null member of class instanties
* generate diagrams:
    * class diagram
    * case use diagram
    * state diagram

### [Tower of Hanoi:](https://github.com/ArBom/SharpPDDL/tree/master/Examples/Hanoi%20Tower)
* a few classes with mutual parent in domain
* goal defined as null member of classes instanties
* generate diagrams:
    * class diagram
    * state diagram

### [Travelling salesman problem:](https://github.com/ArBom/SharpPDDL/tree/master/Examples/Travelling%20Salesman%20Problem)
* finding the solution with cost optimization

### [Water pouring puzzle:](https://github.com/ArBom/SharpPDDL/tree/master/Examples/Water%20pouring%20puzzle)
* example of use action without precondition
* use EventWaitHandle to accept every action do while execution
* added implementation of the execution of solution
* generate diagram:
    * case use diagram
