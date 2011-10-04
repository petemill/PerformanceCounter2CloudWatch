using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Natol.PerformanceCounter2CloudWatch.Framework
{
    public interface IPerformanceCounterLister
    {
        IList<CounterDescriptor> UpdateCounterItems(IList<CounterDescriptor> counterItems);
    }
}
