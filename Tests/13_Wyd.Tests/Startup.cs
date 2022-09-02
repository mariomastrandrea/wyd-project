using System;
using _13_Wyd.App;
using _13_Wyd.App.Services;
using _13_Wyd.App.Services.Data;
using _13_Wyd.Tests.Manager;
using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection;
namespace _13_Wyd.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<WydManager>();
            services.AddTransient<PaymentService>();
            services.AddTransient<ItemsService>();
            services.AddTransient<UsersService>();
            services.AddTransient<OrdersService>();
            services.AddTransient<LogService>();
            services.AddTransient<UserAccountTests>();

            // adding payment Rest Api service
            services.AddHttpClient<PaymentService>(client =>
            {
                Uri paymentApiUri = new Uri("https://localhost:40003/");
                client.BaseAddress = paymentApiUri;
            });

            // adding data Rest Api http services
            Uri dataRestApiUri = new Uri("https://localhost:34670");

            services.AddHttpClient<UsersService>(client =>
                        client.BaseAddress = dataRestApiUri);

            services.AddHttpClient<ItemsService>(client =>
                        client.BaseAddress = dataRestApiUri);

            services.AddHttpClient<OrdersService>(client =>
                        client.BaseAddress = dataRestApiUri);

            services.AddHttpClient<LogService>(client =>
                        client.BaseAddress = dataRestApiUri);
        }

        public void Configure(IServiceProvider provider)
        {
        
        }
    }
}
