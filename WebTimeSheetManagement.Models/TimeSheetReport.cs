using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTimeSheetManagement.Models
{
    public  class TimeSheetReport
    {
        public string Name { get; set; }

        public string DataPeriod { get; set; }

        public int HoursLogged { get; set; }
    }
}
