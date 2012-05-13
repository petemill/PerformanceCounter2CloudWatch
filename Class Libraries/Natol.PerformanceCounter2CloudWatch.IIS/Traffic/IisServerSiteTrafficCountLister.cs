using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Natol.PerformanceCounter2CloudWatch.Framework;
using Microsoft.Web.Administration;
using Natol.PerformanceCounter2CloudWatch.PerformanceCounters;
using System.Diagnostics;

namespace Natol.PerformanceCounter2CloudWatch.IIS.Traffic
{
    public class IisServerSiteTrafficCountLister : IPerformanceCounterLister
    {
        #region IPerformanceCounterLister Members

        public IList<CounterDescriptor> UpdateCounterItems(IList<CounterDescriptor> counterItems)
        {
            //get sites from IIS
            ServerManager iisManager = new ServerManager();
            var sites = iisManager.Sites;

            //build list of names no longer used
            var countersToRemove = new List<string>();
            foreach (var counterItem in counterItems)
            {
                if (!sites.Any(rd => rd.Name==counterItem.Name))
                    countersToRemove.Add(counterItem.Name);
            }
            //remove all contained in removal list
            counterItems = counterItems.Where(item => !countersToRemove.Contains(item.Name)).ToList();

            //add new
            foreach (var iisSite in sites)
            {
                //if we don't have an item with the same name in our list, add it
                if (!counterItems.Any(rd => rd.Name==iisSite.Name))
                {
                    counterItems.Add(CreateDescriptor(iisSite));
                }
            }

            //total
            if (!counterItems.Any(rd => rd.Name == "Total"))
            {
                counterItems.Add(CreateAllMethodRequestsDescriptor());
            }

            //return
            return counterItems;
        }

        private static CounterDescriptor CreateAllMethodRequestsDescriptor()
        {
            var pc = new PerformanceCounter("Web Service", "Total Method Requests/sec", "_Total", true);
            pc.NextValue();

            var pcd = new PerformanceCounterDescriptor
            {
                Name = "Total",
                SystemCounter = pc,
                Unit = "Count/Second",
                MetricName = "Method Requests"
            };

            pcd.Dimensions.Add("SiteName", "Total");

            return pcd;
        }

        private static CounterDescriptor CreateDescriptor(Site iisSite)
        {
            var pc = new PerformanceCounter("Web Service", "Total Method Requests/sec", iisSite.Name, true);
            pc.NextValue();

            var pcd = new TrafficPerformanceCounterDescriptor
            {
                Name=iisSite.Name,
                SystemCounter=pc,
                Unit = "Count/Second",
                MetricName="Method Requests"
            };

            pcd.Dimensions.Add("SiteName",iisSite.Name);

            return pcd;
        }

        public bool RequiresUpdate
        {
            get { return true; }
        }

        #endregion
    }
}
