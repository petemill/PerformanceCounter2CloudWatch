using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Natol.PerformanceCounter2CloudWatch.Framework;
using System.Diagnostics;

namespace Natol.PerformanceCounter2CloudWatch.IIS
{
    public class IisWorkerProcessCpuCounterDescriptor : CounterDescriptor
    {
        public int ProcessId { get; set; }
        public PerformanceCounter SystemCounter { get; set; }

        public override double GetCount()
        {
            return Convert.ToDouble(SystemCounter.NextValue() / Environment.ProcessorCount);
        }
    }
}
