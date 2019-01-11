
using System;

namespace Pure.Profiler.Configuration
{
    public class PureProfilerException:Exception
    {
        public PureProfilerException(string msg):base(msg) {

        }

        public PureProfilerException(string msg, Exception ex) : base(msg, ex)
        {

        }
    }
}
