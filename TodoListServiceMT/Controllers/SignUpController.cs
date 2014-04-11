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
    //[MTAuthorize]
    public class SignUpController : ApiController
    {
        private TodoListServiceMTContext db = new TodoListServiceMTContext();

        //public string Get()
        //{
            
        //    return "u got it";
        //}
        
        [HttpPost]
        public void Onboard([FromBody]string name)
        {    
            //todo: 
            // change the primary key
            // use sub instead of UPN
            string upn = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value;
            if((db.Users.FirstOrDefault(a=>a.UPN == upn))==null)
                db.Users.Add(new User { 
                    UPN = upn, 
                    TenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value
                });
            
            db.SaveChanges();
            //return true;
        }
    }
}
