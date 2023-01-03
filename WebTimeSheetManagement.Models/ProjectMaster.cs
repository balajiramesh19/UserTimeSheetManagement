using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTimeSheetManagement.Models
{
    [Table("ProjectMaster")]
    public class ProjectMaster
    {
        [Key]
        public int ProjectID { get; set; }

        [Required(ErrorMessage = "Enter Project Code")]
        public string ProjectCode { get; set; }

        [Required(ErrorMessage = "Enter Nature of Industry")]
        public string NatureofIndustry { get; set; }

        [Required(ErrorMessage = "Enter Project Name")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Enter NetTerms")]
        public string NetTerms { get; set; }

        [Required(ErrorMessage = "Enter Termination Notice")]
        public string TerminationNotice { get; set; }

        [Required(ErrorMessage = "Enter Status")]
        public bool Status { get; set; }
    }
}
