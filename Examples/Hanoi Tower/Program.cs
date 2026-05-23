/*
 * Treatment the puzzle: https://en.wikipedia.org/wiki/Tower_of_Hanoi
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;

using SharpPDDL;

namespace Hanoi_Tower
{
    class Program
    {
        public class HanoiObj //It cannot be abstract
        {
            public HanoiBrick HanoiBrickUpSide = null;
        }

        public class HanoiBrick : HanoiObj
        {
            readonly public int Size;

            public HanoiBrick(int size)
            {
                this.Size = size;
            }
        }

        public class HanoiTable : HanoiObj
        {
            public readonly int no;
            public HanoiTable(int no, HanoiBrick HanoiBrickUpSide = null)
            {
                this.no = no;
                this.HanoiBrickUpSide = HanoiBrickUpSide;
            }
        }

        static void Main(string[] args)
        {
            Stopwatch Stopwatch = new Stopwatch();

            void PrintPlan(List<List<string>> plan)
            {
                Stopwatch.Stop();

                for (int i = 0; i != plan.Count; i++)
                    Console.WriteLine(plan[i][0] + plan[i][1]);

                Console.WriteLine("Stopwatch result: " + Stopwatch.Elapsed);
            }

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            DomainPDDL newDomein = new DomainPDDL("Hanoi");

            Stopwatch.Start();

            //Add a tracing of library
            var Ts = new TraceSource("Tracing");
            Ts.Switch.Level = SourceLevels.All;
            Ts.Listeners.Add(new TextWriterTraceListener(Console.Out));
            //newDomein.DefineTrace(Ts);

            HanoiBrick MovedBrick = null; //you can take brick...
            HanoiObj ObjBelowMoved = null; //...from table or another brick... 
            HanoiBrick NewStandB = null; //...and put it into bigger brick...
            HanoiTable NewStandT = null; //...or empty table spot.

            Expression<Predicate<HanoiObj>> ObjectIsNoUp = (HO => HO.HanoiBrickUpSide == null); //Moved brick have to be empty up side
            Expression<Predicate<HanoiBrick, HanoiBrick>> PutSmallBrickAtBigger = ((MB, NSB) => (MB.Size < NSB.Size)); //you can put smaller brick onto bigger one
            Expression<Predicate<HanoiBrick, HanoiObj>> FindObjBelongMovd = ((MB, OBM) => (MB == OBM.HanoiBrickUpSide)); //the MB is directly above the OMB

            ActionPDDL moveBrickOnBrick = new ActionPDDL("Move brick onto another brick"); //1st action with 3 parameters: MovedBrick, ObjBelowMoved, NewStandB

            moveBrickOnBrick.AddPartOfActionSententia(ref MovedBrick, "Place the {0}-size brick ", MB => MB.Size);
            moveBrickOnBrick.AddPartOfActionSententia(ref NewStandB, "onto {0}-size brick.", MB => MB.Size);

            moveBrickOnBrick.AddPrecondition("Moved brick is no up", ref MovedBrick, ObjectIsNoUp); //MovedBrick.HanoiBrickUpSide == null
            moveBrickOnBrick.AddPrecondition("New stand is empty", ref NewStandB, ObjectIsNoUp); //NewStandB.HanoiBrickUpSide == null
            moveBrickOnBrick.AddPrecondition("Small brick on bigger one", ref MovedBrick, ref NewStandB, PutSmallBrickAtBigger); //MovedBrick.Size < NewStandB.Size
            moveBrickOnBrick.AddPrecondition("Find brick bottom moved one", ref MovedBrick, ref ObjBelowMoved, FindObjBelongMovd); //MovedBrick == ObjBelowMoved.HanoiBrickUpSide

            moveBrickOnBrick.AddEffect("Old stand is empty", ref ObjBelowMoved, NS => NS.HanoiBrickUpSide, null); //ObjBelowMoved.HanoiBrickUpSide = null
            moveBrickOnBrick.AddEffect("Consociate Bricks", ref NewStandB, NSB => NSB.HanoiBrickUpSide, ref MovedBrick); //NewStandB.HanoiBrickUpSide = MovedBrick

            newDomein.AddAction(moveBrickOnBrick); //Putting empty brick onto bigger one

            ActionPDDL moveBrickOnTable = new ActionPDDL("Move brick on table"); //2st action with 3 parameters: MovedBrick, ObjBelowMoved, NewStandT

            moveBrickOnTable.AddPartOfActionSententia(ref MovedBrick, "Place the {0}-size brick ", MB => MB.Size);
            moveBrickOnTable.AddPartOfActionSententia(ref NewStandT, "onto table no {0}.", NS => NS.no);

            moveBrickOnTable.AddPrecondition("Moved brick is no up", ref MovedBrick, ObjectIsNoUp); //MovedBrick.ObjectIsNoUp == null
            moveBrickOnTable.AddPrecondition("New table is empty", ref NewStandT, ObjectIsNoUp); //NewStandT.ObjectIsNoUp == null
            moveBrickOnTable.AddPrecondition("Find brick bottom moved one", ref MovedBrick, ref ObjBelowMoved, FindObjBelongMovd); //MovedBrick == ObjBelowMoved.HanoiBrickUpSide

            moveBrickOnTable.AddEffect("Old stand is empty", ref ObjBelowMoved, NS => NS.HanoiBrickUpSide, null); //ObjBelowMoved.HanoiBrickUpSide = null
            moveBrickOnTable.AddEffect("Consociate Bricks", ref NewStandT, NST => NST.HanoiBrickUpSide, ref MovedBrick); //NewStandT.HanoiBrickUpSide = MovedBrick

            newDomein.AddAction(moveBrickOnTable); //Putting empty brick onto empty table spot

            int MaxBriSize = 8; //size of the biggest brick
            HanoiBrick prev = null;
            for (int Bri = 1; Bri <= MaxBriSize; Bri++)
            {
                HanoiBrick newOne = new HanoiBrick(Bri) //Make new hanoi brick
                {
                    HanoiBrickUpSide = prev //Put the smaller on the new one
                };

                prev = newOne; //uptate the smaller one for next iteration

                newDomein.domainObjects.Add(newOne); //add it (the new one) to the domain objects
            }

            List<HanoiTable> HanoiTables = new List<HanoiTable> { new HanoiTable(0, prev), new HanoiTable(1), new HanoiTable(2) }; //make tables, and put bricks on 1st one

            //add tables to the domain objects
            foreach (var HT in HanoiTables)
                newDomein.domainObjects.Add(HT);

            GoalPDDL movedBrick = new GoalPDDL("Transfer bricks onto table no. 3"); //define domain goal as...
            movedBrick.AddExpectedObjectState( HanoiTables[0], HT => HT.HanoiBrickUpSide == null); //...0th table - empty...
            movedBrick.AddExpectedObjectState( HanoiTables[1], HT => HT.HanoiBrickUpSide == null); //...1st table - empty...
            newDomein.AddGoal(movedBrick); //...and add it to damain goal

            newDomein.PlanGenerated += PrintPlan; //Print the plan of solution when it be found
            CancellationTokenSource ExternalcancellationTokenSource = new CancellationTokenSource();
            newDomein.GenerateDiagrams("Hanoi", Diagram.Class | Diagram.States); //create diagrams while run
            newDomein.Start(null, ExternalcancellationTokenSource.Token); //Run it all!

            Console.ReadKey();
            ExternalcancellationTokenSource.Cancel();
            Trace.Close();
        }
    }
}
