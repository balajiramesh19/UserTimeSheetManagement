using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimeSheetManagement.Models
{
    [Table("Registration")]
    public class Registration
    {
        [Key]
        public int RegistrationID { get; set; }

        [Required(ErrorMessage = "Enter Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mobileno Required")]
        [RegularExpression(@"^(\d{10})$", ErrorMessage = "Wrong Mobileno")]
        public string Mobileno { get; set; }

        [Required(ErrorMessage = "EmailID Required")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail adress")]
        public string EmailID { get; set; }

        [MinLength(6, ErrorMessage = "Minimum Username must be 6 in charaters")]
        [Required(ErrorMessage = "Username Required")]
        public string Username { get; set; }

        [MinLength(7, ErrorMessage = "Minimum Password must be 7 in charaters")]
        [Required(ErrorMessage = "Password Required")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Enter Valid Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Gender Required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Status Required")]
        public string Status { get; set; }

        [Required(ErrorMessage = "LegalStatus Required")]
        public string LegalStatus { get; set; }

        public List<LegalStatusModel> LegalStatusList { get; set; }

        public string OrganizationId { get; set; }

        public DateTime? Birthdate { get; set; }
        public DateTime? DateofJoining { get; set; }
        
        public int? RoleID { get; set; }

        [MaxLength(5, ErrorMessage = "Minimum Password must be 7 in charaters")]
        public string  EmployeeID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ForceChangePassword { get; set; }

        [Table("LegalStatus")]
        public class LegalStatusModel
        {
            public string ID { get; set; }
            public string LegalStatus { get; set; }
        }
    }
}
