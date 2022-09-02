using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using _13_Wyd.App.Services;
using _13_Wyd.App.Services.Data;

namespace _13_Wyd.App
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
            services.AddRazorPages();
            services.AddSingleton<WydManager>();
            services.AddSingleton<PaymentService>();
            services.AddSingleton<ItemsService>();
            services.AddSingleton<UsersService>();
            services.AddSingleton<OrdersService>();
            services.AddSingleton<LogService>();

            // adding payment Rest Api service
            services.AddHttpClient<PaymentService>(client =>
            {
                Uri paymentApiUri = new Uri(Configuration.GetSection("RestApiUri")
                                                         .GetValue<string>("Payment"));
                client.BaseAddress = paymentApiUri;
            });

            // adding data Rest Api http services
            Uri dataRestApiUri = new Uri(Configuration.GetSection("RestApiUri")
                                                      .GetValue<string>("Data"));

            services.AddHttpClient<UsersService>(client =>
                        client.BaseAddress = dataRestApiUri);

            services.AddHttpClient<ItemsService>(client =>
                        client.BaseAddress = dataRestApiUri);

            services.AddHttpClient<OrdersService>(client =>
                        client.BaseAddress = dataRestApiUri);

            services.AddHttpClient<LogService>(client =>
                        client.BaseAddress = dataRestApiUri);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
