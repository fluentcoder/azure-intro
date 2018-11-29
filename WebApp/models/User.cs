using System;
using System.ComponentModel.DataAnnotations;

namespace models
{
    public class User
    {
        public int UserId { get; set; }

        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
