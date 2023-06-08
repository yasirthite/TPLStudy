using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowPipeline
{
    public static class ExtensionMethods
    {
        internal static void WaitForAll(this DataProcessFlow[] dataProcessFlows)
        {
            foreach (var process in dataProcessFlows)
            {
                process.Wait();
            }
        }
    }
}
