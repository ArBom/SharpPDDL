Treatment the problem: [wiki](https://en.wikipedia.org/wiki/Travelling_salesman_problem)

Define the action:
```cs
ActionPDDL Travel = new ActionPDDL("Travel");
City From = null; //Salesman leaves "From" city,
City To = null; //and goes to "To" city.

Travel.AddPartOfActionSententia(ref To, "Go to {0}.", T => T.Name);

Travel.AddPrecondition( // From.SalesmanHere == true
    "Salesnam is in FROM city now",
    ref From,
    F => F.SalesmanHere);

//Salesman visit city only one time
Travel.AddPrecondition( // To.Visiting == false
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

You can make you sure about the solution with another program: [AtoZmath.com](https://cbom.atozmath.com/CBOM/Assignment.aspx?q=tsnn&q1=0%2C245%2C700%2C372%2C250%2C520%2C687%3B245%2C0%2C456%2C165%2C48%2C293%2C448%3B700%2C456%2C0%2C364%2C458%2C290%2C304%3B372%2C165%2C364%2C0%2C227%2C109%2C295%3B250%2C48%2C458%2C227%2C0%2C311%2C478%3B520%2C293%2C290%2C109%2C311%2C0%2C173%3B687%2C448%2C304%2C295%2C478%2C173%2C0%60MIN%60Koszalin%2CGniezno%2CKrak%C3%B3w%2CP%C5%82ock%2CPozna%C5%84%2CWarszawa%2CLublin%60Koszalin%2CGniezno%2CKrak%C3%B3w%2CP%C5%82ock%2CPozna%C5%84%2CWarszawa%2CLublin%60false%60false&do=1#tblSolution)