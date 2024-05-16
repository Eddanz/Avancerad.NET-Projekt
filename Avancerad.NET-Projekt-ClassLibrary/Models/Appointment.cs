using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avancerad.NET_Projekt_ClassLibrary.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentID { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "You must choose a date for your appointment")]
        [DataType(DataType.DateTime)]
        public DateTime AttendDate { get; set; }

        public bool IsDeleted { get; set; } = false;

        public Customer Customer { get; set; }

        [Required(ErrorMessage = "You must enter a Customer ID for the appointment")]
        public int CustomerID { get; set; }
    }
}
