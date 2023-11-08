using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using SharpPDDL;

namespace User
{
    class A
    {
        public int v;
    }

    class B1 : A
    {
        public char char1;
        public int char2;

        public int Inti (string a)
        {
            return 100;
        }
    }

    class B2 : A
    {
        public bool a;
    }

    class C1 : B1
    { }

    class D1 : C1
    {
        public bool LambdaF()
        {
            return true;
        }

        public Action<D1> ase = ((d) => 
        {
            d.char1 = 'a';
            d.char2 = 'b';
        });

        public Expression<Predicate<D1>> expression = d => d.LambdaF();
    }

    class E1 : D1
    { }

    class Program
    {
        static void Main(string[] args)
        {
            DomeinPDDL newDomein = new DomeinPDDL("nowa");

            newDomein.types.AddTypes(typeof(object), typeof(A));
            newDomein.AddTypes(typeof(C1), typeof(D1));
            newDomein.types.AddTypes(typeof(A), typeof(B1), typeof(B2), typeof(C1));
            newDomein.AddTypes(typeof(D1), typeof(E1));

            //newDomein.types.BuildTree();

            newDomein.AddPredicate("a", typeof(B2));
            newDomein.AddPredicate("second", typeof(B2));

            Func<C1, B2, bool> TheSame = (x, y) => x.v == y.v;
            newDomein.AddPredicate("thesame", typeof(C1), typeof(B2));

            Act.Tryp();

            Console.ReadKey();
        }
    }
}
