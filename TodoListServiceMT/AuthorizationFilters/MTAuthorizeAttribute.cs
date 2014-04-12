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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using TodoListServiceMT.DAL;

namespace TodoListServiceMT.AuthorizationFilters
{
    // This attribute limits access of the resource it decorates to the users that have been onboarded
    public class MTAuthorizeAttribute : AuthorizationFilterAttribute
    {        
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            string issuer = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Issuer;
            string UPN = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value;
            string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

            using (TodoListServiceMTContext db = new TodoListServiceMTContext())
            {
                if (!(
                    // Verifies if the organization to which the caller belongs is trusted.
                    // This onboarding style is not possible in the consent flow originated by a native app shown in this sample,
                    // but it could be achieved by triggering consent from an associated web application.
                    // For details, see the sample https://github.com/AzureADSamples/WebApp-WebAPI-MultiTenant-OpenIdConnect-DotNet
                    (db.Tenants.FirstOrDefault(a => ((a.IssValue == issuer) && (a.AdminConsented))) != null)
                    // Verifies if the caller is in the db of onboarded users.                    
                    || (db.Users.FirstOrDefault(b => (b.UPN == UPN) && (b.TenantID == tenantID)) != null)
                    ))
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, 
                        string.Format("The user {0} has not been onboarded. Sign up and try again",UPN));
                }
            }
        }
    }
}

 
            