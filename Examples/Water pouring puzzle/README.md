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
DomainPDDL DecantingDomain = new DomainPDDL("Decanting problems"); //In this problem...

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

DecantingDomain.AddAction(DecantWater);
```
![Water_pouring_solution](https://github.com/user-attachments/assets/3e35f26a-d4fe-46c9-a1e2-c4bba66b5225)