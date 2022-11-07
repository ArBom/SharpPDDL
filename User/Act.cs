using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpPDDL;

namespace User
{

    public static class Act
    {
        public static void SetParameterValue<T>(out T destination)
        {
            Console.WriteLine("typeof(T)=" + typeof(T).FullName);
            destination = Activator.CreateInstance<T>();
            Int32 t = destination.GetHashCode();
        }

        public static void Tryp()
        {
            C1 c1;
            D1 d1;

            c1 = default;

            ActionPDDL actionPDDL = new ActionPDDL("first");
            
            actionPDDL.AddParameter(out B1 b1);

            actionPDDL.AddPrecondiction("second", ref b1, ref c1.char2); 

            char c = 'b';
            actionPDDL.AddPrecondiction("third", ref b1, c);

            //var t = Tester.RunTheMethod(b1.inti);


            //actionPDDL.Parameters = new List<object> {out b1, out c1, out d1};
            /*
            var c = nameof(b1);



                /*

                actionPDDL.Preconition = new List<PredicatePDDL>();
                actionPDDL.Effects = new List<PredicatePDDL>();*/

        }
    }
}
