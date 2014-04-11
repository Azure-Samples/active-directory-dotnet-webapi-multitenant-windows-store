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
    public class MTAuthorizeAttribute : AuthorizationFilterAttribute
    {
        

        public override void OnAuthorization(HttpActionContext actionContext)
        {

            string issuer = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Issuer;
            string UPN = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Name).Value;

            using (TodoListServiceMTContext db = new TodoListServiceMTContext())
            {
                if (!(
                    //admin consented, recorded issuer
                    (db.Tenants.FirstOrDefault(a => ((a.IssValue == issuer) && (a.AdminConsented))) != null)
                    //user consented, recorded user
                    || (db.Users.FirstOrDefault(b => (b.UPN == UPN)) != null)
                    ))
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Nice try! Ha!");
                }
            }
        }
    }
}