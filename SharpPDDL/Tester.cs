using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    public static class Tester
    {
        public static int Method1(string input)
        {
            //... do something
            return 10;
        }

        public static int Method2(string input)
        {
            //... do something different
            return 1;
        }

        public static bool RunTheMethod(Func<string, int> myMethodName)
        {
            //... do stuff
            var q = myMethodName.Target.GetHashCode();
            var r = myMethodName.Target.ToString();


            int i = myMethodName("My String");
            //... do more stuff
            return true;
        }

        public static bool Test()
        {
            return RunTheMethod(Method1);
        }
    }
}
