using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.App.BusinessModels;
using _13_Wyd.App.Services;
using _13_Wyd.App.Services.Data;
using _13_Wyd.ModelClasses;
using _13_Wyd.ModelClasses.Payment.Events;
using _13_Wyd.ModelClasses.Payment.Methods;

namespace _13_Wyd.App
{
    public class WydManager
    {
        private readonly PaymentService PaymentService;

        private readonly UsersService UsersService;
        private readonly OrdersService OrdersService;
        private readonly ItemsService ItemsService;
        private readonly LogService LogService;


        public WydManager(PaymentService paymentManager, UsersService usersService,
            OrdersService ordersService, ItemsService itemsService, LogService logService)
        {
            this.PaymentService = paymentManager;
            this.UsersService = usersService;
            this.ItemsService = itemsService;
            this.OrdersService = ordersService;
            this.LogService = logService;
        }

        // Register
        
        public async Task<bool> RegisterUser(string firstName, string lastName,  //ok
                            string email, string passwordHash)
        {
            if (firstName == null || lastName == null || email == null ||
                passwordHash == null || firstName.Length == 0 ||
                lastName.Length == 0 || email.Length == 0 || passwordHash.Length == 0)
                    return false;

            UserAccount newUser = new UserAccount(null, firstName, lastName, email, passwordHash);
            UserAccount createdUser = await this.UsersService.CreateUser(newUser);

            Log newLog;
            if (createdUser == null)
                newLog = new Log($"Error registering new User Account with First Name = {firstName}, " +
                                                    $"Last Name = {lastName}, Email = {email}");
            else
                newLog = new Log(createdUser, $"Successfully registered new User Account:\n{createdUser}");

            await this.LogService.RegisterLog(newLog);

            return createdUser != null;
        }

        public async Task<bool> RegisterItem(string name, decimal cost, string seller,  //ok
                      string image, decimal price, TimeSpan period, IEnumerable<string> categories)
        {
            if (name == null || seller == null || name.Length == 0 || seller.Length == 0)
                return false;

            Item newItem = new Item(null, name, cost, seller, image, price, period, categories);
            Item createdItem = await this.ItemsService.CreateItem(newItem);

            Log newLog;
            if (createdItem == null)
                newLog = new Log($"Error creating new item with Name = {name}, Cost = {cost}, Seller = {seller}, " +
                    $"Image = {image}, Price = {price}, Period = {period}, Categories = {string.Join(", ", categories)}");
            else
                newLog = new Log(createdItem, $"Successfully created new Item:\n{createdItem}");

            await this.LogService.RegisterLog(newLog);

            return createdItem != null;
        }

        // Retrieve

        public async Task<UserAccount> GetUser(string userId)      //ok
        {
            if (userId == null)
                return null;

            UserAccount user = await this.UsersService.GetUser(userId);

            Log newLog;
            if (user == null)
                newLog = new Log($"Error retrieving User with ID = {userId}");
            else
                newLog = new Log(user, $"Successfully retrieved User with ID = {userId}");

            await this.LogService.RegisterLog(newLog);

            return user;
        }

        public async Task<BusinessUserAccount> GetShoppingUser(UserAccount user) //ok
        {
            if (user == null || user.Id == null)
                return null;

            BusinessUserAccount shoppingUser = await this.UsersService.GetShoppingUser(user.Id);

            Log newLog;
            if (shoppingUser == null)
                newLog = new Log($"Error retrieving Shopping User from User = {user}");
            else
                newLog = new Log(user, $"Successfully retrieved Shopping User: {shoppingUser}");

            await this.LogService.RegisterLog(newLog);

            return shoppingUser;
        }

        public async Task<Item> GetItem(string itemId) //ok
        {
            if (itemId == null) return null;

            Item item = await this.ItemsService.GetItem(itemId);

            Log newLog;
            if (item == null)
                newLog = new Log($"Error retrieving Item with ID = {itemId}");
            else
                newLog = new Log(item, $"Successfully retrieved Item: {item}");

            await this.LogService.RegisterLog(newLog);

            return item;
        }

        public async Task<Order> GetOrder(string orderId)   //ok
        {
            if (orderId == null)  return null;

            Order order = await this.OrdersService.GetOrder(orderId);

            Log newLog;
            if (order == null)
                newLog = new Log($"Error retrieving Order with ID = {orderId}");
            else
                newLog = new Log(order, $"Successfully retrieved Order: {order}");

            await this.LogService.RegisterLog(newLog);

            return order;
        }

        // Delete

        public async Task<bool> DeleteUser(string userId)   //ok
        {
            if (userId == null) return false;

            UserAccount deletedUser = await this.UsersService.DeleteUser(userId);
            bool correctlyDeleted = deletedUser != null;

            Log newLog;
            if (!correctlyDeleted)
                newLog = new Log($"Error deleting User with ID = {userId}");
            else
                newLog = new Log(deletedUser, $"Successfully deleted User: {deletedUser}");

            await this.LogService.RegisterLog(newLog);

            return correctlyDeleted;
        }

        public async Task<bool> DeleteItem(string itemId)   //ok
        {
            if (itemId == null)
                return false;

            Item deletedItem = await this.ItemsService.DeleteItem(itemId);
            bool correctlyDeleted = deletedItem != null;

            Log newLog;
            if (!correctlyDeleted)
                newLog = new Log($"Error deleting Item with ID = {itemId}");
            else
                newLog = new Log(deletedItem, $"Successfully deleted Item: {deletedItem}");

            await this.LogService.RegisterLog(newLog);

            return correctlyDeleted;
        }

        public async Task<bool> DeleteOrder(string orderId) //ok
        {
            if (orderId == null)
                return false;

            Order deletedOrder = await this.OrdersService.DeleteOrder(orderId);
            bool correctlyDeleted = deletedOrder != null;

            Log newLog;
            if (!correctlyDeleted)
                newLog = new Log($"Error deleting Order with ID = {orderId}");
            else
                newLog = new Log(deletedOrder, $"Successfully deleted Order: {deletedOrder}");

            await this.LogService.RegisterLog(newLog);

            return correctlyDeleted;
        }

        // Update

        public async Task<bool> UpdateUserInfo(UserAccount user)    //ok
        {
            UserAccount updatedUser = await this.UsersService.UpdateUser(user);
            bool correctlyUpdated = updatedUser != null;

            Log newLog;
            if (!correctlyUpdated)
                newLog = new Log($"Error updating User's info: {user}");
            else
                newLog = new Log(updatedUser, $"Successfully updated User's info: {updatedUser}");

            await this.LogService.RegisterLog(newLog);

            return correctlyUpdated;
        }

        public async Task<bool> UpdateShoppingUser(BusinessUserAccount businessUser) //~ok
        {
            if (businessUser == null || businessUser.Id == null)
                return false;

            bool correctlyUpdated = await this.UsersService.UpdateShoppingUser(businessUser);

            Log newLog;
            if (!correctlyUpdated)
                newLog = new Log($"Error updating Shopping User's info: User Id = {businessUser.Id}\n" +
                    $"WishList = {businessUser.Print(businessUser.WishList)}\n" +
                    $"Shopping Cart = {businessUser.Print(businessUser.ShoppingCart)}");
            else
                newLog = new Log(businessUser.UserAccount,
                    $"Successfully updated Shopping User: User Id = {businessUser.Id}\n" +
                    $"WishList = {businessUser.Print(businessUser.WishList)}\n" +
                    $"Shopping Cart = {businessUser.Print(businessUser.ShoppingCart)}");

            await this.LogService.RegisterLog(newLog);

            return correctlyUpdated;
        }

        public async Task<bool> UpdateItem(Item item)   //ok
        {
            Item updatedItem = await this.ItemsService.UpdateItem(item);
            bool correctlyUpdated = updatedItem != null;

            Log newLog;
            if (!correctlyUpdated)
                newLog = new Log($"Error updating Item's info: {item}");
            else
                newLog = new Log(item, $"Successfully updated Item's info: {updatedItem}");

            await this.LogService.RegisterLog(newLog);

            return correctlyUpdated;
        }

        public async Task<bool> CancelOrder(Order order)    //ok
        {
            if (order == null || string.IsNullOrWhiteSpace(order.Id) || order.Canceled == true)
                return false;

            Log newLog;
            //orders can be canceled only in the next 7 days
            if (order.Delivered == true || DateTime.Now - order.TimeStamp > TimeSpan.FromDays(7))
            {
                newLog = new Log(order, $"Error canceling Order with id = {order.Id}, " +
                    $"because it was already delivered or it has been processed more than 7 days ago");

                await this.LogService.RegisterLog(newLog);
                return false;
            }

            order.Canceled = true;

            Order updatedOrder = await this.OrdersService.UpdateOrder(order);
            bool correctlyCanceled = updatedOrder != null && updatedOrder.Canceled == true;

            if (!correctlyCanceled)
                newLog = new Log($"Error canceling Order with ID: {order.Id}");
            else
                newLog = new Log(order, $"Successfully canceled Order: {updatedOrder}");

            await this.LogService.RegisterLog(newLog);

            return correctlyCanceled;
        }

        // User's operations

        //add

        public async Task<bool> AddItemToWishList(Item item, UserAccount user, int qty) //ok
        {
            if (item == null || user == null || qty <= 0 || item.Id == null || user.Id == null)
                return false;

            BusinessUserAccount businessUser = new BusinessUserAccount(user);
            bool setted1 = await this.UsersService.SetUserWishList(businessUser);

            BusinessItem businessItem = new BusinessItem(item);
            bool setted2 = await this.ItemsService.SetItemQty(businessItem);

            Log newLog;
            if (!setted1 || !setted2)
            {
                newLog = new Log(user, $"Error adding item {item.Id} (n.{qty}) to User {user.Id} wishlist: " +
                    $"error setting User's Wishlist or Item's Qty in stock");

                await this.LogService.RegisterLog(newLog);

                return false;   //an error occured
            }

            if (businessUser.AddToWishList(businessItem, qty))
            {
                bool updated1 = await this.UsersService.UpdateUserWishList(businessUser);   //update user

                bool setted3 = await this.ItemsService.SetInterestedUsers(businessItem);

                if (!updated1 || !setted3)
                {
                    newLog = new Log(user, $"Error adding Item {item.Id} (n.{qty}) to User {user.Id} Wishlist: " +
                    $"error updating User's Wishlist or setting Item's Interested Users");

                    await this.LogService.RegisterLog(newLog);
                    return false;   //error occured
                }

                if (businessItem.AddInterestedUser(user))
                {
                    bool updated2 = await this.ItemsService.UpdateInterestedUsers(businessItem);   //update item

                    if (!updated2)
                    {
                        newLog = new Log(user, $"Error adding Item {item.Id} (n.{qty}) to User {user.Id} Wishlist: " +
                            $"error updating Item's Interested Users");

                        await this.LogService.RegisterLog(newLog);  
                        return false;   //error occured
                    }
                }

                newLog = new Log(user, $"Successfully added Item with Id = {item.Id} (n.{qty}) to " +
                    $"User with Id = {user.Id} Wishlist");

                await this.LogService.RegisterLog(newLog);

                return true;    //ok
            }

            return false;   //no changes
        }

        public async Task<bool> AddItemToShoppingCart(Item item, UserAccount user, int qty) //ok
        {
            if (item == null || user == null || qty < 0 || item.Id == null || user.Id == null)
                return false;

            BusinessUserAccount businessUser = new BusinessUserAccount(user);
            bool setted1 = await this.UsersService.SetUserShoppingCart(businessUser);

            BusinessItem businessItem = new BusinessItem(item);
            bool setted2 = await this.ItemsService.SetItemQty(businessItem);

            Log newLog;
            if (!setted1 || !setted2)
            {
                newLog = new Log(user, $"Error adding Item {item.Id} (n.{qty}) to User {user.Id} Shopping Cart: " +
                    $"error setting User's Shopping Cart or Item's Qty in stock");

                await this.LogService.RegisterLog(newLog);

                return false;   //error
            }

            if (businessUser.AddToShoppingCart(businessItem, qty))
            {
                bool updated1 = await this.UsersService.UpdateUserShoppingCart(businessUser);   //update user

                bool setted3 = await this.ItemsService.SetInterestedUsers(businessItem);

                if (!updated1 || !setted3)
                {
                    newLog = new Log(user, $"Error adding Item {item.Id} (n.{qty}) to User {user.Id} Shopping Cart: " +
                    $"error updating User's Shopping Cart or setting Item's Interested Users");

                    await this.LogService.RegisterLog(newLog);

                    return false;   //error occured
                }

                if (businessItem.AddInterestedUser(user))
                {
                    bool updated2 = await this.ItemsService.UpdateInterestedUsers(businessItem);   //update item

                    if (!updated2)
                    {
                        newLog = new Log(user, $"Error adding Item {item.Id} (n.{qty}) to User {user.Id} Shopping Cart: " +
                            $"error updating Item's Interested Users");

                        await this.LogService.RegisterLog(newLog);
                        return false;   //error occured
                    }
                }

                //ok

                newLog = new Log(user, $"Successfully added Item with Id = {item.Id} (n.{qty}) to " +
                    $"User with Id = {user.Id} Shopping Cart");

                await this.LogService.RegisterLog(newLog);
                return true;   
            }

            return false;
        }

        public async Task<bool> AddReview(Item item, UserAccount user, double score, string comment)    //ok
        {
            if (item == null || user == null || score < 0 || score > 5
                || comment == null || comment.Length == 0)
                return false;

            DateTime date = DateTime.Now;
            Review newReview = new Review(item.Id, user.Id, score, comment, date);

            BusinessUserAccount businessUser = new BusinessUserAccount(user);
            bool setted1 = await this.UsersService.SetUserReviews(businessUser);

            BusinessItem businessItem = new BusinessItem(item);
            bool setted2 = await this.ItemsService.SetItemReviewsAndScore(businessItem);

            Log newLog;
            if (!setted1 || !setted2)
            {
                newLog = new Log($"Error adding Review for Item = {item.Id}, by User = {user.Id}, " +
                    $"Score = {score}, Comment = {comment}: error setting User's Reviews or Item's Reviews or Item's Score");

                await this.LogService.RegisterLog(newLog);
                return false; //an error occured
            }

            businessUser.AddReview(newReview);
            businessItem.AddReview(newReview);

            bool updated = await this.UsersService.UpdateUserReviews(businessUser);

            if (updated)
                updated = await this.ItemsService.UpdateItemReviewsAndScore(businessItem);

            if (!updated) //error occured
                newLog = new Log($"Error adding Review for Item = {item.Id}, by User = {user.Id}, " +
                    $"Score = {score}, Comment = {comment}: error updating User's reviews or Item's reviews or Item's score");
            else
                newLog = new Log(user, $"Successfully added Review for Item = {item.Id}, by User = {user.Id}, " +
                    $"Score = {score}, Comment = {comment}");

            await this.LogService.RegisterLog(newLog);
            return updated;
        }

        //remove

        public async Task<bool> RemoveItemFromShoppingCart(Item item, UserAccount user) //ok
        {
            if (item == null || user == null || string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(user.Id))
                return false;

            BusinessUserAccount businessUser = await this.UsersService.GetShoppingUser(user.Id);
            BusinessItem businessItem = new BusinessItem(item);
            bool setted = await this.ItemsService.SetInterestedUsers(businessItem);

            Log newLog;
            if (businessUser == null || !setted || businessUser.ShoppingCart == null
                                    || businessUser.WishList == null)
            {
                newLog = new Log($"Error removing Item = {item.Id} from User = {user.Id} shopping cart: " +
                    $"error retrieving Shopping User or setting Item's interesting users");

                await this.LogService.RegisterLog(newLog);
                return false; //error occured
            }

            bool removed = businessUser.RemoveFromShoppingCart(item);
            bool updated1 = removed && await this.UsersService.UpdateUserShoppingCart(businessUser);

            if (!updated1)
            {
                newLog = new Log($"Error removing Item = {item.Id} from User = {user.Id} shopping cart: " +
                    $"error removing Item from User's shopping cart or updating User's shopping cart");

                await this.LogService.RegisterLog(newLog);
                return false; //error occured
            }
            
            if (!businessUser.WishList.Keys.Contains(item))
            {
                businessItem.RemoveInterestedUser(user);

                bool updated2 = await this.ItemsService.UpdateInterestedUsers(businessItem);

                if (!updated2)
                {
                    newLog = new Log($"Error removing Item = {item.Id} from User = {user.Id} shopping cart: " +
                    $"error updating Item's interested users");

                    await this.LogService.RegisterLog(newLog);
                    return false; //error occured
                }
            }

            newLog = new Log(user, $"Successfully removed Item = {item.Id} from User = {user.Id} shopping cart");
            await this.LogService.RegisterLog(newLog);
            return true;
        }

        public async Task<bool> RemoveItemFromWishList(Item item, UserAccount user) //ok
        {
            if (item == null || user == null)
                return false;

            BusinessUserAccount businessUser = await this.UsersService.GetShoppingUser(user.Id);
            BusinessItem businessItem = new BusinessItem(item);
            bool setted = await this.ItemsService.SetInterestedUsers(businessItem);

            Log newLog;
            if (businessUser == null || !setted || businessUser.ShoppingCart == null
                                    || businessUser.WishList == null)
            {
                newLog = new Log($"Error removing Item = {item.Id} from User = {user.Id} wishlist: " +
                    $"error retrieving shopping user or setting item's interesting users");

                await this.LogService.RegisterLog(newLog);
                return false; //error occured
            }

            bool removed = businessUser.RemoveFromWishList(item);
            bool updated1 = removed && await this.UsersService.UpdateUserWishList(businessUser);

            if (!updated1)
            {
                newLog = new Log($"Error removing item = {item.Id} from User = {user.Id} wishlist: " +
                    $"error removing Item from User's wishlist or updating User's wishlist");

                await this.LogService.RegisterLog(newLog);
                return false; //error occured
            }

            if (!businessUser.ShoppingCart.Keys.Contains(item))
            {
                businessItem.RemoveInterestedUser(user);

                bool updated2 = await this.ItemsService.UpdateInterestedUsers(businessItem);

                if (!updated2)
                {
                    newLog = new Log($"Error removing Item = {item.Id} from User = {user.Id} shopping cart: " +
                    $"error updating Item's interested users");

                    await this.LogService.RegisterLog(newLog);
                    return false;
                }
            }

            newLog = new Log(user, $"Successfully removed Item = {item.Id} from User = {user.Id} wishlist");
            await this.LogService.RegisterLog(newLog);
            return true;
        }

        public async Task<bool> MoveFromWishlistToShoppingCart(UserAccount user, Item itemToMove) //ok
        {
            if (user == null || itemToMove == null || string.IsNullOrWhiteSpace(user.Id) || string.IsNullOrWhiteSpace(itemToMove.Id))
                return false;

            BusinessUserAccount businessUser = await this.UsersService.GetShoppingUser(user.Id);

            bool moved = businessUser != null &&
                                businessUser.MoveFromWishlistToShoppingCart(itemToMove);
            Log newLog;
            if (!moved)
            {
                newLog = new Log($"Error moving Item = {itemToMove.Id} from Wishlist to " +
                    $"Shopping Cart of User with Id = {user.Id}: " +
                    $"error getting Shopping User or moving Item from Wishlist to Shopping cart");

                await this.LogService.RegisterLog(newLog);
                return false;
            }

            bool updated = await this.UsersService.UpdateShoppingUser(businessUser);

            if (!updated)
                newLog = new Log($"Error moving Item = {itemToMove.Id} from Wishlist to " +
                    $"Shopping Cart of User with Id = {user.Id}: " +
                    $"error updating User's Wishlist or updating User's Shopping Cart");
            else
                newLog = new Log(user, $"Successfully moved Item with Id = {itemToMove.Id} " +
                    $"from Wishlist to Shopping Cart of User with Id = {user.Id}");

            await this.LogService.RegisterLog(newLog);
            return updated;
        }

        // Make a new Order

        public async Task<bool> MakeANewOrder(UserAccount user, string shippingAddress, //ok
            PaymentType paymentType, bool gift)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id)
                || string.IsNullOrWhiteSpace(shippingAddress))
                return false;

            BusinessUserAccount businessUser = new BusinessUserAccount(user);
            bool setted1 = await this.UsersService.SetUserShoppingCart(businessUser);

            Log newLog;
            if (!setted1 || businessUser.ShoppingCart == null)
            {
                newLog = new Log($"Error making a new Order with User = {user.Id}, Shipping Address = {shippingAddress}, " +
                    $"Payment Type = {paymentType}, Gift = {gift}: error setting User's Shopping Cart");

                await this.LogService.RegisterLog(newLog);
                return false; //an error occured
            }

            if (!businessUser.ShoppingCart.Any())
            {
                newLog = new Log($"Error making a new Order with User = {user.Id}, Shipping Address = {shippingAddress}, " +
                    $"Payment Type = {paymentType}, Gift = {gift}: User's Shopping Cart is empty");

                await this.LogService.RegisterLog(newLog);
                return false; 
            }           

            //user's shopping cart is not empty here

            Dictionary<Item, int> purchasedItems = businessUser.ShoppingCart;

            //  check if quantities in shopping cart are available
            // items --> business items (transformation)
            Dictionary<BusinessItem, int> purchasedBusinessItems =
                purchasedItems.ToDictionary(pair =>
                {
                    BusinessItem businessItem = new BusinessItem(pair.Key);
                    return businessItem;
                },
                pair => pair.Value);

            bool setted2 = await this.ItemsService.SetQtyAndInterestedUsers(purchasedBusinessItems.Keys);

            if(!setted2)
            {
                newLog = new Log(user, $"Error making a new Order with: userId = {user.Id}, Shipping Address = {shippingAddress}, Payment Type = {paymentType}, gift = {gift}.\n" +
                                            $"Error updating Items' Qty and Items' Interested Users Before the Payment");

                await this.LogService.RegisterLog(newLog);
                return false;
            }

            List<Item> notAvailableItems = new List<Item>();

            foreach(var itemQtyPair in purchasedBusinessItems)
            {
                BusinessItem businessItem = itemQtyPair.Key;
                int purchasedQty = itemQtyPair.Value;

                if (businessItem.QtyInStock == null || businessItem.QtyInStock < purchasedQty)
                    notAvailableItems.Add(businessItem.Item);
            }

            if(notAvailableItems.Any())
            {
                newLog = new Log(user, $"Error making a new Order with: userId = {user.Id}, Shipping Address = {shippingAddress}, Payment Type = {paymentType}, gift = {gift}.\n" +
                                            $"Not enough quantities for the following items: {string.Join("\n", notAvailableItems)}");

                await this.LogService.RegisterLog(newLog);
                return false;
            }

            //items' quantities ok here

            decimal totCost = purchasedItems.Sum(pair =>
            {
                return (decimal)pair.Key.Price * new decimal(pair.Value);
            });

            Dictionary<string, int> purchasedItemsIDs =
                purchasedItems.ToDictionary(pair => pair.Key.Id, pair => pair.Value);
            
            Order newOrder = new Order(null, user.Id, purchasedItemsIDs, totCost,
                shippingAddress, paymentType, gift, DateTime.Now, false, false);

            // create order in db, and return it with the next OrderId
            newOrder = await this.OrdersService.CreateOrder(newOrder);

            if (newOrder == null)
            {
                newLog = new Log(user, $"Error making a new Order with User = {user.Id}, Shipping Address = {shippingAddress}, " +
                    $"Payment Type = {paymentType}, Gift = {gift}: error creating new order in DB");

                await this.LogService.RegisterLog(newLog);
                return false;  // an error occured
            }

            //  * process new order payment *
            PaymentEvent paymentCorrectlyProcessed = await this.PaymentService.ProcessPayment(
                                                    paymentType, newOrder.Id, user.Id, purchasedItems);

            if (paymentCorrectlyProcessed == null)
            {
                bool orderDeleted = await this.DeleteOrder(newOrder.Id);
                string orderDeletedInfo = orderDeleted ? "Order deleted" : "But order not properly deleted";

                newLog = new Log(newOrder, $"Error making a new Order with info: {newOrder}.\n" +
                                         $"Error processing payment. {orderDeletedInfo}");
                return false;   // an error occured
            }

            // * PAYMENT OK here -> process order *
            newLog = new Log(paymentCorrectlyProcessed, $"Successfully processed new Payment: {paymentCorrectlyProcessed}");
            await this.LogService.RegisterLog(newLog);

            //update user's orders
            bool cleared = await this.UsersService.ClearShoppingCartOf(user.Id);

            if(!cleared)
            {
                newLog = new Log(user, $"Error making a new Order with info: {newOrder}.\n" +
                                            $"Error clearing User's Shopping Cart after Payment");

                await this.LogService.RegisterLog(newLog);
            }

            bool areItemsAllUpdated = true;

            foreach (var pair in purchasedBusinessItems)
            {
                BusinessItem businessItem = pair.Key;
                businessItem.Manager = this;
                int qty = pair.Value;

                if (businessItem.QtyInStock == null || businessItem.InterestedUsers == null)
                {
                    areItemsAllUpdated = false;
                    continue;   //error occured
                }

                bool updateInterestedUsers = await businessItem.DecreaseQtyAndUpdateInterestedUsers(qty);

                if (updateInterestedUsers)
                    areItemsAllUpdated = (await this.ItemsService.UpdateInterestedUsers(businessItem)) && areItemsAllUpdated;
            }

            areItemsAllUpdated = (await this.ItemsService.UpdateItemsQuantities(purchasedBusinessItems.Keys)) && areItemsAllUpdated;

            if(!areItemsAllUpdated)
            {
                newLog = new Log(user, $"Error making a new Order with info: {newOrder}.\n" +
                                            $"Error updating Items' Qty and Items' Interested Users after Payment");

                await this.LogService.RegisterLog(newLog);
            }

            if(cleared && areItemsAllUpdated)
            {
                newLog = new Log(user, $"Successfully made a new Order: {newOrder}");
                await this.LogService.RegisterLog(newLog);
            }

            return cleared && areItemsAllUpdated;
        }    
    }
}
