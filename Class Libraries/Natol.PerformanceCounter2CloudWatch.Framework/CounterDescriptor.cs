using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Natol.PerformanceCounter2CloudWatch.Framework
{
    public abstract class CounterDescriptor
    {
        public CounterDescriptor()
        {
            Dimensions = new Dictionary<string, string>();
        }

        public string Name { get; set; }
        public string Unit { get; set; }
        public string MetricName { get; set; }
        public IDictionary<string,string> Dimensions { get; set; }
        public abstract double GetCount();
    }
}
