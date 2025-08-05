using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPDDL
{
    internal class PlanImplementor
    {
        internal readonly DomeinPDDL Owner;
        internal CancellationTokenSource InternalCancelationTokenSource;

        internal Task ImplementorTask { get; private set; }

        internal PlanImplementor(DomeinPDDL Owner)
        {
            this.Owner = Owner;
        }

        protected void WaitActionsListAgrees(List<CrisscrossChildrenCon> ActionList, CancellationToken cancelationToken)
        {
            if ((Owner.ImplementorUpdate.PlanImplementor_Agrees & ExecutionAgrees.Plan) == 0)
                return;

            GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 26, GloCla.ResMan.GetString("V2"));

            object[] ActionsNameArray = ActionList.Select(AL => (object)(Owner.actions[AL.ActionNr].InstantExecution.Method.Name)).ToArray();
            Owner.ImplementorUpdate.SignalizeNeedAcception?.Invoke(GloCla.PlanToAcceptation, ActionsNameArray);

            if (Owner.ImplementorUpdate.WaitOn is null)
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 127, GloCla.ResMan.GetString("C41"));

            WaitHandle.WaitAny(new WaitHandle[] { Owner.ImplementorUpdate.WaitOn, cancelationToken.WaitHandle });

            if (cancelationToken.IsCancellationRequested)
                GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 29, GloCla.ResMan.GetString("V4"));
            else
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 27, GloCla.ResMan.GetString("V3"));
                Owner.ImplementorUpdate.WaitOn.Reset();
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
            if (Owner.actions[Act.ActionNr].InstantExecution.Method.Name.StartsWith(GloCla.SpecialFuncPrefix) && (Owner.ImplementorUpdate.PlanImplementor_Agrees & ExecutionAgrees.SpecialAction) != 0)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 30, GloCla.ResMan.GetString("V5"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name.Substring(1));
                Owner.ImplementorUpdate.SignalizeNeedAcception?.Invoke(Owner.actions[Act.ActionNr].InstantExecution.Method.Name.Substring(1), Act.ActionArgOryg);

                if (Owner.ImplementorUpdate.WaitOn is null)
                    GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 128, GloCla.ResMan.GetString("C42"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name.Substring(1));

                WaitHandle.WaitAny(new WaitHandle[] { Owner.ImplementorUpdate.WaitOn, cancelationToken.WaitHandle });
                return true;
            }
            else if ((Owner.ImplementorUpdate.PlanImplementor_Agrees & ExecutionAgrees.EveryAction) != 0)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 31, GloCla.ResMan.GetString("V6"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name);
                Owner.ImplementorUpdate.SignalizeNeedAcception?.Invoke(Owner.actions[Act.ActionNr].InstantExecution.Method.Name, Act.ActionArgOryg);

                if (Owner.ImplementorUpdate.WaitOn is null)
                    GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 129, GloCla.ResMan.GetString("C43"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name.Substring(1));

                WaitHandle.WaitAny(new WaitHandle[] { Owner.ImplementorUpdate.WaitOn, cancelationToken.WaitHandle });
                return true;
            }
            else
                return false;
        }

        protected void ExecuteTheAction(CrisscrossChildrenCon Act, CancellationToken cancelationToken)
        {
            int ExecutionTaskId = -1;

            try
            {
                Task ExecutionTask = new Task(() =>
                {
                    Owner.actions[Act.ActionNr].InstantExecution.DynamicInvoke(Act.ActionArgOryg);
                }, cancelationToken);

                if (!(GloCla.Tracer is null))
                {
                    ExecutionTaskId = ExecutionTask.Id;
                    GloCla.Tracer.TraceEvent(TraceEventType.Start, 34, GloCla.ResMan.GetString("Sa3"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name, ExecutionTaskId);
                }

                ExecutionTask.RunSynchronously();
            }
            catch (PrecondExecutionException UnexpectedPrecondition)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 23, GloCla.ResMan.GetString("W1"), UnexpectedPrecondition.Data[PrecondExecutionException.ActionName], UnexpectedPrecondition.Data[PrecondExecutionException.ExecutionPreconditionName]);
                throw UnexpectedPrecondition;
                //TODO no i co dalej?
            }
            catch (Exception exception)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 24, GloCla.ResMan.GetString("C4"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name, exception.ToString());
                throw exception;
            }
            finally
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 35, GloCla.ResMan.GetString("Sp3"), ExecutionTaskId);
            }

            if (!(GloCla.Tracer is null))
                Owner.actions[Act.ActionNr].InstantExecutionChecker.DynamicInvoke(Act);
        }

        protected void ActionListRealize(List<CrisscrossChildrenCon> ActionList, CancellationToken cancelationToken)
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 22, GloCla.ResMan.GetString("Sa2"));

            if ((Owner.ImplementorUpdate.PlanImplementor_Agrees & ExecutionAgrees.DONT_DO_IT) != 0)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 25, GloCla.ResMan.GetString("Sp1"));
                return;
            }

            WaitActionsListAgrees(ActionList, cancelationToken);

            if (cancelationToken.IsCancellationRequested)
                return;

            foreach (CrisscrossChildrenCon Act in ActionList)
            {
                if (Owner.actions[Act.ActionNr].InstantExecution is null)
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 28, GloCla.ResMan.GetString("W2"));
                    continue;
                }

                bool NeedToWait = WaitTheActionAgree(Act, cancelationToken);

                if (cancelationToken.IsCancellationRequested)
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 32, GloCla.ResMan.GetString("V7"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name);
                    return;
                }

                if (NeedToWait)
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 33, GloCla.ResMan.GetString("V8"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name);
                    Owner.ImplementorUpdate.WaitOn?.Reset();
                }

                ExecuteTheAction(Act, cancelationToken);

                List<GoalPDDL> RealizedGoals = Owner.DomainPlanner.RemoveRealizedGoalsOfCrisscross(Act.Child);
                if (!(RealizedGoals is null))
                {
                    //for every realized goal...
                    foreach (GoalPDDL goalPDDL in RealizedGoals)
                    {
                        //...check every GoalObject of its
                        foreach (IGoalObject goalObject in goalPDDL.GoalObjects)
                        {
                            //ignore if its not migrate
                            if (!goalObject.MigrateIntheEnd)
                                continue;

                            //ignore if its removed early
                            if (!Owner.domainObjects.Contains(goalObject.OriginalObj))
                                continue;

                            //move oryginal obj. to new domain if its needed
                            if (!(goalObject.NewPDDLdomain is null))
                                goalObject.NewPDDLdomain.domainObjects.Add(goalObject.OriginalObj);

                            //remove it from here
                            Owner.domainObjects.Remove(goalObject.OriginalObj);
                        }
                    }
                }

                Owner.CurrentState = Act.Child.Content;
            }

            GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 26, GloCla.ResMan.GetString("Sp2"));
        }

        internal Task RealizeIt(List<CrisscrossChildrenCon> ActionList, CancellationToken ExternalCancellationToken)
        {
            InternalCancelationTokenSource = new CancellationTokenSource();
            var PlanImplementorTokens = CancellationTokenSource.CreateLinkedTokenSource(ExternalCancellationToken, InternalCancelationTokenSource.Token).Token;

            void actions() => ActionListRealize(ActionList, PlanImplementorTokens);
            ImplementorTask = Task.Factory.StartNew(actions, PlanImplementorTokens);
            return ImplementorTask;
        }
    }
}
