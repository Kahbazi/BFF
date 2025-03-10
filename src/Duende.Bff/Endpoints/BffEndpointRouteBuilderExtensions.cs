// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using Duende.Bff;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Duende.Bff.Endpoints;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for the BFF endpoints
    /// </summary>
    public static class BffEndpointRouteBuilderExtensions
    {
        internal static bool _licenseChecked;
        
        private static Task ProcessWith<T>(HttpContext context)
            where T : IBffEndpointService
        {
            var service = context.RequestServices.GetRequiredService<T>();
            return service.ProcessRequestAsync(context);
        }

        /// <summary>
        /// Adds the BFF management endpoints (login, logout, logout notifications)
        /// </summary>
        /// <param name="endpoints"></param>
        public static void MapBffManagementEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapBffManagementLoginEndpoint();
            endpoints.MapBffManagementLogoutEndpoint();
            endpoints.MapBffManagementUserEndpoint();
            endpoints.MapBffManagementBackchannelEndpoint();
        }

        /// <summary>
        /// Adds the login BFF management endpoint
        /// </summary>
        /// <param name="endpoints"></param>
        public static void MapBffManagementLoginEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.CheckLicense();
            
            var options = endpoints.ServiceProvider.GetRequiredService<BffOptions>();

            endpoints.MapGet(options.LoginPath, ProcessWith<ILoginService>);
        }

        /// <summary>
        /// Adds the logout BFF management endpoint
        /// </summary>
        /// <param name="endpoints"></param>
        public static void MapBffManagementLogoutEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.CheckLicense();
            
            var options = endpoints.ServiceProvider.GetRequiredService<BffOptions>();

            endpoints.MapGet(options.LogoutPath, ProcessWith<ILogoutService>);
        }

        /// <summary>
        /// Adds the user BFF management endpoint
        /// </summary>
        /// <param name="endpoints"></param>
        public static void MapBffManagementUserEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.CheckLicense();
            
            var options = endpoints.ServiceProvider.GetRequiredService<BffOptions>();

            endpoints.MapGet(options.UserPath, ProcessWith<IUserService>)
                .AsBffApiEndpoint();
        }

        /// <summary>
        /// Adds the back channel BFF management endpoint
        /// </summary>
        /// <param name="endpoints"></param>
        public static void MapBffManagementBackchannelEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.CheckLicense();
            
            var options = endpoints.ServiceProvider.GetRequiredService<BffOptions>();

            endpoints.MapPost(options.BackChannelLogoutPath, ProcessWith<IBackchannelLogoutService>);
        }

        /// <summary>
        /// Adds a remote BFF API endpoint
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="localPath"></param>
        /// <param name="apiAddress"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapRemoteBffApiEndpoint(
            this IEndpointRouteBuilder endpoints,
            PathString localPath, 
            string apiAddress, 
            bool requireAntiForgeryCheck = true)
        {
            endpoints.CheckLicense();
            
            return endpoints.Map(
                    localPath.Add("/{**catch-all}").Value,
                    BffRemoteApiEndpoint.Map(localPath, apiAddress))
                .WithMetadata(new BffRemoteApiEndpointMetadata { RequireAntiForgeryHeader =  requireAntiForgeryCheck });

        }
        
        internal static void CheckLicense(this IEndpointRouteBuilder endpoints)
        {
            if (_licenseChecked == false)
            {
                var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
                var options = endpoints.ServiceProvider.GetRequiredService<BffOptions>();
                
                LicenseValidator.Initalize(loggerFactory, options);
                LicenseValidator.ValidateLicense();
            }

            _licenseChecked = true;
        }
    }
}