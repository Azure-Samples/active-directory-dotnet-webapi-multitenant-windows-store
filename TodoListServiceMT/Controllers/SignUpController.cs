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
using System.Web.Http;
using TodoListServiceMT.AuthorizationFilters;
using TodoListServiceMT.DAL;
using TodoListServiceMT.Models;

namespace TodoListServiceMT.Controllers
{
    [Authorize]
    public class SignUpController : ApiController
    {
        private TodoListServiceMTContext db = new TodoListServiceMTContext();

        // This method is a placeholder for your onboarding logic.
        // The information provided in the parameters should be used to determine whether the caller
        // (represented by the token securing the call) should be stored as a valid user of the API
        [HttpPost]
        public void Onboard([FromBody]string name)
        {    
            // here "name" is just a placeholder for the real data your app would require from the caller
            // if (MyCustomOnboardingDataValidation(name))
            string upn = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value;
            string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            if (db.Users.FirstOrDefault(a => (a.UPN == upn) && (a.TenantID == tenantID)) == null)
            {
                // add the caller to the collection of valid users
                db.Users.Add(new User { UPN = upn, TenantID = tenantID });
            }            
            db.SaveChanges();
        }
    }
}
