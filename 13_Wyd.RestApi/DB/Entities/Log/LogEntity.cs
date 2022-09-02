using System;
using Microsoft.Azure.Cosmos.Table;

namespace _13_Wyd.RestApi.DB.Entities.Log
{
    public class LogEntity : TableEntity
    {
        public DateTime LogTimeStamp { get; set; }
        public string Message { get; set; }


        public LogEntity() { }

        public LogEntity(ModelClasses.Log log)
        {
            this.PartitionKey = log.Id;
            this.RowKey = log.SubjectId ?? string.Empty;

            this.LogTimeStamp = log.Timestamp;
            this.Message = log.Message;
        }

        public ModelClasses.Log ToLog()
        {
            ModelClasses.Log log = new ModelClasses.Log()
            {
                Id = this.PartitionKey,
                Timestamp = this.LogTimeStamp,
                SubjectId = this.RowKey,
                Message = this.Message
            };

            return log;
        }
    }
}
