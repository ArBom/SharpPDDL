/*
 * Treatment the problem: https://en.wikipedia.org/wiki/Seven_Bridges_of_Königsberg
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpPDDL;

namespace _7_Bridges_of_Königsberg
{
    class Program
    {
        public class Citizen
        {
            public Area location = null;
        }

        public class Area
        {
            public readonly char Name;

            public Area (char Name)
            {
                this.Name = Name;
            }
        }

        public class Bridge
        {
            public uint Nr;
            public readonly Area NE;
            public readonly Area SW;

            public bool used = false;

            public Bridge (uint Nr, Area NE, Area SW)
            {
                this.Nr = Nr;
                this.NE = NE;
                this.SW = SW;
            }
        }

        static void Impossible(List<List<string>> plan)
        {
            Console.WriteLine("Get ready Abel Prize for me ;)");
        }

        public class NoWayFilter : TraceFilter
        {
            override public bool ShouldTrace(TraceEventCache cache, string source,
                TraceEventType eventType, int id, string formatOrMessage,
                object[] args, object data, object[] dataArray)
            {
                return 
                    id == 140 ||
                    id == 142;
            }
        }

        static void Main(string[] args)
        {
            DomainPDDL Konigsberg = new DomainPDDL("Königsberg");

            //Make area of strart
            Citizen citizen0 = null;
            Area area0 = null;

            ActionPDDL StartAt = new ActionPDDL("Set startpoint");
            StartAt.AddPrecondition("Citizen is outside now", ref citizen0, c => c.location == null);
            StartAt.AddEffect("Set citizen at area", ref citizen0, c => c.location, ref area0);
            Konigsberg.AddAction(StartAt);       

            //Go North or East
            Citizen citizen1 = null;
            Area area1_S = null;
            Area area1_F = null;
            Bridge bridge1 = null;

            ActionPDDL Go_NE = new ActionPDDL("Go N or E");
            Go_NE.AddPrecondition<Citizen, Citizen, Area, Area>("Citizen is SW", ref citizen1, ref area1_S, (c, a) => c.location == a);
            Go_NE.AddPrecondition("Bridge is unused yet", ref bridge1, b => b.used == false);
            Go_NE.AddPrecondition<Bridge, Bridge, Area, Area>("Start brigde is SW", ref bridge1, ref area1_S, (b, a) => b.SW == a);
            Go_NE.AddPrecondition<Bridge, Bridge, Area, Area>("Finish bridge is NE", ref bridge1, ref area1_F, (b, a) => b.NE == a);
            Go_NE.AddEffect("new citizen loc", ref citizen1, c => c.location, ref area1_F);
            Go_NE.AddEffect("set bridge used", ref bridge1, b => b.used, true);
            Konigsberg.AddAction(Go_NE);
           
            //Go South or West           
            Citizen citizen2 = null;
            Area area2_S = null;
            Area area2_F = null;
            Bridge bridge2 = null;

            ActionPDDL Go_SW = new ActionPDDL("Go S or W");
            Go_SW.AddPrecondition<Citizen, Citizen, Area, Area>("Citizen is NE", ref citizen2, ref area2_S, (c, a) => c.location == a);
            Go_SW.AddPrecondition("Bridge is unused yet", ref bridge2, b => b.used == false);
            Go_SW.AddPrecondition<Bridge, Bridge, Area, Area>("Start brigde is NE", ref bridge2, ref area2_S, (b, a) => b.NE == a);
            Go_SW.AddPrecondition<Bridge, Bridge, Area, Area>("Finish bridge is SW", ref bridge2, ref area2_F, (b, a) => b.SW == a);
            Go_SW.AddEffect("new citizen loc", ref citizen2, c => c.location, ref area2_F);
            Go_SW.AddEffect("set bridge used", ref bridge2, b => b.used, true);
            Konigsberg.AddAction(Go_SW);

            /*  
                Plan of Königsberg city:
                ┌─────────────────────────┐
                │                         │
                │            A            │
                │                         │
                │    ╔═══1════2═╦════3════╡
                │    ║          ║         │
                │    ║     B    4         │
                │    ║          ║    C    │
                ╞════╩══5════6══╣         │
                │               ║         │
                │               ╚════7════╡
                │          D              │
                │                         │
                └─────────────────────────┘
                Letter - Area, Number - Bridge
            */

            Citizen Königsberger = new Citizen();

            Area A = new Area('A');
            Area B = new Area('B');
            Area C = new Area('C');
            Area D = new Area('D');

            Bridge b1 = new Bridge(1, A, B);
            Bridge b2 = new Bridge(2, A, B);
            Bridge b3 = new Bridge(3, A, C);
            Bridge b4 = new Bridge(4, C, B);
            Bridge b5 = new Bridge(5, B, D);
            Bridge b6 = new Bridge(6, B, D);
            Bridge b7 = new Bridge(7, C, D);

            GoalPDDL CrossAllOnce = new GoalPDDL("Cross all bridges once", GoalPriority.HighPriority);
            //To check right of working comment one of Expected State below and run
            CrossAllOnce.AddExpectedObjectState(b1, b => b.used);
            CrossAllOnce.AddExpectedObjectState(b2, b => b.used);
            CrossAllOnce.AddExpectedObjectState(b3, b => b.used);
            CrossAllOnce.AddExpectedObjectState(b4, b => b.used);
            CrossAllOnce.AddExpectedObjectState(b5, b => b.used);
            CrossAllOnce.AddExpectedObjectState(b6, b => b.used);
            CrossAllOnce.AddExpectedObjectState(b7, b => b.used);
            //To check right of working comment one of Expected State top and run
            Konigsberg.AddGoal(CrossAllOnce);

            Konigsberg.domainObjects.Add(Königsberger);
            Konigsberg.domainObjects.Add(A);
            Konigsberg.domainObjects.Add(B);
            Konigsberg.domainObjects.Add(C);
            Konigsberg.domainObjects.Add(D);
            Konigsberg.domainObjects.Add(b1);
            Konigsberg.domainObjects.Add(b2);
            Konigsberg.domainObjects.Add(b3);
            Konigsberg.domainObjects.Add(b4);
            Konigsberg.domainObjects.Add(b5);
            Konigsberg.domainObjects.Add(b6);
            Konigsberg.domainObjects.Add(b7);

            TraceSource traceSource = new TraceSource("Tracing");
            traceSource.Switch.Level = SourceLevels.All;
            traceSource.Listeners.Add(new TextWriterTraceListener(Console.Out, "Console"));
            traceSource.Listeners["Console"].Filter = new NoWayFilter();
            Konigsberg.DefineTrace(traceSource);

            Konigsberg.PlanGenerated += Impossible;
            Konigsberg.Start();
            Console.ReadKey();
        }
    }
}
