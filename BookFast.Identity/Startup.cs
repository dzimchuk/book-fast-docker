using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookFast.Identity
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddApplicationInsightsTelemetry(configuration);

            ConfigureDataProtection(services);
        }

        private void ConfigureDataProtection(IServiceCollection services)
        {
            var connectionString = configuration["Data:Azure:Storage:StorageConnectionString"];
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var client = storageAccount.CreateCloudBlobClient();
                var container = client.GetContainerReference("key-container");

                container.CreateIfNotExistsAsync().GetAwaiter().GetResult();

                services.AddDataProtection()
                    .SetApplicationName("BookFast.Identity")
                    .PersistKeysToAzureBlobStorage(container, "BookFast.Identity.xml");
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
