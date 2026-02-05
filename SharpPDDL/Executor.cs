using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPDDL
{
    internal class Executor
    {
        private IReadOnlyList<Delegate> actions;
        private readonly ImplementorUpdater ImplementorUpdate;
        private ICollection<object> domainObjects;
        private ICollection<GoalPDDL> domainGoals;

        internal Action PlanRealized;
        internal CancellationTokenSource InternalCancelationPlanImplementor;

        internal Task ImplementorTask { get; private set; }

        internal Executor(DomeinPDDL Owner)
        {
            this.actions = Owner.actions.Select(a => a.InstantExecution).ToList();
            this.ImplementorUpdate = Owner.ImplementorUpdate;
            this.domainObjects = Owner.domainObjects;
            this.domainGoals = Owner.domainGoals;
        }

        protected void WaitActionsListAgrees(List<CrisscrossChildrenCon> ActionList, CancellationToken cancelationToken)
        {
            if ((ImplementorUpdate.PlanImplementor_Agrees & ExecutionAgrees.Plan) == 0)
                return;

            GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 26, GloCla.ResMan.GetString("V2"));

            object[] ActionsNameArray = ActionList.Select(AL => (object)(actions[AL.ActionNr].Method.Name)).ToArray();
            ImplementorUpdate.SignalizeNeedAcception?.Invoke(GloCla.PlanToAcceptation, ActionsNameArray);

            if (ImplementorUpdate.WaitOn is null)
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 127, GloCla.ResMan.GetString("C41"));

            WaitHandle.WaitAny(new WaitHandle[] { ImplementorUpdate.WaitOn, cancelationToken.WaitHandle });

            if (cancelationToken.IsCancellationRequested)
                GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 29, GloCla.ResMan.GetString("V4"));
            else
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 27, GloCla.ResMan.GetString("V3"));
                ImplementorUpdate.WaitOn.Reset();
            }
        }

        /// <summary>
        /// Function is waiting for agree or cancelation with CancelationToken
        /// </summary>
        /// <param name="Act">Action to agree check</param>
        /// <param name="cancelationToken"></param>
        /// <returns>It returns TRUE for waiting needed, FALSE for not needes</returns>
        protected bool WaitTheActionAgree(CrisscrossChildrenCon Act, CancellationToken cancelationToken)
        {
            if (actions[Act.ActionNr].Method.Name.StartsWith(GloCla.SpecialFuncPrefix) && (ImplementorUpdate.PlanImplementor_Agrees & ExecutionAgrees.SpecialAction) != 0)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 30, GloCla.ResMan.GetString("V5"), actions[Act.ActionNr].Method.Name.Substring(1));
                ImplementorUpdate.SignalizeNeedAcception?.Invoke(actions[Act.ActionNr].Method.Name.Substring(1), Act.ActionArgOryg);

                if (ImplementorUpdate.WaitOn is null)
                    GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 128, GloCla.ResMan.GetString("C42"), actions[Act.ActionNr].Method.Name.Substring(1));

                WaitHandle.WaitAny(new WaitHandle[] { ImplementorUpdate.WaitOn, cancelationToken.WaitHandle });
                return true;
            }
            else if ((ImplementorUpdate.PlanImplementor_Agrees & ExecutionAgrees.EveryAction) != 0)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 31, GloCla.ResMan.GetString("V6"), actions[Act.ActionNr].Method.Name);
                ImplementorUpdate.SignalizeNeedAcception?.Invoke(actions[Act.ActionNr].Method.Name, Act.ActionArgOryg);

                if (ImplementorUpdate.WaitOn is null)
                    GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 129, GloCla.ResMan.GetString("C43"), actions[Act.ActionNr].Method.Name.Substring(1));

                WaitHandle.WaitAny(new WaitHandle[] { ImplementorUpdate.WaitOn, cancelationToken.WaitHandle });
                return true;
            }
            else
                return false;
        }

        protected void ExecuteTheAction(CrisscrossChildrenCon Act, CancellationToken cancelationToken)
        {
            int ExecutionTaskId = -1;

                Task ExecutionTask = new Task(() =>
                {
                    try
                    {
                        actions[Act.ActionNr].DynamicInvoke(Act.ActionArgOryg);
                    }
                    catch (Exception exception)
                    {
                        if (!(exception.InnerException is null))
                        {
                            if (exception.InnerException is PrecondExecutionException)
                            {
                                GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 23, GloCla.ResMan.GetString("W1"), exception.InnerException.Data[PrecondExecutionException.ActionName], exception.InnerException.Data[PrecondExecutionException.ExecutionPreconditionName]);
                                throw exception.InnerException;
                                //TODO no i co dalej?
                            }
                        }

                        GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 24, GloCla.ResMan.GetString("C4"), actions[Act.ActionNr].Method.Name, exception.ToString());
                        throw exception;
                    }
                    finally
                    {
                        GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 35, GloCla.ResMan.GetString("Sp3"), ExecutionTaskId);
                    }
                }, cancelationToken);

                if (!(GloCla.Tracer is null))
                {
                    ExecutionTaskId = ExecutionTask.Id;
                    GloCla.Tracer.TraceEvent(TraceEventType.Start, 34, GloCla.ResMan.GetString("Sa3"), actions[Act.ActionNr].Method.Name, ExecutionTaskId);
                }

                ExecutionTask.Start();
                ExecutionTask.Wait();

            if (!(GloCla.Tracer is null))
                actions[Act.ActionNr].DynamicInvoke(Act);
        }

        protected void ActionListRealize(List<CrisscrossChildrenCon> ActionList, CancellationToken CancelationPlanImplementor)
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 22, GloCla.ResMan.GetString("Sa2"));

            if ((ImplementorUpdate.PlanImplementor_Agrees & ExecutionAgrees.DONT_DO_IT) != 0)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 25, GloCla.ResMan.GetString("Sp1"));
                return;
            }

            WaitActionsListAgrees(ActionList, CancelationPlanImplementor);

            if (CancelationPlanImplementor.IsCancellationRequested)
                return;

            foreach (CrisscrossChildrenCon Act in ActionList)
            {
                if (actions[Act.ActionNr] is null)
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 28, GloCla.ResMan.GetString("W2"));
                    continue;
                }

                bool NeedToWait = WaitTheActionAgree(Act, CancelationPlanImplementor);

                if (CancelationPlanImplementor.IsCancellationRequested)
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 32, GloCla.ResMan.GetString("V7"), actions[Act.ActionNr].Method.Name);
                    return;
                }

                if (NeedToWait)
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 33, GloCla.ResMan.GetString("V8"), actions[Act.ActionNr].Method.Name);
                    ImplementorUpdate.WaitOn?.Reset();
                }

                ExecuteTheAction(Act, CancelationPlanImplementor);
            }



  
            GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 26, GloCla.ResMan.GetString("Sp2"));
            PlanRealized?.Invoke();
        }

        protected void RunNewImplementorTask (Task GoalToRealization)
        {
            ImplementorTask = GoalToRealization;
            ImplementorTask.Start();
        }

        internal Task RealizeIt(List<CrisscrossChildrenCon> ActionList, CancellationToken CancellationDomein)
        {
            InternalCancelationPlanImplementor = new CancellationTokenSource();
            CancellationToken CancelationPlanImplementor = CancellationTokenSource.CreateLinkedTokenSource(CancellationDomein, InternalCancelationPlanImplementor.Token).Token;

            void actions() => ActionListRealize(ActionList, CancelationPlanImplementor);

            Task NextGoalToRealization = new Task(actions, CancelationPlanImplementor);

            if (ImplementorTask is null)
            {
                RunNewImplementorTask(NextGoalToRealization);
            }
            else if (ImplementorTask.Status == TaskStatus.Canceled || 
                     ImplementorTask.Status == TaskStatus.Faulted || 
                     ImplementorTask.Status == TaskStatus.RanToCompletion)
            {
                RunNewImplementorTask(NextGoalToRealization);
            }
            else
            {
                ImplementorTask.Wait();
                RunNewImplementorTask(NextGoalToRealization);
            }

            return NextGoalToRealization;
        }
    }
}
