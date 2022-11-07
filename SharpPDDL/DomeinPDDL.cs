using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    public class DomeinPDDL
    {
        public readonly string Name;
        public TypesPDDL types;
        //public List<PredicatePDDL> predicates;
        public List<ActionPDDL> actions;

        private void CheckActions()
        {
            ActionPDDL.allTypes = this.types.allTypes;

            foreach (ActionPDDL act in actions)
            {
                act.BuildIt();
            }
        }

        private bool CheckExistingOfPredicate(string name, Type typeOf1Class, Type typeOf2Class = null)
        {
            if (!types.allTypes.Exists(t => t.type == typeOf1Class))
            {
                throw new Exception(""); //taki typ nie istnieje wsrod dodanych typow
            }

            if (typeOf2Class != null)
            {
                if (!types.allTypes.Exists(t => t.type == typeOf2Class))
                {
                    throw new Exception(""); //taki typ nie istnieje wsrod dodanych typow
                }
            }

            bool exist = false;
            foreach (KnotOfTree i in types.allTypes)
            {
                if (i.predicates.Exists(p => p.Name == name))
                {
                    exist = true;
                    break;
                }
            }
            if (exist)
                throw new Exception(); //Taki Predicate juz zostal dodany

            return true;
        }

        public void AddTypes(Type baseType, Type classes1, params Type[] classesN)
        {
            types.AddTypes(baseType, classes1, classesN);
        }

        public void AddPredicate(string name, Type typeOfClass)
        {
            if (!CheckExistingOfPredicate(name, typeOfClass))
                return;

            PredicatePDDL TempPredicatePDDL = new PredicatePDDL(name, typeOfClass);

            types.allTypes.Find(t => t.type == typeOfClass).predicates.Add(TempPredicatePDDL);
        }

        public void AddPredicate(string name, Type typeOf1Class, Type typeOf2Class)
        {
            if (!CheckExistingOfPredicate(name, typeOf1Class, typeOf2Class))
                return;

            PredicatePDDL TempPredicatePDDL = new PredicatePDDL(name, typeOf1Class, typeOf2Class);

            types.allTypes.Find(t => t.type == typeOf1Class).predicates.Add(TempPredicatePDDL);
        }

        public DomeinPDDL (string name)
        {
            this.Name = name;
            this.types = new TypesPDDL();
            //this.predicates = new List<PredicatePDDL>();
            this.actions = new List<ActionPDDL>();
        }
    }
}
