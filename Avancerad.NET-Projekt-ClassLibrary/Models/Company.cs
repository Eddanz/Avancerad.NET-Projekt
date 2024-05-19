using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avancerad.NET_Projekt_ClassLibrary.Models
{
    public class Company
    {
        [Key]
        public int CompanyID { get; set; }

        [Required(ErrorMessage = "A company name is required")]
        [StringLength(50, ErrorMessage = "Company name can't be longer than 50 characters")]
        public string CompanyName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public IdentityUser IdentityUser { get; set; }

        public string IdentityUserId { get; set; }
    }
}
