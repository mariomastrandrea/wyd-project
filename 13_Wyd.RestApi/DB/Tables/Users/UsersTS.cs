using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.DB.Entities.Users;
using AzureTableStorage.TableQueryAsync;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Users
{
    public class UsersTS
    {
        private CloudTableClient TableStorageClient;
        private CloudTable Table;
        private readonly string TABLE_NAME;


        public UsersTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            TABLE_NAME = configuration.GetSection("TableStorage")
                                      .GetSection("Tables")
                                      .GetValue<string>("Users");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<UserAccount> GetUser(string userId)
        {
            if (userId == null || userId.Length == 0) return null;

            var query = this.Table.CreateQuery<UserEntity>()
                                                .Where(row => row.PartitionKey.Equals(userId))
                                                .AsTableQuery();

            IEnumerable<UserEntity> result = await query.ExecuteAsync();

            if (result == null || !result.Any()) return null;

            if (result.Count() > 1)
                throw new Exception("Unexpectedly retrieved more than one Item");

            UserEntity entityRetrieved = result.First();

            return entityRetrieved.ToUserAccount();
        }

        public async Task<UserAccount> GetUserByEmail(string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail)) return null;

            var query = this.Table.CreateQuery<UserEntity>()
                                   .Where(row => row.RowKey.Equals(userEmail))
                                   .AsTableQuery();

            IEnumerable<UserEntity> usersEntities = await query.ExecuteAsync();

            if (usersEntities == null || !usersEntities.Any()) return null;

            if(usersEntities.Count() > 1)
                throw new Exception("Unexpectedly retrieved more than one Item");

            UserEntity interestingEntity = usersEntities.First();

            return interestingEntity.ToUserAccount();
        }

        public async Task<IEnumerable<UserAccount>> GetUsers()
        {
            var query = this.Table.CreateQuery<UserEntity>();

            IEnumerable<UserEntity> result = await query.ExecuteAsync();

            if (result == null) return null;

            IEnumerable<UserAccount> allUsers = result.Select(entity => entity.ToUserAccount());
            return allUsers;
        }

        public async Task<IEnumerable<UserAccount>> GetUsers(IEnumerable<string> usersIDs)
        {
            /*
            if (userIDs == null) return null;

            var query = this.Table.CreateQuery<UserEntity>()
                                .Where(row => userIDs.Contains(row.PartitionKey))   //it doesn't works 
                                .AsTableQuery();

            IEnumerable<UserEntity> result = await query.ExecuteAsync();

            if (result == null)
                return null;

            IEnumerable<UserAccount> searchedUsers = result.Select(entity => entity.ToUserAccount());
            return searchedUsers;
            */

            List<UserAccount> searchedUsers = new List<UserAccount>();

            foreach (string userId in usersIDs)
            {
                UserAccount user = await this.GetUser(userId);

                if (user == null) return null;  //an error occurred retrieving an user -> quit

                searchedUsers.Add(user);
            }

            return searchedUsers;
        }

        public async Task<IEnumerable<UserAccount>> SearchUsers(Expression<Func<UserEntity, bool>> filter)
        {
            var query = this.Table.CreateQuery<UserEntity>()
                                            .Where(filter).AsTableQuery();

            IEnumerable<UserEntity> result = await query.ExecuteAsync();

            if (result == null) return null;

            IEnumerable<UserAccount> searchedUsers = result.Select(entity => entity.ToUserAccount());
            return searchedUsers;
        }

        public async Task<UserAccount> Create(UserAccount newUser)
        {
            if (newUser == null || string.IsNullOrWhiteSpace(newUser.Id))
                return null;

            UserEntity newEntity = new UserEntity(newUser);

            TableOperation insertOperation = TableOperation.Insert(newEntity);
            TableResult result = await this.Table.ExecuteAsync(insertOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            UserEntity createdEntity = result.Result as UserEntity;
            return createdEntity.ToUserAccount();
        }

        public async Task<UserAccount> Delete(string userId)   
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            UserAccount userToDelete = await this.GetUser(userId);

            if (userToDelete == null) return null; //there is no User with ID = {userID}

            UserEntity entityToDelete = new UserEntity(userToDelete)
            {
                ETag = "*"
            };

            TableOperation deleteOperation = TableOperation.Delete(entityToDelete);
            TableResult result = await this.Table.ExecuteAsync(deleteOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            UserAccount deletedUser = (result.Result as UserEntity).ToUserAccount();
            return deletedUser;
        }

        public async Task<UserAccount> Update(UserAccount newUser)  
        {
            if (newUser == null || string.IsNullOrWhiteSpace(newUser.Id))
                return null;

            UserAccount userToUpdate = await this.GetUser(newUser.Id);

            if (userToUpdate == null) return null;

            if (newUser.Email != null && !newUser.Email.Equals(userToUpdate.Email))
            {
                //email must be changed -> delete old row with old email and insert a new row with new email
                UserAccount deletedOldUser = await this.Delete(userToUpdate.Id);

                if (deletedOldUser == null) return null; //an error occurred

                deletedOldUser.Email = newUser.Email;
                UserAccount updatedEmailUser = deletedOldUser;

                updatedEmailUser = await this.Create(updatedEmailUser);

                if (updatedEmailUser == null) return null; //an error occurred
            }
            else
            {
                //email must not to be changed  (maintain the same email)
                newUser.Email = userToUpdate.Email;
            }

            UserEntity entityToMerge = new UserEntity(newUser)
            {
                ETag = "*"
            };

            TableOperation mergeOperation = TableOperation.Merge(entityToMerge);
            TableResult result = await this.Table.ExecuteAsync(mergeOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            UserEntity updatedEntity = result.Result as UserEntity;
            UserAccount updatedUser = updatedEntity.ToUserAccount().FillWith(userToUpdate);

            return updatedUser;
        }
    }
}
