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

        internal static readonly string correctExtension = ".dgml";
        protected abstract string MakeFilePath(string prefix);

        protected const string Node_Key = "Node";
        protected const string Link_Key = "Link";
        protected const string Property_Key = "Property";

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

        protected const string Id_Key = "Id";
        protected const string Background_Key = "Background";
        protected const string Category_Key = "Category";
        protected const string Contains_Key = "Contains";
        protected const string DataType_Key = "DataType";
        protected const string Label_Key = "Label";
        protected const string Source_Key = "Source";
        protected const string Stroke_Key = "Stroke";
        protected const string Target_Key = "Target";

        protected const string Class_Colour = "#FF0E70C0";
        protected const string State_Colour = "#FF770056";
        protected const string Action_Colour = "#FF8E0C10";
        protected const string Realization_Colour = "#FFFF7600";

        void Close() => writer.WriteEndElement();

        protected void AddRecord(string Type, Dictionary<string, string> atributes)
        {
            writer.WriteStartElement(Type);

            foreach (var atr in atributes)
                writer.WriteAttributeString(atr.Key, atr.Value);

            Close();
        }

        protected void AddTFStyle(string target, string valueToCheck, bool IsTrue, string propertyToSet, string newValue)
        {
            string IsTrueString = IsTrue.ToString();

            writer.WriteStartElement("Style");
            writer.WriteAttributeString("TargetType", target);
            writer.WriteAttributeString("GroupLabel", valueToCheck);
            writer.WriteAttributeString("ValueLabel", IsTrueString);

            AddRecord("Condition", new Dictionary<string, string> { ["Expression"] = $"{valueToCheck} = '{IsTrueString}'" });
            AddRecord("Setter", new Dictionary<string, string> { ["Property"] = propertyToSet, ["Value"] = newValue });
            writer.WriteEndElement();
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

            string ch_path = MakeFilePath(path);
            writer = XmlWriter.Create(ch_path, settings);

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
