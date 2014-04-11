using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TodoListServiceMT.Models
{
    public class Todo
    {
        public int ID { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
    }

    public class Tenant
    {
        public int ID { get; set; }
        public string IssValue { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        [DisplayName("Check this if you are an administrator and you want to enable the app for all your users")]
        public bool AdminConsented { get; set; }
    }

    public class User
    {
        [Key]
        public string UPN { get; set; }
        public string TenantID { get; set; }
    }
}