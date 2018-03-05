using SelfServiceWeb.Provisioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Principal;

namespace SelfServiceWeb
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add cookie-based authentication to the request pipeline
            services.AddScoped<ISettings>(p =>
            {
                return new Settings(Configuration);
            });

            services.AddScoped<IProvisioning, Provisioning.Provisioning>();
            services.AddScoped<IValidator, EnvironmentValidator>();
            services.AddScoped<IWorkflow, Workflow>();

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // Azure app service will send the x-ms-client-principal-id when authenticated
            app.Use(async (context, next) =>
            {
                // Create a user on current thread from provided header
                if (context.Request.Headers.ContainsKey("X-MS-CLIENT-PRINCIPAL-ID"))
                {
                    // Read headers from Azure
                    var azureAppServicePrincipalIdHeader = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"][0];
                    var azureAppServicePrincipalNameHeader = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"][0];

                    // Create claims id
                    var claims = new Claim[] {
                        new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", azureAppServicePrincipalIdHeader),
                        new Claim("name", azureAppServicePrincipalNameHeader)
                    };

                    // Set user in current context as claims principal
                    var identity = new GenericIdentity(azureAppServicePrincipalIdHeader);
                    identity.AddClaims(claims);

                    // Set current thread user to identity
                    context.User = new GenericPrincipal(identity, null);
                };

                await next.Invoke();
            });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
