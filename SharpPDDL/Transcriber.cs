using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPDDL
{
    internal class Transcriber
    {
        //New root of states
        internal readonly Crisscross NewOne;

        //List of domain actions
        private readonly IReadOnlyList<ActionPDDL> Actions;

        //Crisscroses without child in prev. run, spots to start run alg.
        internal readonly SortedSet<Crisscross> ChildlessCrisscrosses;

        //List of all Crisscrosses in NewOne
        internal readonly SortedList<string, Crisscross> NewIndexedStates;

        //Collection of Crisscrosses to transcribe
        private SortedSet<KeyValuePair<Crisscross, List<CrisscrossChildrenCon>>> NotTranscribedChildYet;

        //List of Crisscrosses pined into NewOne with alternatives Root, candidates to reduce
        private List<Crisscross> PossibleToCrisscrossReduce;

        internal Transcriber(Crisscross NewRoot, List<ActionPDDL> actions)
        {
            this.Actions = actions.AsReadOnly();

            List<ThumbnailObject> NewThumbnailObjects = new List<ThumbnailObject>();
            foreach (ThumbnailObject TO in NewRoot.Content.ThumbnailObjects)
                NewThumbnailObjects.Add(new ThumbnailObjectPrecursor<object>(TO, false) as ThumbnailObject);

            PossibleState NewOneContent = new PossibleState(NewThumbnailObjects);
            NewOne = new Crisscross(NewOneContent);

            NotTranscribedChildYet = new SortedSet<KeyValuePair<Crisscross, List<CrisscrossChildrenCon>>>(new TupleCo())
            {
                new KeyValuePair<Crisscross, List<CrisscrossChildrenCon>>(NewOne, NewRoot.Children)
            };

            ChildlessCrisscrosses = new SortedSet<Crisscross>(Crisscross.SortCumulativedTransitionCharge());

            PossibleToCrisscrossReduce = new List<Crisscross>() { NewOne };

            NewIndexedStates = new SortedList<string, Crisscross>
            {
                { NewOne.Content.CheckSum, NewOne }
            };
        }

        private Crisscross TranscribeCrisscross(Crisscross WorkWithIt, CrisscrossChildrenCon CurrentCrisscrossCon)
        {
            //Array of thumbnail to check
            ThumbnailObject[] SetToCheck = new ThumbnailObject[CurrentCrisscrossCon.ActionArgThOb.Length];
            //making array of thumbnail to check out
            for (int i = 0; i != CurrentCrisscrossCon.ActionArgThOb.Length; i++)
                SetToCheck[i] = WorkWithIt.Content.ThumbnailObjects.First(TO => TO.OriginalObj.Equals(CurrentCrisscrossCon.ActionArgThOb[i].OriginalObj));

            //try to recheck action possibility:
            object ResultOfCheck = Actions[CurrentCrisscrossCon.ActionNr].InstantActionPDDLSimplified.DynamicInvoke(SetToCheck);

            //- if its NOT OK - skip it
            if (ResultOfCheck is null)
                return null;

            //- otherwise - cast result to correct type...
            var ResultOfCheckasList = (List<List<KeyValuePair<ushort, ValueType>>>)ResultOfCheck;
            //...and create a list with new ThumbnailObject(s)
            var ChangedThObs = new List<ThumbnailObject>();
            for (int j = 0; j < SetToCheck.Length; j++)
            {
                ThumbnailObject ChangedThumbnailObject = SetToCheck[j].CreateChild(ResultOfCheckasList[j]);
                ChangedThObs.Add(ChangedThumbnailObject);
            }

            //then create new possible state as a child of action arg...
            PossibleState possibleState = new PossibleState(WorkWithIt.Content, ChangedThObs);
            //...add it to action arg...
            WorkWithIt.Add(possibleState, CurrentCrisscrossCon.ActionNr, SetToCheck, CurrentCrisscrossCon.ActionCost, out Crisscross AddedItem);
            //...and return it packaged into new Crisscross
            return AddedItem;
        }

        private void AddCrisscrosToNextRunList(Crisscross WorkWithIt, CrisscrossChildrenCon CurrentCrisscrossCon, Crisscross NewAddedItem)
        {
            if (NewAddedItem is null)
                return;

            //check NewAddedItem is indexed
            if (NewIndexedStates.Any(s => s.Key == NewAddedItem.Content.CheckSum))
                if (NewIndexedStates.Any(s => s.Value.Content.Compare(ref NewAddedItem.Content)))
                    //if so - skip it
                    return;

            //index NewAddedItem
            NewIndexedStates[NewAddedItem.Content.CheckSum] = NewAddedItem;

            //if the parent Crisscross have many possible way of exist...
            if (WorkWithIt.AlternativeRoots.Any())
            {
                lock (PossibleToCrisscrossReduce)
                {
                    //...its possible that NewAddedItem can be connected next
                    PossibleToCrisscrossReduce.Add(NewAddedItem);
                }
            }

            //check if there are any child state to transcribe...
            if (CurrentCrisscrossCon.Child.Children.Any())
                NotTranscribedChildYet.Add(new KeyValuePair<Crisscross, List<CrisscrossChildrenCon>>(NewAddedItem, CurrentCrisscrossCon.Child.Children));
            //...or they should be just created next
            else
                ChildlessCrisscrosses.Add(NewAddedItem);
        }

        private Task TranscribeOne(CancellationToken cancellationToken)
        {
            Task TranscribeTask = new Task(() =>
            {
                //until there are Crisscrosses to transcribe
                while (NotTranscribedChildYet.Any() && !cancellationToken.IsCancellationRequested)
                {
                    //take one of them...
                    KeyValuePair<Crisscross, List<CrisscrossChildrenCon>> keyValuePair = NotTranscribedChildYet.First();
                    Crisscross WorkWithIt = keyValuePair.Key;

                    //...transcribe all its child...
                    foreach (CrisscrossChildrenCon C in keyValuePair.Value)
                    {
                        Crisscross AddedItem = TranscribeCrisscross(WorkWithIt, C);
                        AddCrisscrosToNextRunList(WorkWithIt, C, AddedItem);
                    }

                    //...remove it
                    NotTranscribedChildYet.Remove(keyValuePair);
                }

                //if in the end there is no state without child...
                if (!ChildlessCrisscrosses.Any())
                    //...add the new root
                    ChildlessCrisscrosses.Add(NewOne);
            });

            TranscribeTask.Start();

            return TranscribeTask;
        }

        internal Task TranscribeState(CancellationToken cancellationToken)
        {
            Task Transcribing = new Task(() =>
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Start, 122, GloCla.ResMan.GetString("Sa10"));

                //Start task of transcribing
                Task transcribeOne = TranscribeOne(cancellationToken);

                CancellationTokenSource InternalCancellationCrisscrossReducer = new CancellationTokenSource();
                CancellationTokenSource ReducerToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, InternalCancellationCrisscrossReducer.Token);

                AutoResetEvent autoResetEvent = new AutoResetEvent(true);
                //Create local reducer...
                CrisscrossReducer crisscrossReducer = new CrisscrossReducer(NewOne, autoResetEvent, PossibleToCrisscrossReduce, new Object(), null, null);
                //...with IndexedStates out side of task
                crisscrossReducer.IndexedStates = NewIndexedStates;
                crisscrossReducer.Start(ReducerToken.Token);

                //Action of checking of the reducing complete
                void CheckAllReduced()
                {
                    autoResetEvent.Set();

                    //if any to work - stil run
                    if (PossibleToCrisscrossReduce.Any())
                        return;

                    //otherwise - cancel it
                    InternalCancellationCrisscrossReducer.Cancel();
                }

                //wait for transcibing end...
                transcribeOne.Wait();
                //...then add checking of the reducing complete
                crisscrossReducer.NoNewData = new Action(CheckAllReduced);
                //run it one more time...
                autoResetEvent.Set();
                //...and wait for the end
                crisscrossReducer.BuildingNewCrisscross.Wait();

                if (cancellationToken.IsCancellationRequested)
                    ChildlessCrisscrosses.UnionWith(NotTranscribedChildYet.Select(NT => NT.Key));                

                GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 123, GloCla.ResMan.GetString("Sp10"));            
            });

            Transcribing.Start();

            return Transcribing;
        }
    }
}
