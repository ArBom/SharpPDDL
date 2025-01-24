using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPDDL
{
    internal static class Agrees
    {
        internal const byte Go_AHEAD = 0b_0000;
        internal const byte SpecialAction = 0b_0001;
        internal const byte EveryAction = 0b_0011;
        internal const byte Plan = 0b_0100;
        internal const byte DONT_DO_IT = 0b_1000;
    }

    public enum AskToAgree : byte
    {
        GO_AHEAD = Agrees.Go_AHEAD,
        //SpecialAction = Agrees.SpecialAction,
        EveryAction = Agrees.EveryAction,
        Plan = Agrees.Plan,
        DONT_DO_IT = Agrees.DONT_DO_IT
    }

    class PlanImplementor
    {
        internal WaitHandle SignalizeNeedAcception;
        internal WaitHandle WaitOn;
        internal byte PlanImplementor_Agrees = 0b_1111;
        internal IReadOnlyList<Delegate> InstantActionsExecutionPDDL;
        private PossibleState CurrentState;
        internal CancellationToken cancelationToken;

        Task ImplementorTask;

        internal void UpdateIt(DomeinPDDL Owner)
        {
            InstantActionsExecutionPDDL = Owner.actions.Select(a => a.InstantExecution).ToList();
            CurrentState = Owner.CurrentState;
        }

        internal void UpdateIt(WaitHandle SignalizeNeedAcception, WaitHandle WaitOn, byte PlanImplementor_Agrees)
        {
            this.SignalizeNeedAcception = SignalizeNeedAcception;
            this.WaitOn = WaitOn;
            this.PlanImplementor_Agrees = PlanImplementor_Agrees;
        }

        protected void ActionListRealize(List<CrisscrossChildrenCon> ActionList, CancellationToken cancelationToken)
        {
            if ((PlanImplementor_Agrees & Agrees.DONT_DO_IT) != 0)
                return;

            if ((PlanImplementor_Agrees & Agrees.Plan) != 0)
                WaitHandle.SignalAndWait(SignalizeNeedAcception, WaitOn);

            foreach (CrisscrossChildrenCon Act in ActionList)
            {
                if ((PlanImplementor_Agrees & Agrees.EveryAction) != 0)  //nieco złożone pytanie
                    WaitHandle.SignalAndWait(SignalizeNeedAcception, WaitOn);

                if (InstantActionsExecutionPDDL[Act.ActionNr] is null)
                {
                    continue; //bardzo TODO
                }

                try
                {
                    var ExecutionTask = new Task(() =>
                    {
                        InstantActionsExecutionPDDL[Act.ActionNr].DynamicInvoke(Act.ActionArgOryg);
                    }, cancelationToken);

                    ExecutionTask.RunSynchronously();
                }
                catch (EffectExecutionException UnexpectedPrecondition)
                {

                }
                catch (Exception exception)
                {

                }

                CurrentState = Act.Child.Content;
            }
        }

        internal void RealizeIt(List<CrisscrossChildrenCon> ActionList)
        {
            Action actions = () => ActionListRealize(ActionList, cancelationToken);
            ImplementorTask = Task.Factory.StartNew(actions, cancelationToken);
        }
    }
}
