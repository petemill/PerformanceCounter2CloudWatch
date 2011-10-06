using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Natol.PerformanceCounter2CloudWatch.Framework;
using System.Diagnostics;

namespace Natol.PerformanceCounter2CloudWatch.PerformanceCounters
{
    public class PerformanceCounterLister : IPerformanceCounterLister
    {
        public IList<CounterDescriptor> UpdateCounterItems(IList<CounterDescriptor> counterItems)
        {
            //no need to update, since we're hardcoded (& eventually configuration-vased)
            if (counterItems.Count == 0)
            {
                var pc = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                var result = new PerformanceCounterDescriptor
                {
                    Name = "CPUUtilization",
                    SystemCounter = pc,
                    Unit = "Percent",
                    MetricName = "CPUUtilization"
                };
                counterItems.Add(result);
            }

            return counterItems;
        }
    }
}
