using SharpPDDL;
using Gripper.EnvClasses;

namespace Gripper
{
    public static class PlanningActions
    {
        public static void AddActions(DomainPDDL domainPDDL)
        {
            domainPDDL.AddAction(MoveClawLeft());
            domainPDDL.AddAction(MoveClawRight());
            domainPDDL.AddAction(MoveClawUp());
            domainPDDL.AddAction(ChangeClawSize());
            domainPDDL.AddAction(MoveClawDownToEmptyTable());
            domainPDDL.AddAction(MoveClawDownToBall());
            domainPDDL.AddAction(MoveClawDownToBox());
            domainPDDL.AddAction(PutBoxOnTable());
            domainPDDL.AddAction(PutBallOnTable());
            domainPDDL.AddAction(TakeBoxFromTable());
            domainPDDL.AddAction(TakeBallFromTable());
            domainPDDL.AddAction(TakeBallFromBox());
            domainPDDL.AddAction(PutBallToBox());
        }

        private static ActionPDDL MoveClawLeft()
        {
            Claw claw = null;

            ActionPDDL MoveClawLeft = new ActionPDDL("MoveClawLeft", 1);

            MoveClawLeft.AddPrecondition("Claw is up", ref claw, c => c.Depth == 0);
            MoveClawLeft.AddPrecondition("There is a place at left", ref claw, c => c.Xpos > 3);
            MoveClawLeft.AddEffect("Decr spot", ref claw, c => c.Xpos, ref claw, c => c.Xpos - 1);
            MoveClawLeft.AddExecution("Decr spot");
            MoveClawLeft.AddExecution("Draw", () => Gripper.Program.Draw(), true);
            MoveClawLeft.AddPartOfActionSententia("Move claw at left");

            return MoveClawLeft;
        }

        private static ActionPDDL MoveClawRight()
        {
            Claw claw = null;

            ActionPDDL MoveClawRight = new ActionPDDL("MoveClawRight", 1);

            MoveClawRight.AddPrecondition("Claw is up", ref claw, c => c.Depth == 0);
            MoveClawRight.AddPrecondition("There is a place at right", ref claw, c => c.Xpos < 24);
            MoveClawRight.AddEffect("Incr spot", ref claw, c => c.Xpos, ref claw, c => c.Xpos + 1);
            MoveClawRight.AddExecution("Incr spot");
            MoveClawRight.AddExecution("Draw", () => Gripper.Program.Draw(), true);
            MoveClawRight.AddPartOfActionSententia("Move claw at right");

            return MoveClawRight;
        }

        private static ActionPDDL MoveClawUp()
        {
            Claw claw = null;

            ActionPDDL MoveClawUp = new ActionPDDL("MoveClawUp", 4);

            MoveClawUp.AddPrecondition("Claw is not up", ref claw, c => c.Depth > 0);
            MoveClawUp.AddEffect("Decr depth", ref claw, c => c.Depth, ref claw, c => c.Depth - 1);
            MoveClawUp.AddExecution("Decr depth");
            MoveClawUp.AddExecution("Draw", () => Gripper.Program.Draw(), true);
            MoveClawUp.AddPartOfActionSententia("Move claw up");

            return MoveClawUp;
        }

        private static ActionPDDL ChangeClawSize()
        {
            Claw claw = null;

            ActionPDDL ChangeClawSize = new ActionPDDL("ChangeClawSize", 1);

            ChangeClawSize.AddPrecondition("Claw is up", ref claw, c => c.Depth <= 1);
            ChangeClawSize.AddPrecondition("Claw is empty", ref claw, c => c.Content == null);
            ChangeClawSize.AddEffect("Change Size", ref claw, c => c.IsBigger, ref claw, c => !(c.IsBigger));
            ChangeClawSize.AddExecution("Change Size");
            ChangeClawSize.AddExecution("Draw", () => Gripper.Program.Draw(), true);
            ChangeClawSize.AddPartOfActionSententia("Change claw size");

            return ChangeClawSize;
        }

        private static ActionPDDL MoveClawDownToEmptyTable()
        {
            Claw claw = null;
            Table table = null;

            ActionPDDL MoveClawDownToEmptyTable = new ActionPDDL("MoveClawDownToEmptyTable", 3);

            MoveClawDownToEmptyTable.AddPrecondition("Claw is not down", ref claw, c => c.Depth < 4);
            MoveClawDownToEmptyTable.AddPrecondition("Claw is over table", ref claw, c => c.Xpos % 7 == 3);
            MoveClawDownToEmptyTable.AddPrecondition<Claw, Claw, Table, Table>("Claw is at table spot", ref claw, ref table, (c, t) => c.Xpos / 7 == t.Spot);
            MoveClawDownToEmptyTable.AddPrecondition("Table is empty", ref table, t => t.Content == null);
            MoveClawDownToEmptyTable.AddEffect("Incr depth", ref claw, c => c.Depth, ref claw, c => c.Depth + 1);
            MoveClawDownToEmptyTable.AddExecution("Incr depth");
            MoveClawDownToEmptyTable.AddExecution("Draw", () => Gripper.Program.Draw(), true);
            MoveClawDownToEmptyTable.AddPartOfActionSententia("Move claw down (e)");

            return MoveClawDownToEmptyTable;
        }

        private static ActionPDDL MoveClawDownToBall()
        {
            Claw claw = null;
            Table table = null;
            Ball ball = null;

            ActionPDDL MoveClawDownToBall = new ActionPDDL("MoveClawDownToBall", 3);

            MoveClawDownToBall.AddPrecondition("Claw is not down", ref claw, c => c.Depth < 4);
            MoveClawDownToBall.AddPrecondition("Claw is over table", ref claw, c => c.Xpos % 7 == 3);
            MoveClawDownToBall.AddPrecondition<Claw, Claw, Table, Table>("Claw is at table spot", ref claw, ref table, (c, t) => c.Xpos / 7 == t.Spot);
            MoveClawDownToBall.AddPrecondition<Table, Table, Ball, Ball>("Ball is on the table", ref table, ref ball, (t, b) => t.Content == b);
            MoveClawDownToBall.AddEffect("Incr depth", ref claw, c => c.Depth, ref claw, c => c.Depth + 1);
            MoveClawDownToBall.AddExecution("Incr depth");
            MoveClawDownToBall.AddExecution("Draw", () => Gripper.Program.Draw(), true);
            MoveClawDownToBall.AddPartOfActionSententia("Move claw down (ba)");

            return MoveClawDownToBall;
        }

        private static ActionPDDL MoveClawDownToBox()
        {
            Claw claw = null;
            Table table = null;
            Box box = null;

            ActionPDDL MoveClawDownToBox = new ActionPDDL("MoveClawDownToBox", 3);

            MoveClawDownToBox.AddPrecondition("Claw is not down", ref claw, c => c.Depth < 3);
            MoveClawDownToBox.AddPrecondition("Claw is over table", ref claw, c => c.Xpos % 7 == 3);
            MoveClawDownToBox.AddPrecondition<Claw, Claw, Table, Table>("Claw is at table spot", ref claw, ref table, (c, t) => c.Xpos / 7 == t.Spot);
            MoveClawDownToBox.AddPrecondition<Table, Table, Box, Box>("Ball is on the table", ref table, ref box, (t, b) => t.Content == b);
            MoveClawDownToBox.AddEffect("Incr depth", ref claw, c => c.Depth, ref claw, c => c.Depth + 1);
            MoveClawDownToBox.AddExecution("Incr depth");
            MoveClawDownToBox.AddExecution("Draw", () => Gripper.Program.Draw(), true);
            MoveClawDownToBox.AddPartOfActionSententia("Move claw down (bo)");

            return MoveClawDownToBox;
        }

        private static ActionPDDL PutBoxOnTable()
        {
            Claw claw = null;
            Table table = null;
            Box box = null;

            ActionPDDL PutBoxOnTable = new ActionPDDL("PutBoxOnTable", 2);
            PutBoxOnTable.AddPrecondition("Claw correct depth", ref claw, c => c.Depth == 3);
            PutBoxOnTable.AddPrecondition("Claw is over table", ref claw, c => c.Xpos % 7 == 3);
            PutBoxOnTable.AddPrecondition<Claw, Claw, Table, Table>("Claw is at table spot", ref claw, ref table, (c, t) => c.Xpos / 7 == t.Spot);
            PutBoxOnTable.AddPrecondition("Table is empty", ref table, t => t.Content == null);
            PutBoxOnTable.AddPrecondition<Claw, Claw, Box, Box>("Box is in claw", ref claw, ref box, (c,t) => c.Content == t);
            PutBoxOnTable.AddEffect("Make box on table", ref table, t => t.Content, ref box);
            PutBoxOnTable.AddEffect("Make claw empty", ref claw, c => c.Content, null);
            PutBoxOnTable.AddExecution("Make box on table");
            PutBoxOnTable.AddExecution("Make claw empty");
            PutBoxOnTable.AddExecution("Draw", () => Gripper.Program.Draw(), true);

            return PutBoxOnTable;
        }

        private static ActionPDDL PutBallOnTable()
        {
            Claw claw = null;
            Table table = null;
            Ball ball = null;

            ActionPDDL PutBallOnTable = new ActionPDDL("PutBallOnTable", 2);
            PutBallOnTable.AddPrecondition("Claw correct depth", ref claw, c => c.Depth == 4);
            PutBallOnTable.AddPrecondition("Claw is over table", ref claw, c => c.Xpos % 7 == 3);
            PutBallOnTable.AddPrecondition<Claw, Claw, Table, Table>("Claw is at table spot", ref claw, ref table, (c, t) => c.Xpos / 7 == t.Spot);
            PutBallOnTable.AddPrecondition("Table is empty", ref table, t => t.Content == null);
            PutBallOnTable.AddPrecondition<Claw, Claw, Ball, Ball>("Ball is in claw", ref claw, ref ball, (c, b) => c.Content == b);
            PutBallOnTable.AddEffect("Make ball on table", ref table, t => t.Content, ref ball);
            PutBallOnTable.AddEffect("Make claw empty", ref claw, c => c.Content, null);
            PutBallOnTable.AddExecution("Make ball on table");
            PutBallOnTable.AddExecution("Make claw empty");
            PutBallOnTable.AddExecution("Draw", () => Gripper.Program.Draw(), true);

            return PutBallOnTable;
        }

        private static ActionPDDL TakeBoxFromTable()
        {
            Claw claw = null;
            Table table = null;
            Box box = null;

            ActionPDDL TakeBoxFromTable = new ActionPDDL("TakeBoxFromTable", 2);
            TakeBoxFromTable.AddPrecondition("Claw correct depth", ref claw, c => c.Depth == 3);
            TakeBoxFromTable.AddPrecondition("Claw is over table", ref claw, c => c.Xpos % 7 == 3);
            TakeBoxFromTable.AddPrecondition("Claw is big", ref claw, c => c.IsBigger);
            TakeBoxFromTable.AddPrecondition<Claw, Claw, Table, Table>("Claw is at table spot", ref claw, ref table, (c, t) => c.Xpos / 7 == t.Spot);
            TakeBoxFromTable.AddPrecondition("Claw is empty", ref claw, c => c.Content == null);
            TakeBoxFromTable.AddPrecondition<Table, Table, Box, Box>("Box is on table", ref table, ref box, (t,b) => t.Content == b);
            TakeBoxFromTable.AddEffect("Make box in claw", ref claw, c => c.Content, ref box);
            TakeBoxFromTable.AddEffect("Make table empty", ref table, t => t.Content, null);
            TakeBoxFromTable.AddExecution("Make box in claw");
            TakeBoxFromTable.AddExecution("Make table empty");
            TakeBoxFromTable.AddExecution("Draw", () => Gripper.Program.Draw(), true);

            return TakeBoxFromTable;
        }

        private static ActionPDDL TakeBallFromTable()
        {
            Claw claw = null;
            Table table = null;
            Ball ball = null;

            ActionPDDL TakeBallFromTable = new ActionPDDL("TakeBallFromTable", 2);
            TakeBallFromTable.AddPrecondition("Claw correct depth", ref claw, c => c.Depth == 4);
            TakeBallFromTable.AddPrecondition("Claw is over table", ref claw, c => c.Xpos % 7 == 3);
            TakeBallFromTable.AddPrecondition("Claw is small", ref claw, c => !c.IsBigger);
            TakeBallFromTable.AddPrecondition<Claw, Claw, Table, Table>("Claw is at table spot", ref claw, ref table, (c, t) => c.Xpos / 7 == t.Spot);
            TakeBallFromTable.AddPrecondition("Claw is empty", ref claw, c => c.Content == null);
            TakeBallFromTable.AddPrecondition<Table, Table, Ball, Ball>("Ball is on table", ref table, ref ball, (t, b) => t.Content == b);
            TakeBallFromTable.AddEffect("Make ball in claw", ref claw, c => c.Content, ref ball);
            TakeBallFromTable.AddEffect("Make table empty", ref table, t => t.Content, null);
            TakeBallFromTable.AddExecution("Make ball in claw");
            TakeBallFromTable.AddExecution("Make table empty");
            TakeBallFromTable.AddExecution("Draw", () => Gripper.Program.Draw(), true);

            return TakeBallFromTable;
        }

        private static ActionPDDL TakeBallFromBox()
        {
            Claw claw = null;
            Table table = null;
            Box box = null;
            Ball ball = null;

            ActionPDDL TakeBallFromBox = new ActionPDDL("TakeBallFromBox", 2);
            TakeBallFromBox.AddPrecondition("Claw correct depth", ref claw, c => c.Depth == 3);
            TakeBallFromBox.AddPrecondition("Claw is over table", ref claw, c => c.Xpos % 7 == 3);
            TakeBallFromBox.AddPrecondition("Claw is small", ref claw, c => !c.IsBigger);
            TakeBallFromBox.AddPrecondition<Claw, Claw, Table, Table>("Claw is at table spot", ref claw, ref table, (c, t) => c.Xpos / 7 == t.Spot);
            TakeBallFromBox.AddPrecondition("Claw is empty", ref claw, c => c.Content == null);
            TakeBallFromBox.AddPrecondition<Table, Table, Box, Box>("Box is on table", ref table, ref box, (t, bo) => t.Content == bo);
            TakeBallFromBox.AddPrecondition<Box, Box, Ball, Ball>("Ball is in box", ref box, ref ball, (bo, ba) => bo.Content == ba);
            TakeBallFromBox.AddEffect("Make ball in claw", ref claw, c => c.Content, ref ball);
            TakeBallFromBox.AddEffect("Make box empty", ref box, bo => bo.Content, null);
            TakeBallFromBox.AddExecution("Make ball in claw");
            TakeBallFromBox.AddExecution("Make box empty");
            TakeBallFromBox.AddExecution("Draw", () => Gripper.Program.Draw(), true);

            return TakeBallFromBox;
        }

        private static ActionPDDL PutBallToBox()
        {
            Claw claw = null;
            Table table = null;
            Box box = null;
            Ball ball = null;

            ActionPDDL PutBallToBox = new ActionPDDL("PutBallToBox", 2);
            PutBallToBox.AddPrecondition("Claw correct depth", ref claw, c => c.Depth == 3);
            PutBallToBox.AddPrecondition("Claw is over table", ref claw, c => c.Xpos % 7 == 3);
            PutBallToBox.AddPrecondition<Claw, Claw, Table, Table>("Claw is at table spot", ref claw, ref table, (c, t) => c.Xpos / 7 == t.Spot);
            PutBallToBox.AddPrecondition<Claw, Claw, Ball, Ball>("Ball is in claw", ref claw, ref ball, (c,b) => c.Content == b);
            PutBallToBox.AddPrecondition<Table, Table, Box, Box>("Box is on table", ref table, ref box, (t, bo) => t.Content == bo);
            PutBallToBox.AddPrecondition("Box is empty", ref box, b => b.Content == null);
            PutBallToBox.AddEffect("Make ball in box", ref box, c => c.Content, ref ball);
            PutBallToBox.AddEffect("Make claw empty", ref claw, c => c.Content, null);
            PutBallToBox.AddExecution("Make ball in box");
            PutBallToBox.AddExecution("Make claw empty");
            PutBallToBox.AddExecution("Draw", () => Gripper.Program.Draw(), true);

            return PutBallToBox;
        }
    }
}
