using System;
using _13_Wyd.RestApi.DB.Tables.Items;
using _13_Wyd.RestApi.DB.Tables.Log;
using _13_Wyd.RestApi.DB.Tables.Orders;
using _13_Wyd.RestApi.DB.Tables.Payments;
using _13_Wyd.RestApi.DB.Tables.Reviews;
using _13_Wyd.RestApi.DB.Tables.Users;
using _13_Wyd.RestApi.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _13_Wyd.RestApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        { 
            services.AddControllers();

            //adding Table Storage service
            CloudStorageAccount storageAccount;

            try
            {
                storageAccount = CloudStorageAccount.Parse(Configuration.GetConnectionString("TableStorage"));
            }
            catch(FormatException)
            {
                Console.WriteLine("Invalid storage account information provided.");
                throw;
            }
            catch(ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided.");
                throw;
            }

            CloudTableClient tableClientSingleton = storageAccount.CreateCloudTableClient(
                                                        new TableClientConfiguration());
            services.AddSingleton(tableClientSingleton);

            //adding TS classes to services
            services.AddSingleton<ItemsTS>();
            services.AddSingleton<ItemAvgScoresTS>();
            services.AddSingleton<ItemQuantitiesTS>();
            services.AddSingleton<ItemInterestedUsersTS>();

            services.AddSingleton<ReviewsTS>();

            services.AddSingleton<UsersTS>();
            services.AddSingleton<UserShoppingCartsTS>();
            services.AddSingleton<UserWishlistsTS>();

            services.AddSingleton<OrdersTS>();

            services.AddSingleton<PaymentEventsTS>();
            services.AddSingleton<PaymentsQueueTS>();

            services.AddSingleton<LogTS>();

            //injecting repositories
            services.AddSingleton<IItemsRepository, ItemsRepository>();
            services.AddSingleton<IOrdersRepository, OrdersRepository>();
            services.AddSingleton<IUsersRepository, UsersRepository>();
            services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
            services.AddSingleton<ILogRepository, LogRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
