using EventApplicationCore.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebTimeSheetManagement.Concrete;
using WebTimeSheetManagement.Filters;
using WebTimeSheetManagement.Interface;
using WebTimeSheetManagement.Models;
using WebTimeSheetManagement.Service;

namespace WebTimeSheetManagement.Controllers
{
    [ValidateSuperAdminSession]
    public class AllRolesController : Controller
    {
        IAssignRoles _IAssignRoles;
        IUsers _IUsersConcrete;
        public AllRolesController()
        {
            _IAssignRoles = new AssignRolesConcrete();
            _IUsersConcrete = new UsersConcrete();
        }

        // GET: AllRoles
        public ActionResult Roles()
        {
            return View();
        }

        public ActionResult LoadRolesData()
        {
            try
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                int recordsTotal = 0;

                var rolesData = _IAssignRoles.ShowallRoles(sortColumn, sortColumnDir, searchValue);
                recordsTotal = rolesData.Count();
                var data = rolesData.Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult RemovefromRole(string RegistrationID)
        {
            try
            {
                if (string.IsNullOrEmpty(RegistrationID))
                {
                    return RedirectToAction("Roles");
                }

                var role = _IAssignRoles.RemovefromUserRole(RegistrationID);
                RegistrationViewDetailsModel user = _IUsersConcrete.GetUserDetailsByRegistrationID(Convert.ToInt32(RegistrationID));
                RegistrationViewDetailsModel userSuperAdmin = _IUsersConcrete.GetUserDetailsByRegistrationID(Convert.ToInt32(Session["SuperAdmin"]));
                EmailUtility.SendMailAsync(EmailConstants.RemoveRole, GetEmailTemplate(RegistrationID, user), new List<string>() { user.EmailID }, new List<string>() { userSuperAdmin.EmailID }, EmailUtility.EnumEmailSentType.Login);
                return Json(role);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        private string GetEmailTemplate(string RegistrationID, RegistrationViewDetailsModel user)
        {
            return $"Hi {user.Name},<br/><br/><br>The admin {Session["Username"]} has revovked your Timesheet Access.";
        }


    }
}