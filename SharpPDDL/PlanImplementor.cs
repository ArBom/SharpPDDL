using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        internal const byte DONT_EVEN_TRY = 0b_1111;
    }

    public enum AskToAgree : byte
    {
        GO_AHEAD = Agrees.Go_AHEAD,
        //SpecialAction = Agrees.SpecialAction,
        EveryAction = Agrees.EveryAction,
        Plan = Agrees.Plan,
        DONT_DO_IT = Agrees.DONT_DO_IT
    }

    internal class PlanImplementor
    {
        internal readonly DomeinPDDL Owner;
        internal CancellationTokenSource InternalCancelationTokenSource;

        internal Task ImplementorTask { get; private set; }

        internal PlanImplementor(DomeinPDDL Owner)
        {
            this.Owner = Owner;
        }

        protected void ActionListRealize(List<CrisscrossChildrenCon> ActionList, CancellationToken cancelationToken)
        {
            GloCla.Tracer?.TraceEvent(TraceEventType.Start, 22, GloCla.ResMan.GetString("Sa2"));

            if ((Owner.ImplementorUpdate.PlanImplementor_Agrees & Agrees.DONT_DO_IT) != 0)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 25, GloCla.ResMan.GetString("Sp1"));
                return;
            }

            if ((Owner.ImplementorUpdate.PlanImplementor_Agrees & Agrees.Plan) != 0)
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 26, GloCla.ResMan.GetString("V2"));

                object[] ActionsNameArray = ActionList.Select(AL => (object)(Owner.actions[AL.ActionNr].InstantExecution.Method.Name)).ToArray();
                Owner.ImplementorUpdate.SignalizeNeedAcception?.Invoke(GloCla.PlanToAcceptation, ActionsNameArray);

                if (Owner.ImplementorUpdate.WaitOn is null)
                    GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 127, GloCla.ResMan.GetString("C41"));

                WaitHandle.WaitAny(new WaitHandle[] { Owner.ImplementorUpdate.WaitOn, cancelationToken.WaitHandle });

                if (cancelationToken.IsCancellationRequested)
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 29, GloCla.ResMan.GetString("V4"));
                    return;
                }
                else
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 27, GloCla.ResMan.GetString("V3"));
                    Owner.ImplementorUpdate.WaitOn.Reset();
                }
            }

            foreach (CrisscrossChildrenCon Act in ActionList)
            {
                if (Owner.actions[Act.ActionNr].InstantExecution is null)
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Warning, 28, GloCla.ResMan.GetString("W2"));
                    continue;
                }

                if (Owner.actions[Act.ActionNr].InstantExecution.Method.Name.StartsWith(GloCla.SpecialFuncPrefix) && (Owner.ImplementorUpdate.PlanImplementor_Agrees & Agrees.SpecialAction) != 0)
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 30, GloCla.ResMan.GetString("V5"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name.Substring(1));
                    Owner.ImplementorUpdate.SignalizeNeedAcception?.Invoke(Owner.actions[Act.ActionNr].InstantExecution.Method.Name.Substring(1), Act.ActionArgOryg);

                    if (Owner.ImplementorUpdate.WaitOn is null)
                        GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 128, GloCla.ResMan.GetString("C42"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name.Substring(1));

                    WaitHandle.WaitAny(new WaitHandle[] { Owner.ImplementorUpdate.WaitOn, cancelationToken.WaitHandle });                    
                }
                else if ((Owner.ImplementorUpdate.PlanImplementor_Agrees & Agrees.EveryAction) != 0)
                { 
                    GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 31, GloCla.ResMan.GetString("V6"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name);
                    Owner.ImplementorUpdate.SignalizeNeedAcception?.Invoke(Owner.actions[Act.ActionNr].InstantExecution.Method.Name, Act.ActionArgOryg);

                    if (Owner.ImplementorUpdate.WaitOn is null)
                        GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 129, GloCla.ResMan.GetString("C43"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name.Substring(1));

                    WaitHandle.WaitAny(new WaitHandle[] { Owner.ImplementorUpdate.WaitOn, cancelationToken.WaitHandle });                
                }

                if (cancelationToken.IsCancellationRequested)
                {
                    GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 32, GloCla.ResMan.GetString("V7"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name);
                    return;
                }
                else
                    Owner.ImplementorUpdate.WaitOn?.Reset();

                GloCla.Tracer?.TraceEvent(TraceEventType.Verbose, 33, GloCla.ResMan.GetString("V8"), Owner.actions[Act.ActionNr].InstantExecution.Method.Name);

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

                if(!(GloCla.Tracer is null))
                    Owner.actions[Act.ActionNr].InstantExecutionChecker.DynamicInvoke(Act);        

                Owner.CurrentState = Act.Child.Content;
                Owner.DomainPlanner.RemoveRealizedGoalsOfCrisscross(Act.Child);
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
