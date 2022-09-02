using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace _13_Wyd.ModelClasses
{
    public class UserAccount
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }


        [JsonConstructor]
        public UserAccount(string id, string firstName, string lastName,
                            string email, string passwordHash)
        {
            this.Id = id;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Email = email;
            this.PasswordHash = passwordHash;
        }

        public UserAccount() { }

        /// <summary>
        /// this method checks for equality of User IDs
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>
        /// true if <paramref name="obj"/> has the same ID of <paramref name="this"/> istance; false, otherwise
        /// </returns>
        public override bool Equals(object obj) 
        {
            return obj is UserAccount account && Id.Equals(account.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override string ToString()
        {
            return $"Id = {this.Id}, firstName = {this.FirstName}, lastName = {this.LastName}, email = {this.Email}";
        }

        public bool HasSameFieldsOf(UserAccount userToUpdate)
        {
            if (userToUpdate == null || string.IsNullOrWhiteSpace(this.Id) ||
                string.IsNullOrWhiteSpace(userToUpdate.Id) || !this.Id.Equals(userToUpdate.Id))
                return false;

            if (this.Email == null && this.FirstName == null && this.LastName == null && this.PasswordHash == null)
                return false; //nothing to update

            return (this.FirstName == null || userToUpdate.FirstName.Equals(this.FirstName)) &&
                   (this.LastName == null || userToUpdate.LastName.Equals(this.LastName)) &&
                   (this.Email == null || userToUpdate.Email.Equals(this.Email)) &&
                   (this.PasswordHash == null || userToUpdate.PasswordHash.Equals(this.PasswordHash));
        }

        public UserAccount FillWith(UserAccount userToUpdate)
        {
            if (string.IsNullOrWhiteSpace(this.Id)) return null;

            if (this.FirstName == null)
                this.FirstName = userToUpdate.FirstName;

            if (this.LastName == null)
                this.LastName = userToUpdate.LastName;

            if (this.Email == null)
                this.Email = userToUpdate.Email;

            if (this.PasswordHash == null)
                this.PasswordHash = userToUpdate.PasswordHash;

            return this;
        }
    }
}
