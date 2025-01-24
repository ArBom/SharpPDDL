/*
 * Treatment the puzzle: https://en.wikipedia.org/wiki/Water_pouring_puzzle#Standard_example 
 */

using System;
using System.Collections.Generic;
using System.Threading;
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
                Thread.Sleep(time * 1000);
            }

            public readonly float Capacity; //max level of fluid
            private float _flood;
            public float flood //current level of fluid
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
            DomeinPDDL DecantingDomein = new DomeinPDDL("Decanting problems"); //In this problem...

            ActionPDDL DecantWater = new ActionPDDL("Decant water"); //...you need one action with 2 parameters:
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

            //Decanting needs time to realize
            DecantWater.AddExecution("Wait for decantation", ref SourceJug, ref DestinationJug, (Source_Jug, Destination_Jug) => Source_Jug.WaitForDecant(Destination_Jug), false);
            DecantWater.UseEffectAlsoAsExecution("Reduce source jug flood"); //assign new value of SourceJug in the same way as effect funct
            DecantWater.UseEffectAlsoAsExecution("Increase destination jug flood"); //assign new value of DestinatioJug in the same way as effect funct
            DecantWater.AddExecution("Let me know", () => Console.WriteLine("Decanted"), true);

            //One need to do as fast as possible
            DecantWater.DefineActionCost(ref SourceJug, ref DestinationJug, (S, D) => WaterJug.DecantedWater(S.flood, D.Capacity, D.flood));

            DecantingDomein.AddAction(DecantWater);

            //Don't ask for agree in time of plan execution
            DecantingDomein.SetExecutionOptions(null, null, AskToAgree.GO_AHEAD);

            //In the begin you have 3 jug of water.
            WaterJug waterJug8 = new WaterJug(8, 8); //8-litre jug is full,
            WaterJug waterJug5 = new WaterJug(5, 0); //5-litre one is empty,
            WaterJug waterJug3 = new WaterJug(3, 0); //3-litre one is empty.

            DecantingDomein.domainObjects.Add(waterJug8);
            DecantingDomein.domainObjects.Add(waterJug5);
            DecantingDomein.domainObjects.Add(waterJug3);

            GoalPDDL Halve = new GoalPDDL("Divide in half"); //You need to...
            Halve.AddExpectedObjectState(waterJug8, Water_Jug => Water_Jug.flood == 4); //...4-litre level inside 8-litre jug,
            Halve.AddExpectedObjectState(waterJug5, Water_Jug => Water_Jug.flood == 4); //and 4-litre level inside 5-litre jug.
            DecantingDomein.AddGoal(Halve);

            DecantingDomein.PlanGenerated += PrintPlan;
            DecantingDomein.Start();
            Console.ReadKey();
        }
    }
}
