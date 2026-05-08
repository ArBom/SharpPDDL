using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SharpPDDL
{
    [Flags]
    public enum Diagram : short
    {
        None = 0,
        Class = 1,
        UseCase = 2,
        States = 4
    }

    internal abstract class DGML
    {
        protected XmlWriter writer;
        protected abstract string GraphTitle();
        protected abstract string GraphLayout();

        void OpenGraph(string title, string Layout)
        {
            writer.WriteStartElement("DirectedGraph", @"http://schemas.microsoft.com/vs/2009/dgml");
            writer.WriteAttributeString("Title", title);
            writer.WriteAttributeString("Layout", Layout);
        }

        protected abstract void CreateData();

        protected const string NodeName = "Node";
        protected const string LinkName = "Link";
        protected const string CategoryName = "Category";
        protected const string PropertyName = "Property";
        protected const string StyleName = "Style";

        void OpenNodes() => writer.WriteStartElement("Nodes");
        void OpenLinks() => writer.WriteStartElement("Links");
        void OpenCategories() => writer.WriteStartElement("Categories");
        void OpenProperties() => writer.WriteStartElement("Properties");
        void OpenStyles() => writer.WriteStartElement("Styles");

        internal abstract void AddNodes();
        internal abstract void AddLinkes();
        internal abstract void AddCategories();
        internal abstract void AddProperties();
        internal abstract void AddStyles();

        void Close() => writer.WriteEndElement();

        protected void AddRecord(string Type, Dictionary<string, string> atributes)
        {
            writer.WriteStartElement(Type);

            foreach (var atr in atributes)
                writer.WriteAttributeString(atr.Key, atr.Value);

            Close();
        }

        internal Task MakeGraphTask(string path, CancellationToken cancelationToken)
        {
            Task task = new Task(() => MakeGraph(path), cancelationToken);
            task.Start();
            return task;
        }

        internal void MakeGraph(string path)
        {
            CreateData();

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = " ",
            };

            writer = XmlWriter.Create(path, settings);

            OpenGraph(GraphTitle(), GraphLayout());

            OpenNodes();
            AddNodes();
            Close();

            OpenLinks();
            AddLinkes();
            Close();

            OpenCategories();
            AddCategories();
            Close();

            OpenProperties();
            AddProperties();
            Close();

            OpenStyles();
            AddStyles();
            Close();

            writer.Flush();
            writer.Close();
        }
    }
}
