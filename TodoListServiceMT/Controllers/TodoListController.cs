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
    [MTAuthorize]
    public class TodoListController : ApiController
    {
        private TodoListServiceMTContext db = new TodoListServiceMTContext();

        // GET api/todoapi
        public IEnumerable<Todo> Get()
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            var currentUserToDos = db.Todoes.Where(a => a.Owner == owner);
            return (currentUserToDos.ToList());
        }

        // POST api/todoapi
        public void Post(Todo todo)
        {
            todo.Owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            db.Todoes.Add(todo);
            db.SaveChanges();
        }

        // TODO cleanup
        // DELETE api/todoapi/5
        public void Delete(int id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Todo todo = db.Todoes.Find(id);
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (todo == null || (todo.Owner != owner))
            {
                //return HttpNotFound();
            }
            db.Todoes.Remove(todo);
            db.SaveChanges();
        }

    }
}
