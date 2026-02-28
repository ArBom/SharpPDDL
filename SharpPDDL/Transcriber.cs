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

        private readonly List<ActionPDDL> Actions;

        //Crisscroses without child in prev. run, spots to start run alg.
        internal readonly SortedSet<Crisscross> ChildlessCrisscrosses;

        //List of all Crisscrosses in NewOne
        internal readonly SortedList<string, Crisscross> NewIndexedStates;

        //Collection of Crisscrosses waiting to pin into NewOne "tree"
        //private SortedSet<KeyValuePair<uint, Crisscross>> NotTranscribedChildYet;

        private SortedSet<KeyValuePair<Crisscross, List<CrisscrossChildrenCon>>> NotTranscribedChildYet;

        //List of Crisscrosses pined into NewOne with alternatives Root, candidates to reduce
        private List<Crisscross> PossibleToCrisscrossReduce;

        internal Transcriber(Crisscross NewRoot, List<ActionPDDL> actions)
        {
            this.Actions = actions;

            List<ThumbnailObject> NewThumbnailObjects = new List<ThumbnailObject>();
            foreach (ThumbnailObject TO in NewRoot.Content.ThumbnailObjects)
                NewThumbnailObjects.Add(new ThumbnailObjectPrecursor<object>(TO, false) as ThumbnailObject);

            NewOne = new Crisscross
            {
                Content = new PossibleState(NewThumbnailObjects)
            };

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

        private Task TranscribeOne(CancellationToken cancellationToken)
        {
            Task TranscribeTask = new Task(() =>
            {
                while (NotTranscribedChildYet.Any() && !cancellationToken.IsCancellationRequested)
                {
                    KeyValuePair<Crisscross, List<CrisscrossChildrenCon>> keyValuePair = NotTranscribedChildYet.First();
                    Crisscross WorkWithIt = keyValuePair.Key;

                    foreach (CrisscrossChildrenCon C in keyValuePair.Value)
                    {
                        ThumbnailObject[] SetToCheck = new ThumbnailObject[C.ActionArgThOb.Length];

                        for (int i = 0; i != C.ActionArgThOb.Length; i++)
                            SetToCheck[i] = WorkWithIt.Content.ThumbnailObjects.First(TO => TO.OriginalObj.Equals(C.ActionArgThOb[i].OriginalObj));

                        object ResultOfCheck = Actions[C.ActionNr].InstantActionPDDLSimplified.DynamicInvoke(SetToCheck);

                        if (ResultOfCheck is null)
                            continue;

                        var ResultOfCheckasList = (List<List<KeyValuePair<ushort, ValueType>>>)ResultOfCheck;
                        var ChangedThObs = new List<ThumbnailObject>();

                        for (int j = 0; j < SetToCheck.Length; j++)
                        {
                            var ChangedThumbnailObject = SetToCheck[j].CreateChild(ResultOfCheckasList[j]);
                            ChangedThObs.Add(ChangedThumbnailObject);
                        }

                        PossibleState possibleState = new PossibleState(WorkWithIt.Content, ChangedThObs);
                        WorkWithIt.Add(possibleState, C.ActionNr, SetToCheck, C.ActionCost, out Crisscross AddedItem);

                        if (NewIndexedStates.Any(s => s.Key == AddedItem.Content.CheckSum))
                        {
                            if (NewIndexedStates.Any(s => s.Value.Content.Compare(ref AddedItem.Content)))
                                continue;
                        }

                        if (WorkWithIt.AlternativeRoots.Any())
                        {
                            lock (PossibleToCrisscrossReduce)
                            {
                                PossibleToCrisscrossReduce.Add(AddedItem);
                            }
                        }
                        else
                        {
                            try
                            {
                                NewIndexedStates.Add(AddedItem.Content.CheckSum, AddedItem);
                            }
                            catch { }
                        }

                        if (C.Child.Children.Any())
                            NotTranscribedChildYet.Add(new KeyValuePair<Crisscross, List<CrisscrossChildrenCon>>(AddedItem, C.Child.Children));
                        else
                            ChildlessCrisscrosses.Add(AddedItem);

                    }

                    NotTranscribedChildYet.Remove(keyValuePair);
                }
            });

            TranscribeTask.Start();

            return TranscribeTask;
        }

        internal Task TranscribeState(CancellationToken cancellationToken)
        {
            Task Transcribing = new Task(() =>
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Start, 122, GloCla.ResMan.GetString("Sa10"));
                AutoResetEvent autoResetEvent = new AutoResetEvent(true);
                object CrisscrossReduceLocker = new Object();
                CrisscrossReducer crisscrossReducer = new CrisscrossReducer(NewOne, autoResetEvent, PossibleToCrisscrossReduce, CrisscrossReduceLocker, null, null);
                crisscrossReducer.IndexStates(NewIndexedStates);

                //Task transcribeOne = TranscribeOne(cancellationToken);
                crisscrossReducer.Start(cancellationToken);
                //transcribeOne.Wait();
                ChildlessCrisscrosses.Add(NewOne);

                if (cancellationToken.IsCancellationRequested)
                    ChildlessCrisscrosses.UnionWith(NotTranscribedChildYet.Select(NT => NT.Key));                

                GloCla.Tracer?.TraceEvent(TraceEventType.Stop, 123, GloCla.ResMan.GetString("Sp10"));            
            }, cancellationToken);

            Transcribing.Start();

            return Transcribing;
        }
    }
}
