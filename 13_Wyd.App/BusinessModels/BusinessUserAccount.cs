using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using _13_Wyd.ModelClasses;

namespace _13_Wyd.App.BusinessModels
{
    public class BusinessUserAccount    // wrapper of UserAccount
    {
        public UserAccount UserAccount { get; }

        public string Id            { get => UserAccount.Id;           set => UserAccount.Id = value; }
        public string FirstName     { get => UserAccount.FirstName;    set => UserAccount.FirstName = value; }
        public string LastName      { get => UserAccount.LastName;     set => UserAccount.LastName = value; }
        public string Email         { get => UserAccount.Email;        set => UserAccount.Email = value; }
        public string PasswordHash  { get => UserAccount.PasswordHash; set => UserAccount.PasswordHash = value; }

        // business logic properties
        public Dictionary<Item, int> WishList { get; set; } 
        public Dictionary<Item, int> ShoppingCart { get; set; } 
        public Dictionary<string, Order> Orders { get; set; }   
        public List<Review> Reviews { get; set; }   


        [JsonConstructor]
        public BusinessUserAccount(string id, string firstName, string lastName,
                            string email, string passwordHash,
                            Dictionary<Item, int> wishList, Dictionary<Item, int> shoppingCart,
                            Dictionary<string, Order> orders, List<Review> reviews)
        {
            this.UserAccount = new UserAccount();

            this.Id = id;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Email = email;
            this.PasswordHash = passwordHash;

            this.WishList = wishList;
            this.ShoppingCart = shoppingCart;
            this.Orders = orders;
            this.Reviews = reviews;
        }

        public BusinessUserAccount(UserAccount user)
        {
            this.UserAccount = user;
        }

        public BusinessUserAccount() { }

        public bool AddToWishList(BusinessItem businessItem, int qty)
        {
            if (qty <= 0 || businessItem.QtyInStock < qty)
                return false;

            Item item = businessItem.Item;

            if (this.WishList.ContainsKey(item))
            {
                this.WishList.Remove(item, out int oldQty);
                this.WishList.Add(item, oldQty + qty);
            }
            else
                this.WishList.Add(item, qty);

            return true;
        }

        public bool AddToShoppingCart(BusinessItem businessItem, int qty)
        {
            if (qty <= 0 || businessItem.QtyInStock < qty)
                return false;

            Item item = businessItem.Item;

            if (this.ShoppingCart.ContainsKey(item))
            {
                this.ShoppingCart.Remove(item, out int oldQty);
                this.ShoppingCart.Add(item, oldQty + qty);
            }
            else
                this.ShoppingCart.Add(item, qty);

            return true;
        }

        public void AddReview(Review newReview)
        {
            this.Reviews.Add(newReview);
        }

        public bool RemoveFromShoppingCart(Item item)
        {
            return this.ShoppingCart.Remove(item);
        }

        public bool RemoveFromWishList(Item item)
        {
            return this.WishList.Remove(item);
        }

        public void AddNewOrder(Order newOrder)
        {
            this.Orders.Add(newOrder.Id, newOrder);
            this.ShoppingCart.Clear();
        }

        public bool MoveFromWishlistToShoppingCart(Item item)
        {
            if (item == null)
                return false;

            bool removed = this.WishList.Remove(item, out int qty);

            if (!removed || qty == 0)
                return false;

            if (!this.ShoppingCart.ContainsKey(item))
                this.ShoppingCart.Add(item, qty);
            else
            {
                this.ShoppingCart.Remove(item, out int oldQty);
                this.ShoppingCart.Add(item, oldQty + qty);
            }

            return true;
        }

        public void Update(BusinessUserAccount newUser)
        {
            string newFirstName = newUser.FirstName;
            string newLastName = newUser.LastName;
            string newEmail = newUser.Email;
            string newPasswordHash = newUser.PasswordHash;
            Dictionary<Item, int> newWishList = newUser.WishList;
            Dictionary<Item, int> newShoppingCart = newUser.ShoppingCart;
            Dictionary<string, Order> newOrders = newUser.Orders;
            List<Review> newReviews = newUser.Reviews;

            if (newFirstName != null && newFirstName.Length > 0)
                this.FirstName = newFirstName;

            if (newLastName != null && newLastName.Length > 0)
                this.LastName = newLastName;

            if (newEmail != null && newEmail.Length > 0)
                this.Email = newEmail;

            if (newPasswordHash != null && newPasswordHash.Length > 0)
                this.PasswordHash = newPasswordHash;

            if (newWishList != null)
                this.WishList = newWishList;

            if (newShoppingCart != null)
                this.ShoppingCart = newShoppingCart;

            if (newOrders != null)
                this.Orders = newOrders;

            if (newReviews != null)
                this.Reviews = newReviews;
        }

        /// <summary>
        /// this method checks for equality of BusinessUser IDs
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>
        /// true, if <paramref name="obj"/> has the same ID of <paramref name="this"/> istance; false, otherwise
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is BusinessUserAccount account &&
                   Id == account.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.UserAccount.ToString());

            sb.Append("\nWishList: ");

            if (this.WishList == null)
                sb.Append("null");
            else if (!this.WishList.Any())
                sb.Append("(empty)\n");
            else
                sb.Append(this.Print(this.WishList));


            sb.Append("\nShoppingCart: ");

            if (this.ShoppingCart == null)
                sb.Append("null");
            else if (!this.ShoppingCart.Any())
                sb.Append("(empty)\n");
            else
                sb.Append(this.Print(this.ShoppingCart));


            sb.Append("\nOrders: ");

            if (this.Orders == null)
                sb.Append("null");
            else if (!this.Orders.Any())
                sb.Append("(empty)");
            else
                sb.Append(this.Print(this.Orders));

            sb.Append("\nReviews: ");

            if (this.Reviews == null)
                sb.Append("(null)");
            else if (!this.Reviews.Any())
                sb.Append("(empty)");
            else
                sb.Append('\n').Append(this.Print(this.Reviews));

            return sb.ToString();
        }

        public string Print(Dictionary<Item, int> itemList)
        {
            return string.Join(", ", this.ShoppingCart.Select(pair => $"{pair.Key.Id}: {pair.Value}"));
        }

        public string Print(Dictionary<string, Order> orderList)
        {
            return string.Join(", ", this.Orders.Keys);
        }

        public string Print(IEnumerable<Review> reviewList)
        {
            return string.Join('\n', this.Reviews);
        }
    }
}
