using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTimeSheetManagement.Constants
{
    public static class EmailConstants
    {
        private static readonly string RegistrationSubject="Welcome to Tresume Timesheet";
        private static readonly List<string> ToEmail= new List<string> { "balaji@tresume.us" };
        //private static readonly List<string> CCEmail = new List<string> { "shalini@tresume.us", "balaji@tresume.us", "rohit@tresume.us", "prab@astacrs.com" };
    }
}