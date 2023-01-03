using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTimeSheetManagement.Models
{
    [NotMapped]
    public class DefaulterModel
    {
        public string UserName { get; set; }
        public string LegalStatus { get; set; }
        public string EmployeeStatus { get; set; }
        public List<string> missedDatesToFill { get; set; }

    }
}
