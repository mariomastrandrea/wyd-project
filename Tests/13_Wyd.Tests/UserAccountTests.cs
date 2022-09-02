using System;
using System.Threading.Tasks;
using _13_Wyd.App;
using Microsoft.AspNetCore.Components;
using Xunit;
using Xunit.DependencyInjection;

namespace _13_Wyd.Tests.Manager
{
    public class UserAccountTests
    {
        private readonly WydManager Manager;

        public UserAccountTests(WydManager manager)
        {
            this.Manager = manager;
        }

        [Theory]
        [InlineData("Pippo","Rossi","pippoverdi@pippo.com","abcdeabcdeabcde")]
        public async Task RegisterUser_rightValuesAsync(string firstName, string lastName,
                            string email, string passwordHash)
        {
            bool result = await this.Manager.RegisterUser(firstName, lastName, email, passwordHash);

            Assert.True(result);
        }

        [Fact]
        public void RegisterUser2_rightValuesAsync()
        { 
            Assert.True(true);
        }
    }
}
