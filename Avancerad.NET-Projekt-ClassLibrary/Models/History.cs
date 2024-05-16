using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avancerad.NET_Projekt_ClassLibrary.Models
{
    public class History
    {
        [Key]
        public int HistoryID { get; set; }

        [Required(ErrorMessage = "You must choose a date for your appointment")]
        [DataType(DataType.DateTime)]
        public DateTime ChangeDate { get; set; }

        public string ChangeType { get; set; }

        public string ChangeDescription { get; set; }

        public Appointment Appointment { get; set; }

        [ForeignKey("Appointment")]
        public int AppointmentID { get; set; }
    }
}
