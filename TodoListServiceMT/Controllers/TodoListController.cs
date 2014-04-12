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
    // This attribute guarantees that only callers belonging to the collection of onboarded users can access the actions on this controller
    [MTAuthorize]
    public class TodoListController : ApiController
    {
        private TodoListServiceMTContext db = new TodoListServiceMTContext();

        // GET api/todoapi
        // Returns a list of all the saved Todo items associated to the current caller
        public IEnumerable<Todo> Get()
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            var currentUserToDos = db.Todoes.Where(a => a.Owner == owner);
            return (currentUserToDos.ToList());
        }

        // POST api/todoapi
        // Saves in the db a new Todo item associated to the current caller
        public void Post(Todo todo)
        {
            todo.Owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            db.Todoes.Add(todo);
            db.SaveChanges();
        }
    }
}
