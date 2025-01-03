/*
 * Treatment the puzzle: https://en.wikipedia.org/wiki/Water_pouring_puzzle#Standard_example 
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPDDL;

namespace Water_pouring_puzzle
{
    class Program
    {
        public class WaterJug
        {
            //Define Action cost function as quantify of water to decant
            public static int DecantedWater (float SourceFlood, float DestinationCapacity, float DestinationFlood)
            {
                if (SourceFlood + DestinationFlood > DestinationCapacity)
                    return (int)(DestinationCapacity - DestinationFlood);
                else
                    return (int)SourceFlood;
            }

            public void WaitForDecant (WaterJug DestinationWaterJug)
            {
                int time = DecantedWater(this.flood, DestinationWaterJug.Capacity, DestinationWaterJug.flood);
                Task.Delay(time * 2000);
            }

            public readonly float Capacity;
            private float _flood;
            public float flood
            {
                get { return _flood; }
                set
                {
                    if (value >= 0 && value <= Capacity)
                        _flood = value;
                }
            }

            public WaterJug(float Capacity, float flood = 0)
            {
                this.Capacity = Capacity;
                this.flood = flood;
            }
        }

        static void PrintPlan(List<List<string>> plan)
        {
            for (int i = 0; i != plan.Count; i++)
                Console.WriteLine(plan[i][0] + plan[i][1] + plan[i][2]);
        }

        static void Main(string[] args)
        {
            DomeinPDDL DecantingDomein = new DomeinPDDL("Decanting problems");

            ActionPDDL DecantWater = new ActionPDDL("Decant water");
            WaterJug SourceJug = null;
            WaterJug DestinationJug = null;

            DecantWater.AddAssignedParametr(ref SourceJug, "from {0}-liter jug ", SJ => SJ.Capacity);
            DecantWater.AddAssignedParametr(ref DestinationJug, "to the {0}-liter jug.", DJ => DJ.Capacity);

            DecantWater.AddEffect(
                "Reduce source jug flood", 
                ref SourceJug, 
                Source_Jug => Source_Jug.flood,
                ref DestinationJug,
                (Source_Jug, Destination_Jug) => Destination_Jug.flood + Source_Jug.flood >= Destination_Jug.Capacity ? Source_Jug.flood - Destination_Jug.Capacity + Destination_Jug.flood : 0)
                .UseAsExecution();

            DecantWater.AddEffect(
                "Increase destination jug flood",
                ref DestinationJug,
                Destination_Jug => Destination_Jug.flood,
                ref SourceJug,
                (Destination_Jug, Source_Jug) => Destination_Jug.flood + Source_Jug.flood >= Destination_Jug.Capacity ? Destination_Jug.Capacity : Destination_Jug.flood + Source_Jug.flood)
                .UseAsExecution();

            DecantWater.AddExecution("Wait for decantation", ref SourceJug, ref DestinationJug, (Source_Jug, Destination_Jug) => Source_Jug.WaitForDecant(Destination_Jug), false);
            DecantWater.AddExecution("Let me know", () => Console.WriteLine("Decanted"), true);

            DecantWater.DefineActionCost(ref SourceJug, ref DestinationJug, (S, D) => WaterJug.DecantedWater(S.flood, D.Capacity, D.flood));

            DecantingDomein.AddAction(DecantWater);

            WaterJug waterJug8 = new WaterJug(8, 8);
            WaterJug waterJug5 = new WaterJug(5, 0);
            WaterJug waterJug3 = new WaterJug(3, 0);

            DecantingDomein.domainObjects.Add(waterJug8);
            DecantingDomein.domainObjects.Add(waterJug5);
            DecantingDomein.domainObjects.Add(waterJug3);

            GoalPDDL Halve = new GoalPDDL("Divide in half");
            Halve.AddExpectedObjectState(waterJug8, Water_Jug => Water_Jug.flood == 4);
            Halve.AddExpectedObjectState(waterJug5, Water_Jug => Water_Jug.flood == 4);
            DecantingDomein.AddGoal(Halve);

            DecantingDomein.PlanGenerated += PrintPlan;
            DecantingDomein.Start();
            Console.ReadKey();
        }
    }
}
