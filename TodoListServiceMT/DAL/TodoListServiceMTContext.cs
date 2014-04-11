using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using TodoListServiceMT.Models;

namespace TodoListServiceMT.DAL
{
    public class TodoListServiceMTContext: DbContext
    {
        public TodoListServiceMTContext()
            : base("TodoListServiceMTContext")
        { }
        public DbSet<Todo> Todoes { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}