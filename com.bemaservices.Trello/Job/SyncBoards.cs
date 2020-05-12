using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace com.bemaservices.TrelloSync.Job
{
    [DisallowConcurrentExecution]
    public class SyncBoards : IJob
    {
        public void Execute( IJobExecutionContext context )
        {
            var trelloApi = new TrelloApi();

            trelloApi.GetBoards();
        }
    }
}
