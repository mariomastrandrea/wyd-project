using System;
using _13_Wyd.ModelClasses.Payment.Events;

namespace _13_Wyd.ModelClasses
{
    public class Log
    {
        public string Id { get; set; }  //inverted dateTime ticks + guid
        public DateTime Timestamp { get; set; }

        public string SubjectId { get; set; }   //ItemId/UserId/OrderId/PaymentId
        public string Message { get; set; }


        public Log() { }

        public Log(string message)
        {
            this.Timestamp = DateTime.Now;
            long invertedTicks = DateTime.MaxValue.Ticks - this.Timestamp.Ticks;
            string newGuid = Guid.NewGuid().ToString();
            this.Id = $"LO-{invertedTicks:D19}-{newGuid}";
            this.Message = message;
        }

        public Log(UserAccount user, string message) : this(message)
        {
            this.SubjectId = user.Id;
        }

        public Log(Item item, string message) : this(message)
        {
            this.SubjectId = item.Id;
        }

        public Log(Order order, string message) : this(message)
        {
            this.SubjectId = order.Id;
        }

        public Log(PaymentEvent payment, string message) : this(message)
        {
            this.SubjectId = payment.PaymentId;
        }

        public override string ToString()
        {
            return $"Log N. {this.Id}\nSubject: {this.SubjectId}\nTimestamp: {this.Timestamp}\nMessage:\n{this.Message}\n";
        }
    }
}
