using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.DB.Tables.Log;

namespace _13_Wyd.RestApi.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly LogTS LogTable;

        public LogRepository(LogTS logTable)
        {
            this.LogTable = logTable;
        }

        public async Task<Log> CreateLog(Log newLog)
        {
            if (newLog == null || string.IsNullOrWhiteSpace(newLog.Id)
                || string.IsNullOrWhiteSpace(newLog.Message))
                return null;

            Log createdLog = await this.LogTable.CreateLog(newLog);
            return createdLog;
        }

        public async Task<IEnumerable<Log>> GetTopLogs(int num)
        {
            if (num <= 0) return null;

            IEnumerable<Log> topLogs = await this.LogTable.GetTopLogs(num);
            return topLogs;
        }
    }
}
