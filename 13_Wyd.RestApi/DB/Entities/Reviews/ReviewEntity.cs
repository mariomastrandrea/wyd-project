using System;
using _13_Wyd.ModelClasses;
using Microsoft.Azure.Cosmos.Table;

namespace _13_Wyd.RestApi.DB.Entities.Reviews
{
    public class ReviewEntity : TableEntity
    {
        public double Score { get; set; } //from 0 to 5
        public string Comment { get; set; }
        public DateTime Date { get; set; }

        public ReviewEntity() { }

        public ReviewEntity(Review review)
        {
            this.PartitionKey = review.ItemId;
            this.RowKey = review.UserId;

            this.Score = review.Score;
            this.Comment = review.Comment;
            this.Date = review.Date;
        }

        public Review ToReview()
        {
            Review newReview = new Review(this.PartitionKey,
                        this.RowKey, this.Score, this.Comment, this.Date);

            return newReview;
        }
    }
}
