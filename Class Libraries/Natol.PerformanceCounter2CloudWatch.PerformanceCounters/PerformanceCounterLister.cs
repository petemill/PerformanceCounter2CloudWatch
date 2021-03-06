﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Natol.PerformanceCounter2CloudWatch.Framework;
using System.Diagnostics;

namespace Natol.PerformanceCounter2CloudWatch.PerformanceCounters
{
    public class PerformanceCounterLister : IPerformanceCounterLister
    {
        /// <summary>
        /// This instance does not requires update as the list of PerformanceCounter references is obtained through hard-coding
        /// </summary>
        public bool RequiresUpdate
        {
            get { return false; }
        }

        public IList<CounterDescriptor> UpdateCounterItems(IList<CounterDescriptor> counterItems)
        {
            //no need to update, since we're hardcoded (& eventually configuration-based)
            if (counterItems.Count == 0)
            {
                //create reference to PerformanceCounter object
                var pc = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                //read value to start counter
                pc.NextValue();

                //setup descriptor object for central framework
                var result = new CpuPerformanceCounterDescriptor
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
