using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Natol.PerformanceCounter2CloudWatch.Framework;
using System.Diagnostics;

namespace Natol.PerformanceCounter2CloudWatch.PerformanceCounters
{
    public class PerformanceCounterDescriptor : CounterDescriptor
    {
        public PerformanceCounter SystemCounter { get; set; }

        public  override double? GetCount()
        {
            try
            {
                return Convert.ToDouble(SystemCounter.NextValue());
            }
            catch
            {
                return null;
            }
        }
    }
}
