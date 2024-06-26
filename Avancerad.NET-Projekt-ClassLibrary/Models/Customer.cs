﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Avancerad.NET_Projekt_ClassLibrary.Models
{
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "A first name is required")]
        [StringLength(25, ErrorMessage = "First name can't be longer than 25 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "A last name is required")]
        [StringLength(50, ErrorMessage = "Last name can't be longer than 50 characters")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public IdentityUser IdentityUser { get; set; }

        public string IdentityUserId { get; set; }
    }
}
