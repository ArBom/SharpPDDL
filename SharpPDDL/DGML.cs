using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace SharpPDDL
{
    internal abstract class DGML
    {
        protected XmlWriter writer;
        protected abstract string GraphTitle();

        void OpenGraph(string title)
        {
            writer.WriteStartElement("DirectedGraph", @"http://schemas.microsoft.com/vs/2009/dgml");
            writer.WriteAttributeString("Title", title);
        }

        protected abstract void CreateData();

        protected const string NodeName = "Node";
        protected const string LinkName = "Link";
        protected const string CategoryName = "Category";
        protected const string PropertyName = "Property";

        void OpenNodes() => writer.WriteStartElement("Nodes");
        void OpenLinks() => writer.WriteStartElement("Links");
        void OpenCategories() => writer.WriteStartElement("Categories");
        void OpenProperties() => writer.WriteStartElement("Properties");

        internal abstract void AddNodes();
        internal abstract void AddLinkes();
        internal abstract void AddCategories();
        internal abstract void AddProperties();

        void Close() => writer.WriteEndElement();

        protected string CheckPath(string path)
        {
            if (String.IsNullOrEmpty(path))
            {

            }
            else
            {
                bool incorect = Path.HasExtension(path);
            }

            return path;
        }

        protected void AddRecord(string Type, Dictionary<string, string> atributes)
        {
            writer.WriteStartElement(Type);

            foreach (var atr in atributes)
                writer.WriteAttributeString(atr.Key, atr.Value);

            Close();
        }

        internal void MakeGraph(string path)
        {
            string CorrectPath = CheckPath(path);

            CreateData();

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = " ",
            };

            writer = XmlWriter.Create(CorrectPath, settings);

            OpenGraph(GraphTitle());

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

            writer.Flush();
            writer.Close();
        }
    }
}
