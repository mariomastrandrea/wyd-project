using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;

namespace _13_Wyd.RestApi.Repositories
{
    public interface ILogRepository
    {
        Task<Log> CreateLog(Log newLog);
        Task<IEnumerable<Log>> GetTopLogs(int num);
    }
}
