using System;
using _13_Wyd.ModelClasses;
using Microsoft.Azure.Cosmos.Table;

namespace _13_Wyd.RestApi.DB.Entities.Users
{
    public class UserEntity : TableEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }


        public UserEntity() { }

        public UserEntity(UserAccount user)
        {
            this.PartitionKey = user.Id;
            this.RowKey = user.Email;

            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.PasswordHash = user.PasswordHash;
        }

        public UserAccount ToUserAccount()
        {
            string userId = this.PartitionKey;
            string userEmail = this.RowKey;

            return new UserAccount(userId, this.FirstName, this.LastName,
                    userEmail, this.PasswordHash);
        }
    }
}
