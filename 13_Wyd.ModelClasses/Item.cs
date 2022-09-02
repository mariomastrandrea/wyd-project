using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace _13_Wyd.ModelClasses
{
    [TypeConverter(typeof(StringToItemConverter))]
    public class Item
    {
        public string Id { get; set;  } //unique identifier: ITXXXX
        public string Name { get; set; }
        public decimal? Cost { get; set; }
        public string Seller { get; set; }
        public string Image { get; set; }
        public decimal? Price { get; set; }
        public long? PeriodTicks { get; set; }
        public IEnumerable<string> Categories { get; set; }


        public Item() { }

        public Item(string id, string name, decimal? cost, string seller,
                     string image, decimal? price, TimeSpan? period,
                     IEnumerable<string> categories)
        {
            this.Id = id;
            this.Name = name;
            this.Cost = cost;
            this.Seller = seller;
            this.Image = image;
            this.Price = price;
            this.PeriodTicks = period == null ? null : ((TimeSpan)period).Ticks;
            this.Categories = categories;
        }

        /// <summary>
        /// it returns true if obj has the same ID of this istance; false, otherwise
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Item item && Id.Equals(item.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override string ToString()
        {
            string timeSpanStr = this.PeriodTicks == null ? "(null)" : TimeSpan.FromTicks((long)this.PeriodTicks).ToString();
            string categoriesStr = this.Categories == null ? "(null)" : string.Join(", ", this.Categories);
            return $"Id = {this.Id}; Name = {this.Name}; Cost = {this.Cost}; " +
                $"Seller = {this.Seller}; Image = {this.Image}; Price = {this.Price}; " +
                $"Period = {timeSpanStr}; Categories = {categoriesStr}";
        }

        public bool HasSameFieldsOf(Item itemToUpdate)
        {
            if (itemToUpdate == null || string.IsNullOrWhiteSpace(itemToUpdate.Id) ||
                string.IsNullOrWhiteSpace(this.Id) || !this.Id.Equals(itemToUpdate.Id))
                return false;

            if (this.Name == null && this.Cost == null && this.Seller == null && this.Image == null &&
                this.Price == null && this.PeriodTicks == null && this.Categories == null)
                return false;

            return (this.Name == null || this.Name.Equals(itemToUpdate.Name)) &&
                   (this.Cost == null || this.Cost == itemToUpdate.Cost) &&
                   (this.Seller == null || this.Seller.Equals(itemToUpdate.Seller)) &&
                   (this.Image == null || this.Image.Equals(itemToUpdate.Image)) &&
                   (this.Price == null || this.Price == itemToUpdate.Price) &&
                   (this.PeriodTicks == null || this.PeriodTicks == itemToUpdate.PeriodTicks) &&
                   (this.Categories == null || HaveSameCategories(this.Categories, itemToUpdate.Categories));
        }

        public Item FillWith(Item itemToUpdate)
        {
            if (string.IsNullOrWhiteSpace(this.Id))
                return null;

            if (this.Name == null)
                this.Name = itemToUpdate.Name;

            if (this.Cost == null)
                this.Cost = itemToUpdate.Cost;

            if (this.Seller == null)
                this.Seller = itemToUpdate.Seller;

            if (this.Image == null)
                this.Image = itemToUpdate.Image;

            if (this.Price == null)
                this.Price = itemToUpdate.Price;

            if (this.PeriodTicks == null)
                this.PeriodTicks = itemToUpdate.PeriodTicks;

            if (this.Categories == null)
                this.Categories = itemToUpdate.Categories;

            return this;
        }

        public static bool HaveSameCategories(IEnumerable<string> categories1, IEnumerable<string> categories2)
        {
            if (categories1 == null || categories2 == null || categories1.Count() != categories2.Count())
                return false;

            return categories1.All(category1 => categories2.Any(category2 => category1.Equals(category2)));
        }
    }
}
