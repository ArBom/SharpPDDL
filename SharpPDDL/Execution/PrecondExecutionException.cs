using System;

namespace SharpPDDL
{
    internal class PrecondExecutionException : Exception
    {
        internal const string ActionName = "ActionName";
        internal const string ExecutionPreconditionName = "ExecutionPrecondition";

        public PrecondExecutionException(string ActionName, string ExecutionPreconditionName) : base("Unexpected value in time of trying realize " + ActionName + " action; unfulfil " + ExecutionPreconditionName + " precondition")
        {
            Data.Add(PrecondExecutionException.ActionName, ActionName);
            Data.Add(PrecondExecutionException.ExecutionPreconditionName, ExecutionPreconditionName);
        }
    }
}