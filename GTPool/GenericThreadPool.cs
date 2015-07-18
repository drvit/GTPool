using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTPool
{
    public sealed class GenericThreadPool
    {
        private static readonly GenericThreadPool _instance = new GenericThreadPool();

        static GenericThreadPool() { }

        private GenericThreadPool() { }

        public static GenericThreadPool Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}
