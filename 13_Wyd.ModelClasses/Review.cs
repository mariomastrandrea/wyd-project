using System;
namespace _13_Wyd.ModelClasses
{
    public class Review
    {
        public string ItemId { get; set; }
        public string UserId { get; set; }
        public double Score { get; set; } //from 0 to 5
        public string Comment { get; set; }
        public DateTime Date { get; set; }


        public Review(string itemId, string userId, double score, string comment, DateTime date)
        {
            this.ItemId = itemId;
            this.UserId = userId;
            this.Score = score;
            this.Comment = comment;
            this.Date = date;
        }

        public override string ToString()
        {
            return $"Item: {ItemId}, User: {UserId}, Score: {Score}, Comment: {Comment}, Date: {Date}";
        }

        public override bool Equals(object obj)
        {
            return obj is Review review &&
                   ItemId == review.ItemId &&
                   UserId == review.UserId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ItemId, UserId);
        }
    }
}
