using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Temperature.Startup))]

namespace Temperature
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var options = new DashboardOptions
            {
                AuthorizationFilters = new[]
            {
                new LocalRequestsOnlyAuthorizationFilter()
            }
            };

            app.UseHangfireDashboard("/hangfire", options);

            /*
            app.UseHangfire(config =>
            {
                config.UseSqlServerStorage("DbConnection");
                config.UseServer();
            });

            app.UseHangfireServer();*/
        }
    }
}
