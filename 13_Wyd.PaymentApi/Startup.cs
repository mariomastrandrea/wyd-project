using System;
using _13_Wyd.PaymentApi.EventsPayment;
using _13_Wyd.PaymentApi.PaymentEvents;
using _13_Wyd.PaymentApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _13_Wyd.PaymentApi
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

            services.AddSingleton<PaymentsManager>();
            services.AddSingleton<PaymentMethodFactory>();
            services.AddSingleton<PaymentsDataService>();

            // adding data Rest Api http service
            Uri dataRestApiUri = new Uri(Configuration.GetSection("RestApiUri")
                                                      .GetValue<string>("Data"));

            services.AddHttpClient<PaymentsDataService>(client =>
                                client.BaseAddress = dataRestApiUri);

            //add external payment APIs clients

            services.AddHttpClient("CardClient", client =>
            {
                Uri cardApiUri = new Uri(Configuration.GetSection("ExternalApiUri")
                                                      .GetValue<string>("Card"));
                client.BaseAddress = cardApiUri;
            });

            services.AddHttpClient("PaypalClient", client =>
            {
                Uri paypalApiUri = new Uri(Configuration.GetSection("ExternalApiUri")
                                                        .GetValue<string>("Paypal"));
                client.BaseAddress = paypalApiUri;
            });

            services.AddHttpClient("TransferClient", client =>
            {
                Uri transferApiUri = new Uri(Configuration.GetSection("ExternalApiUri")
                                                          .GetValue<string>("Transfer"));
                client.BaseAddress = transferApiUri;
            });
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
