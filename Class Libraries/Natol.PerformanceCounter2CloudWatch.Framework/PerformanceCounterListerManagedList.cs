using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Natol.PerformanceCounter2CloudWatch.Framework
{
    public class PerformanceCounterListerManagedList
    {
        public IPerformanceCounterLister Lister { get; set; }
        public IList<CounterDescriptor> List { get; set; }
    }
}
