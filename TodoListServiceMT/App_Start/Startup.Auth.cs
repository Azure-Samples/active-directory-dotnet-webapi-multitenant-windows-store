//----------------------------------------------------------------------------------------------
//    Copyright 2014 Microsoft Corporation
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;

namespace TodoListServiceMT
{
    public partial class Startup
    {
        // This service must be able to accept tokens for users from multiple organizations.
        // The default validation logic enforces that the incoming Issuer value corresponds to the one associated to the target authority,
        // but in this case every organization will use a different value.
        // To accommodate the situation, the code below turns off the default Issuer validation logic.
        // The actual check takes place in the MTAuthorizeAttribute filter.
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Tenant = ConfigurationManager.AppSettings["ida:Tenant"],
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters() { ValidateIssuer = false, ValidAudience = ConfigurationManager.AppSettings["ida:Audience"]
                    }
                });
        }
    }
}
