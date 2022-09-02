using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;

namespace _13_Wyd.App.BusinessModels
{
    public class BusinessItem   // wrapper of Item
    {
        public Item Item { get; }

        public string Id         { get => Item.Id;          set => Item.Id = value; }
        public string Name       { get => Item.Name;        set => Item.Name = value; }
        public decimal? Cost     { get => Item.Cost;        set => Item.Cost = value; }
        public string Seller     { get => Item.Seller;      set => Item.Seller = value; }
        public string Image      { get => Item.Image;       set => Item.Image = value; }
        public decimal? Price    { get => Item.Price;       set => Item.Price = value; }
        public long? PeriodTicks { get => Item.PeriodTicks; set => Item.PeriodTicks = value; }
        public IEnumerable<string> Categories { get => Item.Categories; set => Item.Categories = value; }

        //business properties
        public int? QtyInStock { get; set; } 
        public double? AvgScore { get; set; } //from 0 to 5  
        public List<Review> Reviews { get; set; }  
        public List<UserAccount> InterestedUsers { get; set; }

        public WydManager Manager { get; set; }


        public BusinessItem() { }

        public BusinessItem(string id, string name, decimal? cost, string seller,
                     int? qtyInStock, string image, decimal? price,
                     TimeSpan? period, IEnumerable<string> categories, double? avgScore,
                     List<Review> reviews, List<UserAccount> interestedUsers)
        {
            this.Item = new Item();

            this.Id = id;
            this.Name = name;
            this.Cost = cost;
            this.Seller = seller;
            this.Image = image;
            this.Price = price;
            this.PeriodTicks = period == null ? null : ((TimeSpan)period).Ticks;
            this.Categories = categories;

            this.QtyInStock = qtyInStock;
            this.AvgScore = avgScore;
            this.Reviews = reviews;
            this.InterestedUsers = interestedUsers;
        }

        public BusinessItem(Item item)
        {
            this.Item = item;
        }

        public void AddReview(Review newReview)
        {
            this.Reviews.Add(newReview);

            double tot = 0.0;
            foreach (Review r in this.Reviews)
                tot += r.Score;

            double newAvgScore = tot / (double)this.Reviews.Count;
            this.AvgScore = newAvgScore;
        }

        public async Task<bool> DecreaseQtyAndUpdateInterestedUsers(int purchasedQty)
        {
            if (purchasedQty > this.QtyInStock)
                throw new ArgumentOutOfRangeException();

            if (this.Manager == null)
                throw new Exception("You must initialize WydManager first");

            this.QtyInStock -= purchasedQty;

            List<UserAccount> noLongerInterestedUsers = new List<UserAccount>();

            //update Users' Shopping Carts And WishLists quantities
            foreach (UserAccount user in this.InterestedUsers)
            {
                BusinessUserAccount businessUser = await this.Manager.GetShoppingUser(user);

                if (businessUser == null) continue;   //connection error

                bool updated = false;

                if (businessUser.WishList.ContainsKey(this.Item))
                    if (businessUser.WishList[this.Item] > this.QtyInStock)
                    {
                        businessUser.WishList.Remove(this.Item);

                        if(this.QtyInStock > 0)
                            businessUser.WishList.Add(this.Item, (int)this.QtyInStock);

                        updated = true;
                    }

                if (businessUser.ShoppingCart.ContainsKey(this.Item))
                    if (businessUser.ShoppingCart[this.Item] > this.QtyInStock)
                    {
                        businessUser.ShoppingCart.Remove(this.Item);

                        if(this.QtyInStock > 0)
                            businessUser.ShoppingCart.Add(this.Item, (int)this.QtyInStock);

                        updated = true;
                    }

                if (updated)
                    await this.Manager.UpdateShoppingUser(businessUser);

                if (!businessUser.ShoppingCart.ContainsKey(this.Item) &&
                        !businessUser.WishList.ContainsKey(this.Item))
                    noLongerInterestedUsers.Add(user);
            }

            if(!noLongerInterestedUsers.Any())
                return false;   //interestedUsers NOT changed

            foreach (UserAccount u in noLongerInterestedUsers)
                this.InterestedUsers.Remove(u);

            return true;    //interestedUsers changed
        }

        public bool AddInterestedUser(UserAccount user)
        {
            if (this.InterestedUsers.Contains(user))
                return false;   //nothing changes

            this.InterestedUsers.Add(user);
            return true;
        }


        public bool RemoveInterestedUser(UserAccount user)
        {
            return this.InterestedUsers.Remove(user);
        }

        public void Update(BusinessItem newItem)
        {
            string newName = newItem.Name;
            decimal? newCost = newItem.Cost;
            string newSeller = newItem.Seller;
            int? newQtyInStock = newItem.QtyInStock;
            string newImage = newItem.Image;
            decimal? newPrice = newItem.Price;
            long? newPeriodTicks = newItem.PeriodTicks;
            IEnumerable<string> newCategories = newItem.Categories;
            double? newAvgScore = newItem.AvgScore;
            List<Review> newReviews = newItem.Reviews;
            List<UserAccount> newInterestedUsers = newItem.InterestedUsers;

            if (newName != null && newName.Length > 0)
                this.Name = newName;

            if (newCost != null)
                this.Cost = newCost;

            if (newSeller != null && newSeller.Length > 0)
                this.Seller = newSeller;

            if (newQtyInStock != null)
                this.QtyInStock = newQtyInStock;

            if (newImage != null)
                this.Image = newImage;

            if (newPrice != null)
                this.Price = newPrice;

            if (newPeriodTicks != null)
                this.PeriodTicks = newPeriodTicks;

            if (newCategories != null)
                this.Categories = newCategories;

            if (newAvgScore != null)
                this.AvgScore = newAvgScore;

            if (newReviews != null)
                this.Reviews = newReviews;

            if (newInterestedUsers != null)
                this.InterestedUsers = newInterestedUsers;
        }

        /// <summary>
        /// this method checks for equality of BusinessItem IDs
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>
        /// true, if <paramref name="obj"/> has the same ID of <paramref name="this"/> istance; false, otherwise
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is BusinessItem item &&
                   Id == item.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.Item.ToString());

            sb.Append($"\nQty in stock: {this.QtyInStock}");
            sb.Append($"\nAvg score: {this.AvgScore}");
            sb.Append("\nReviews: ");

            if (this.Reviews == null)
                sb.Append("(null)");
            else if (!this.Reviews.Any())
                sb.Append("(empty)");
            else
                sb.Append('\n').Append(string.Join('\n', this.Reviews));

            sb.Append("\nInterested users: ");

            if (this.InterestedUsers == null)
                sb.Append("(null)");
            else if (!this.InterestedUsers.Any())
                sb.Append("(none)");
            else
                sb.Append(this.InterestedUsers.Count);

            return sb.ToString();
        }
    }
}
