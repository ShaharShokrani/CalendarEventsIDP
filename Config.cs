// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;

namespace CalendarEvents.IDP
{
    public static class Config
    {
        private static readonly List<string> Claims = new List<string>
        {              
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            IdentityServerConstants.StandardScopes.Email,
            "calendareventsapi"
        };
        
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {            
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("calendareventsapi", "Calendar Events API")
                {
                }
            };

        private static string clientUri => Environment.GetEnvironmentVariable("ClientUri") ?? "localhost:4200";//todo cha host.docker.internal
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "calendareventsui",
                    ClientName = "Calendar Events UI",                    
                    RequireClientSecret = false,
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris =           { $"http://{clientUri}/signin-callback", $"http://{clientUri}/assets/silent-callback.html" },
                    PostLogoutRedirectUris = { $"http://{clientUri}/signout-callback" },
                    AllowedCorsOrigins =     { $"http://{clientUri}" },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "calendareventsapi"
                    },
                    AccessTokenLifetime = 600
                }
            };
    }
}