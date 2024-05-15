using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SharpPDDL;

namespace Hanoi_Tower
{
    class Program
    {
        public class HanoiObj //Sorry it cannot be abstract
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
            public int no;
            public HanoiTable(int no, int HanoiObjSizeUpSide = 0, bool isEmpty = true)
            {
                this.no = no;
                this.HanoiObjSizeUpSide = HanoiObjSizeUpSide;
                this.IsEmptyUpSide = isEmpty;
            }
        }

        static void Main(string[] args)
        {
            DomeinPDDL newDomein = new DomeinPDDL("Hanoi");

            HanoiBrick MovedBrick = null;
            HanoiObj ObjBelowMoved = null;
            HanoiBrick NewStandB = null;
            HanoiTable NewStandT = null;

            Expression<Predicate<HanoiBrick>> MovedBrickIsNoUp = (HB => HB.IsEmptyUpSide);
            Expression<Predicate<HanoiBrick>> NewStandBrickIsEmpty = (HO => HO.IsEmptyUpSide);
            Expression<Predicate<HanoiTable>> NewStandTableIsEmpty = (HO => HO.IsEmptyUpSide);
            Expression<Predicate<HanoiBrick, HanoiBrick>> PutSmallBrickAtBigger = ((MB, NSB) => (MB.Size < NSB.Size));
            Expression<Predicate<HanoiBrick, HanoiObj>> FindObjBelongMovd = ((MB, OBM) => (MB.Size == OBM.HanoiObjSizeUpSide));

            ActionPDDL moveBrickOnBrick = new ActionPDDL("Move brick on another brick");

            moveBrickOnBrick.AddPrecondiction("Moved brick is no up", ref MovedBrick, MovedBrickIsNoUp);
            moveBrickOnBrick.AddPrecondiction("New stand is empty", ref NewStandB, NewStandBrickIsEmpty);
            moveBrickOnBrick.AddPrecondiction("Small brick on bigger one", ref MovedBrick, ref NewStandB, PutSmallBrickAtBigger);
            moveBrickOnBrick.AddPrecondiction("Find brick bottom moved one", ref MovedBrick, ref ObjBelowMoved, FindObjBelongMovd);

            moveBrickOnBrick.AddEffect("New stand is full", false, ref NewStandB, NS => NS.IsEmptyUpSide);
            moveBrickOnBrick.AddEffect("Old stand is empty", true, ref ObjBelowMoved, NS => NS.IsEmptyUpSide);
            moveBrickOnBrick.AddEffect("UnConsociate Objs", 0, ref ObjBelowMoved, OS => OS.HanoiObjSizeUpSide);
            moveBrickOnBrick.AddEffect("Consociate Bricks", ref MovedBrick, MB => MB.Size, ref NewStandB, NSB => NSB.HanoiObjSizeUpSide);

            newDomein.AddAction(moveBrickOnBrick);

            ActionPDDL moveBrickOnTable = new ActionPDDL("Move brick on table");

            //moveBrickOnTable.AddPrecondiction("rubbish", ref NewStandT, (HT => HT.no != -10));

            moveBrickOnTable.AddPrecondiction("Moved brick is no up", ref MovedBrick, MovedBrickIsNoUp);
            moveBrickOnTable.AddPrecondiction("New table is empty", ref NewStandT, NewStandTableIsEmpty);
            moveBrickOnTable.AddPrecondiction("Find brick bottom moved one", ref MovedBrick, ref ObjBelowMoved, FindObjBelongMovd);

            moveBrickOnTable.AddEffect("New stand is full", false, ref NewStandT, NS => NS.IsEmptyUpSide); //bierze stąt bo tak
            moveBrickOnTable.AddEffect("Old stand is empty", true, ref ObjBelowMoved, NS => NS.IsEmptyUpSide);
            moveBrickOnTable.AddEffect("UnConsociate Objs", 0, ref ObjBelowMoved, OS => OS.HanoiObjSizeUpSide);
            moveBrickOnTable.AddEffect("Consociate Bricks", ref MovedBrick, MB => MB.Size, ref NewStandT, NST => NST.HanoiObjSizeUpSide);

            newDomein.AddAction(moveBrickOnTable);

            List<HanoiBrick> HanoiBricks = new List<HanoiBrick>();

            int MaxBriSize = 1;
            for (int Bri = 1; Bri <= MaxBriSize; Bri++)
            {
                HanoiBrick newOne = new HanoiBrick(Bri);
                if (Bri != 1)
                    newOne.HanoiObjSizeUpSide = Bri - 1;
                else
                    newOne.IsEmptyUpSide = true;

                HanoiBricks.Add(newOne);
                newDomein.domainObjects.Add(newOne);
            }

            List<HanoiTable> HanoiTables = new List<HanoiTable> { new HanoiTable(0, MaxBriSize, false), new HanoiTable(1), new HanoiTable(2) };

            foreach (var HT in HanoiTables)
                newDomein.domainObjects.Add(HT);

            GoalPDDL movedBrick = new GoalPDDL("Moved the Brick");
            //movedBrick.AddExpectedObjectState(new List<Expression<Predicate<HanoiTable>>> { HT => HT.IsEmptyUpSide });
            movedBrick.AddExpectedObjectState( HT => HT.IsEmptyUpSide, HanoiTables[0]);
            newDomein.AddGoal(movedBrick);

            newDomein.Start();

            int AO = 1500;
        }
    }
}
