using System;
using System.Collections.Generic;
using System.Linq;

namespace TestProject
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Item item = new Item("221100", "marcaDaBollo", new decimal(22.30),
                "pippofranco", null, null, new TimeSpan(0, 0, 36),
                new List<string>() { "magia", "fisi=c=a", "letteratura" });

            string format = item.ToString();
            Item parsedItem = ConvertToItem(format);

            Console.WriteLine(parsedItem);
        }

        public static Item ConvertToItem(object value)
        {
            string stringToConvert = (string)value;
            string[] itemFields = stringToConvert.Split(';');

            if (itemFields.Length != 8) return null;

            List<Tuple<string, string>> fieldsPairs = itemFields.Select(field =>
            {
                string[] pair = field.Split(new char[]{'='}, 2);
                return pair.Length == 2 ? new Tuple<string, string>(pair[0].Trim(), pair[1].Trim()) : null;
            }).ToList();

            if (fieldsPairs.Contains(null)) return null;

            if (!fieldsPairs[0].Item1.ToLower().Equals("id") ||
               !fieldsPairs[1].Item1.ToLower().Equals("name") ||
               !fieldsPairs[2].Item1.ToLower().Equals("cost") ||
               !fieldsPairs[3].Item1.ToLower().Equals("seller") ||
               !fieldsPairs[4].Item1.ToLower().Equals("image") ||
               !fieldsPairs[5].Item1.ToLower().Equals("price") ||
               !fieldsPairs[6].Item1.ToLower().Equals("period") ||
               !fieldsPairs[7].Item1.ToLower().Equals("categories"))
                return null;

            string id, name, seller, image;
            decimal? cost, price;
            TimeSpan? period;
            IEnumerable<string> categories;

            if (fieldsPairs[0].Item2.Equals(string.Empty))
                id = null;
            else
                id = fieldsPairs[0].Item2;

            if (fieldsPairs[1].Item2.Equals(string.Empty))
                name = null;
            else
                name = fieldsPairs[1].Item2;

            if (fieldsPairs[2].Item2.Equals(string.Empty))
                cost = null;
            else
                cost = decimal.Parse(fieldsPairs[2].Item2);

            if (fieldsPairs[3].Item2.Equals(string.Empty))
                seller = null;
            else
                seller = fieldsPairs[3].Item2;

            if (fieldsPairs[4].Item2.Equals(string.Empty))
                image = null;
            else
                image = fieldsPairs[4].Item2;

            if (fieldsPairs[5].Item2.Equals(string.Empty))
                price = null;
            else
                price = decimal.Parse(fieldsPairs[5].Item2);

            try
            {
                if (fieldsPairs[6].Item2.Equals(string.Empty))
                    period = null;
                else
                    period = TimeSpan.Parse(fieldsPairs[6].Item2);

                if (fieldsPairs[7].Item2.Equals(string.Empty))
                    categories = null;
                else
                    categories = fieldsPairs[7].Item2.Split(',').Select(s => s.Trim());
            }
            catch (Exception)
            {
                return null;
            }

            return new Item(id, name, cost, seller, image, price, period, categories);
        }
        
    }

    public class Item
    {
        public string Id { get; set; } //unique identifier: ITXXXX
        public string Name { get; set; }
        public decimal? Cost { get; set; }
        public string Seller { get; set; }
        public string Image { get; set; }
        public decimal? Price { get; set; }
        public TimeSpan? Period { get; set; }
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
            this.Period = period;
            this.Categories = categories;
        }

        public override string ToString()
        {
            return $"Id = {this.Id}; Name = {this.Name}; Cost = {this.Cost}; " +
                $"Seller = {this.Seller}; Image = {this.Image}; Price = {this.Price}; " +
                $"Period = {this.Period}; Categories = {string.Join(", ", this.Categories)}";
        }

        public override bool Equals(object obj)
        {
            return obj is Item item &&
                   Id == item.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<string>.Default.GetHashCode(Id);
        }
    }
}
