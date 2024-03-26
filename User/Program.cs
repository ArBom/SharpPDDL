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
            var watch = System.Diagnostics.Stopwatch.StartNew();

            DomeinPDDL newDomein = new DomeinPDDL("nowa");

            Func<C1, B2, bool> TheSame = (x, y) => x.v == y.v;

            //Act.Tryp();

            C1 c1;
            D1 d1;

            c1 = default;
            //d1 = new D1();
            d1 = null;

            ActionPDDL actionPDDL = new ActionPDDL("first");

            char c = 'b';
            //actionPDDL.AddPrecondiction("third", ref b1, c);

            Expression<Predicate<D1>> funcX = (d => d.v != 65);
            Expression<Predicate<D1>> funcY = (d => d.LambdaF());
            Expression<Predicate<D1>> func = (d => d.char2 > 'a' && d.char2 < 'p');

            Expression<Func<D1, ValueType>> ef = (d => d.char1);

            //Expression<Action<D1>> block = R => R.char2 = R.char1; //operator przypisania
            /*Expression<Action<D1>> blocko = Expression.Block
            (
                Expression.Assign(c1.char2, c1.char1),
                Expression.PostIncrementAssign(c1.char2)
            );*/

            actionPDDL.AddPrecondiction("fourth", ref d1, func);
            actionPDDL.AddEffect("Przypisanie", 'v', ref d1, ef);

            newDomein.AddAction(actionPDDL);
            //newDomein.actions.Add(actionPDDL);
            newDomein.CheckActions();

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Time in millis: "+ elapsedMs);

            Console.ReadKey();
        }
    }
}
