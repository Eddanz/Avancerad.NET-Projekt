using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avancerad.NET_Projekt_ClassLibrary.Models
{
    public class Tracker
    {
        public int Id { get; set; }
        public string ModelName { get; set; }
        public string ActionPerformed { get; set; }
        public DateTime Timestamp { get; set; }
        public string Changes { get; set; }
    }
}
