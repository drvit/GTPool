using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            IInterfaceA<List<int>> iA = new ClassA<List<int>>();
            var ret = iA.Get(new List<int> { 3, 4, 5, 6 });

            ret.Add(7);
            foreach (var r in ret)
            {
                System.Console.Write(r + ", ");
            }

            
        }
    }
}
