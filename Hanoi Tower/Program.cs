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
            public int HanoiObjSizeUpSide = 0;
            public bool IsEmptyUpSide;
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
            public HanoiTable(int no, int HanoiObjSizeUpSide = 0, bool isEmpty = true)
            {
                this.no = no;
                this.HanoiObjSizeUpSide = HanoiObjSizeUpSide;
                this.IsEmptyUpSide = isEmpty;
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
            DomeinPDDL newDomein = new DomeinPDDL("Hanoi");

            Stopwatch.Start();

            newDomein.DefineTrace(new TraceSwitch("Default", "default")
            {
                Level = TraceLevel.Off
            });

            HanoiBrick MovedBrick = null;
            HanoiObj ObjBelowMoved = null;
            HanoiBrick NewStandB = null;
            HanoiTable NewStandT = null;

            Expression<Predicate<HanoiObj>> ObjectIsNoUp = (HO => HO.IsEmptyUpSide);
            Expression<Predicate<HanoiBrick, HanoiBrick>> PutSmallBrickAtBigger = ((MB, NSB) => (MB.Size < NSB.Size));
            Expression<Predicate<HanoiBrick, HanoiObj>> FindObjBelongMovd = ((MB, OBM) => (MB.Size == OBM.HanoiObjSizeUpSide));

            ActionPDDL moveBrickOnBrick = new ActionPDDL("Move brick onto another brick");

            moveBrickOnBrick.AddPartOfActionSententia(ref MovedBrick, "Place the {0}-size brick ", MB => MB.Size);
            moveBrickOnBrick.AddPartOfActionSententia(ref NewStandB, "onto {0}-size brick.", MB => MB.Size);

            moveBrickOnBrick.AddPrecondiction("Moved brick is no up", ref MovedBrick, ObjectIsNoUp);
            moveBrickOnBrick.AddPrecondiction("New stand is empty", ref NewStandB, ObjectIsNoUp);
            moveBrickOnBrick.AddPrecondiction("Small brick on bigger one", ref MovedBrick, ref NewStandB, PutSmallBrickAtBigger);
            moveBrickOnBrick.AddPrecondiction("Find brick bottom moved one", ref MovedBrick, ref ObjBelowMoved, FindObjBelongMovd);

            moveBrickOnBrick.AddEffect("New stand is full", ref NewStandB, NS => NS.IsEmptyUpSide, false);
            moveBrickOnBrick.AddEffect("Old stand is empty", ref ObjBelowMoved, NS => NS.IsEmptyUpSide, true);
            moveBrickOnBrick.AddEffect("UnConsociate Objs", ref ObjBelowMoved, OS => OS.HanoiObjSizeUpSide, 0);
            moveBrickOnBrick.AddEffect("Consociate Bricks", ref NewStandB, NSB => NSB.HanoiObjSizeUpSide, ref MovedBrick, MB => MB.Size);

            newDomein.AddAction(moveBrickOnBrick);

            ActionPDDL moveBrickOnTable = new ActionPDDL("Move brick on table");

            moveBrickOnTable.AddPartOfActionSententia(ref MovedBrick, "Place the {0}-size brick ", MB => MB.Size);
            moveBrickOnTable.AddPartOfActionSententia(ref NewStandT, "onto table no {0}.", NS => NS.no);

            moveBrickOnTable.AddPrecondiction("Moved brick is no up", ref MovedBrick, ObjectIsNoUp);
            moveBrickOnTable.AddPrecondiction("New table is empty", ref NewStandT, ObjectIsNoUp);
            moveBrickOnTable.AddPrecondiction("Find brick bottom moved one", ref MovedBrick, ref ObjBelowMoved, FindObjBelongMovd);

            moveBrickOnTable.AddEffect("New stand is full", ref NewStandT, NS => NS.IsEmptyUpSide, false);
            moveBrickOnTable.AddEffect("Old stand is empty", ref ObjBelowMoved, NS => NS.IsEmptyUpSide, true);
            moveBrickOnTable.AddEffect("UnConsociate Objs", ref ObjBelowMoved, OS => OS.HanoiObjSizeUpSide, 0);
            moveBrickOnTable.AddEffect("Consociate Bricks", ref NewStandT, NST => NST.HanoiObjSizeUpSide, ref MovedBrick, MB => MB.Size);

            newDomein.AddAction(moveBrickOnTable);

            int MaxBriSize = 8;
            for (int Bri = 1; Bri <= MaxBriSize; Bri++)
            {
                HanoiBrick newOne = new HanoiBrick(Bri);
                if (Bri != 1)
                    newOne.HanoiObjSizeUpSide = Bri - 1;
                else
                    newOne.IsEmptyUpSide = true;

                newDomein.domainObjects.Add(newOne);
            }

            List<HanoiTable> HanoiTables = new List<HanoiTable> { new HanoiTable(0, MaxBriSize, false), new HanoiTable(1), new HanoiTable(2) };

            foreach (var HT in HanoiTables)
                newDomein.domainObjects.Add(HT);

            GoalPDDL movedBrick = new GoalPDDL("Transfer bricks onto table no. 3");
            movedBrick.AddExpectedObjectState( HanoiTables[0], HT => HT.IsEmptyUpSide );
            movedBrick.AddExpectedObjectState( HanoiTables[1], HT => HT.IsEmptyUpSide );
            newDomein.AddGoal(movedBrick);

            newDomein.PlanGenerated += PrintPlan;
            CancellationTokenSource ExternalcancellationTokenSource = new CancellationTokenSource();
            newDomein.Start(null, ExternalcancellationTokenSource.Token);

            Console.ReadKey();
            ExternalcancellationTokenSource.Cancel();

            int AO = 1500;
            Trace.Close();
        }
    }
}
