using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Natol.PerformanceCounter2CloudWatch.PerformanceCounters
{
    public class CpuPerformanceCounterDescriptor : PerformanceCounterDescriptor
    {
        public override double? GetCount()
        {
            return Convert.ToDouble(SystemCounter.NextValue() / Environment.ProcessorCount);
        }
    }
}
