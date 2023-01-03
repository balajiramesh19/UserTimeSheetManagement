using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTimeSheetManagement.Models
{
    public  class DashboardData
    {
        public int ApprovalUser { get; set; }
        public int TotalTimeSheets { get; set; }
        public int Totalhours { get; set; }
        public int SubmittedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public string TSMonth { get; set; }

    }
}
