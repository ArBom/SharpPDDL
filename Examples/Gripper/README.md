
# Gripper

## Treatment the problem
This problem is a simulation of work of gripper, which is moving 2 types of objects. The goal of gripper is sort objects by a colour of them. The begin and the finish state are shown on the pictures below.

![Diff](https://github.com/user-attachments/assets/b13986ae-0eca-4385-91b9-51459fdb46d8)

## Rules of this environment:
1. You can put a ball or a box on a table.
2. Inside the box can be one ball.
3. You cannot put the box on another box or the ball.
4. Inside a gripper can be one object (the box or the ball) in one time.
5. Gripper can to take in / out the ball from / to the box

There are pictures of 2 possible states of conditions taking the rules into account

![State1](https://github.com/user-attachments/assets/d27a76c8-90c9-4495-8cc9-449433a4fe24)
![State2](https://github.com/user-attachments/assets/92460c80-1251-4535-badb-2168280244f0)
## Defining relationships between objects in the objective description

Relation of objects in this domain is shown on the class diagram.

![Diagram](https://github.com/user-attachments/assets/0e563fe4-e778-4082-8ed9-72972d79b9f3)

There is not a "Colour" - the member of TechniColObj on the diagram because it's not in use in any action.

```cs
class TechniColObj : EnvObj
{
    public readonly int Colour;

    public TechniColObj (ConsoleColor colour)
    {
        this.Colour = (int)colour;
    }
}
```

The goal definition of table state goal is refer to the color of it and a color of object putted on it.

```cs
goalPDDL.AddExpectedObjectState(Table_R, t => t.Colour == t.Content.Colour);
```
