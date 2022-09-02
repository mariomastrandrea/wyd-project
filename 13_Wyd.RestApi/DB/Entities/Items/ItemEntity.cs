using System;
using System.Collections.Generic;
using _13_Wyd.ModelClasses;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace _13_Wyd.RestApi.DB.Entities.Items
{
    public class ItemEntity : TableEntity
    {
        public string Name { get; set; }
        public double? Cost { get; set; }    //converted: decimal <-> double
        public string Seller { get; set; }
        public string Image { get; set; }
        public double? Price { get; set; }   //converted: decimal <-> double
        public long? Period { get; set; }    
        public string JsonCategories { get; set; }  //converted: IEnumerable<string> <-> string


        public ItemEntity() { }

        public ItemEntity(Item item)
        {
            this.PartitionKey = item.Id;
            this.RowKey = string.Empty;

            this.Name = item.Name;
            this.Cost = (double?)item.Cost;
            this.Seller = item.Seller;
            this.Image = item.Image;
            this.Price = (double?)item.Price;
            this.Period = item.PeriodTicks;
            this.JsonCategories = item.Categories == null ? null : JsonConvert.SerializeObject(item.Categories);
        }

        public Item ToItem()
        {
            string itemId = this.PartitionKey;
            decimal? cost = (decimal?)this.Cost;
            decimal? price = (decimal?)this.Price;
            TimeSpan? period = this.Period == null ? null : TimeSpan.FromTicks((long)this.Period);

            IEnumerable<string> categories = this.JsonCategories == null ? null : 
                JsonConvert.DeserializeObject<IEnumerable<string>>(this.JsonCategories);

            Item newItem = new Item(itemId, this.Name, cost, this.Seller,
                this.Image, price, period, categories);

            return newItem;
        }
    }
}
