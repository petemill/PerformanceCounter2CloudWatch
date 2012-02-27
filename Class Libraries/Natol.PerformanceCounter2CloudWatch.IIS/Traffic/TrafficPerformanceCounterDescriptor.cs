using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Natol.PerformanceCounter2CloudWatch.PerformanceCounters;

namespace Natol.PerformanceCounter2CloudWatch.IIS.Traffic
{
    public class TrafficPerformanceCounterDescriptor : PerformanceCounterDescriptor
    {
        public override double? GetCount()
        {
            var baseCount = base.GetCount();
            if (!baseCount.HasValue || baseCount.Value == 0)
                return null;
            
            return baseCount;
        }
    }
}
