using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTPool
{
    public interface IGtpMode
    {
        bool WithWait { get; }
    }

    public class GtpSync : IGtpMode
    {
        public bool WithWait
        {
            get { return true; }
        }
    }

    public class GtpAsync : IGtpMode
    {
        public bool WithWait
        {
            get { return false; }
        }        
    }
}
