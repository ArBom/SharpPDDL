using System;
using System.Collections.Generic;
using SharpPDDL;
using Gripper.EnvClasses;
using System.Text;
using System.Threading;
using System.Linq;

namespace Gripper
{
    class Program
    {
        static void Main(string[] args)
        {
            envObjsToDraw = new List<EnvObj>();

            //Make objects of domain
            Ball Ball_R = new Ball(ConsoleColor.Red);
            Ball Ball_G = new Ball(ConsoleColor.Green);
            Ball Ball_B = new Ball(ConsoleColor.Blue);
            Box Box_R = new Box(ConsoleColor.Red);
            Box Box_G = new Box(ConsoleColor.Green);
            Box Box_B = new Box(ConsoleColor.Blue);
            Table Table_W = new Table(0, ConsoleColor.White);
            Table Table_R = new Table(1, ConsoleColor.Red);
            Table Table_G = new Table(2, ConsoleColor.Green);
            Table Table_B = new Table(3, ConsoleColor.Blue);
            Claw Claw = new Claw();

            //Make associative between domain objects
            Box_R.Content = Ball_G;  //The green ball is in the red box
            Table_B.Content = Box_R; //The Red box is on the blue table
            Box_G.Content = Ball_B;  //The blue ball is in the green box
            Table_R.Content = Box_G; //The green box is on the red table
            Box_B.Content = Ball_R;  //The red ball is in the blue box
            Table_G.Content = Box_B; //The blue box is on the green table

            //add objects to draw to the list
            envObjsToDraw.Add(Table_W);
            envObjsToDraw.Add(Table_R);
            envObjsToDraw.Add(Table_G);
            envObjsToDraw.Add(Table_B);
            envObjsToDraw.Add(Claw);

            //create a domain
            DomainPDDL CraneDomain = new DomainPDDL("Crane");

            //add all objects to the domain
            CraneDomain.domainObjects.Add(Ball_R);
            CraneDomain.domainObjects.Add(Ball_G);
            CraneDomain.domainObjects.Add(Ball_B);
            CraneDomain.domainObjects.Add(Box_R);
            CraneDomain.domainObjects.Add(Box_G);
            CraneDomain.domainObjects.Add(Box_B);
            CraneDomain.domainObjects.Add(Table_W);
            CraneDomain.domainObjects.Add(Table_R);
            CraneDomain.domainObjects.Add(Table_G);
            CraneDomain.domainObjects.Add(Table_B);
            CraneDomain.domainObjects.Add(Claw);

            //add rules of the domain
            PlanningActions.AddActions(CraneDomain);

            //create a goal and define attribute of objects
            GoalPDDL goalPDDL = new GoalPDDL("Sorted");
            goalPDDL.AddExpectedObjectState(Table_R, t => t.Colour == t.Content.Colour); //On the red table should be leight an object of the same colour
            goalPDDL.AddExpectedObjectState(Box_R, b => b.Colour == b.Content.Colour);   //In the red box should be an object (a ball) of the same colour

            goalPDDL.AddExpectedObjectState(Table_G, t => t.Colour == t.Content.Colour); //On the green table should be leight an object of the same colour
            goalPDDL.AddExpectedObjectState(Box_G, b => b.Colour == b.Content.Colour);   //In the green box should be an object (a ball) of the same colour

            goalPDDL.AddExpectedObjectState(Table_B, t => t.Colour == t.Content.Colour); //On the blue table should be leight an object of the same colour
            goalPDDL.AddExpectedObjectState(Box_B, b => b.Colour == b.Content.Colour);   //In the blue box should be an object (a ball) of the same colour

            goalPDDL.AddExpectedObjectState(Claw, c => c.Depth == 0); //The claw should be at max up position
            CraneDomain.AddGoal(goalPDDL);

            CraneDomain.SetExecutionOptions(null, null, AskToAgree.GO_AHEAD); //execute the actions

            CraneDomain.GenerateDiagrams("Classes.dgml", Diagram.Class); //create diagrams while run

            //set correct console options
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;

            Draw(); //draw the beginning state
            CraneDomain.Start(); //run the algorithm
            Console.ReadKey();
        }

        /// <summary>
        /// List of objects to draw in Draw() function
        /// </summary>
        public static List<EnvObj> envObjsToDraw;

        /// <summary>
        /// Draw objects from envObjsToDraw the list
        /// </summary>
        public static void Draw()
        {
            int clawSpot = ((Claw)envObjsToDraw.First(o => o is Claw)).AboveSpot;
            
            for (int i = 0; i != 7; i++)
            {
                Console.SetCursorPosition(clawSpot * 7, i);
                if (i <= 2)
                {
                    Console.SetCursorPosition(0, i);
                    Console.WriteLine("                            ");
                }
                else
                {
                    Console.SetCursorPosition(clawSpot * 7, i);
                    Console.Write("       ");
                }
            }

            foreach (var y in envObjsToDraw)
            {
                y.Draw();
            }

            Thread.Sleep(200);
        }
    }
}
