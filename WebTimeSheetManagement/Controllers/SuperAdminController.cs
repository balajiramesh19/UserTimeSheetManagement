using Dapper;
using EventApplicationCore.Library;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using WebTimeSheetManagement.Concrete;
using WebTimeSheetManagement.Filters;
using WebTimeSheetManagement.Helpers;
using WebTimeSheetManagement.Interface;
using WebTimeSheetManagement.Models;

namespace WebTimeSheetManagement.Controllers
{
    [ValidateCommonAdminSession]
    public class SuperAdminController : Controller
    {
        private IRegistration _IRegistration;
        private IRoles _IRoles;
        private IAssignRoles _IAssignRoles;
        private ICacheManager _ICacheManager;
        private IUsers _IUsers;
        private IProject _IProject;
        private ITimeSheet _ITimeSheet;
        

        public SuperAdminController()
        {
            _IRegistration = new RegistrationConcrete();
            _IRoles = new RolesConcrete();
            _IAssignRoles = new AssignRolesConcrete();
            _ICacheManager = new CacheManager();
            _IUsers = new UsersConcrete();
            _IProject = new ProjectConcrete();
            _ITimeSheet = new TimeSheetConcrete();
        }

        // GET: SuperAdmin
        public ActionResult Dashboard()
        {
            try
            {
                var datadashboard = _ITimeSheet.GetDashboardDataByID(Convert.ToString(Session["SuperAdmin"]), "SuperAdminUser");
                ViewBag.DashboardData = datadashboard;
                var adminCount = _ICacheManager.Get<object>("AdminCount");
                var statusCountdashboard = _ITimeSheet.GetDashboardStatusDataByID(Convert.ToString(Session["SuperAdmin"]), "SuperAdminUser");
                var LegalStatusCountdashboardData = _ITimeSheet.GetDashboardLegalStatusDataByAdminID(Convert.ToString(Session["SuperAdmin"]), "SuperAdminUser");

                ViewBag.StatusCountdashboard = statusCountdashboard;
                ViewBag.LegalStatusCountdashboardData = LegalStatusCountdashboardData;
                if (adminCount == null)
                {
                    var admincount = _IUsers.GetTotalAdminsCount();
                    _ICacheManager.Add("AdminCount", admincount);
                    ViewBag.AdminCount = admincount;
                }
                else
                {
                    ViewBag.AdminCount = adminCount;
                }

                var usersCount = _ICacheManager.Get<object>("UsersCount");

                if (usersCount == null)
                {
                    var userscount = _IUsers.GetTotalUsersCount();
                    _ICacheManager.Add("UsersCount", userscount);
                    ViewBag.UsersCount = userscount;
                }
                else
                {
                    ViewBag.UsersCount = usersCount;
                }

                var projectCount = _ICacheManager.Get<object>("ProjectCount");

                if (projectCount == null)
                {
                    var projectcount = _IProject.GetTotalProjectsCounts();
                    _ICacheManager.Add("ProjectCount", projectcount);
                    ViewBag.ProjectCount = projectcount;
                }
                else
                {
                    ViewBag.ProjectCount = projectCount;
                }

                return View();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult TSCountData()
        {
            var datadashboard = _ITimeSheet.GetDashboardDataByID(Convert.ToString(Session["SuperAdmin"]), "SuperAdminUser");
            ViewBag.DashboardData = datadashboard;
            return View();
        }
        public ActionResult StatusData()
        {
            var statusCountdashboard = _ITimeSheet.GetDashboardStatusDataByID(Convert.ToString(Session["SuperAdmin"]), "SuperAdminUser");
            ViewBag.statusCountdashboard = statusCountdashboard;
            return View();
        }

        public ActionResult LegalStatusData()
        {
            var LegalStatusCountdashboardData = _ITimeSheet.GetDashboardLegalStatusDataByAdminID(Convert.ToString(Session["SuperAdmin"]), "SuperAdminUser");
            ViewBag.LegalStatusCountdashboardData = LegalStatusCountdashboardData;
            return View();
        }


        [HttpGet]
        public ActionResult CreateAdmin()
        {
            return View(new Registration());
        }

        [HttpPost]
        public ActionResult CreateAdmin(Registration registration)
        {

            try
            {
                var isUsernameExists = _IRegistration.CheckUserNameExists(registration.Username);

                if (isUsernameExists)
                {
                    ModelState.AddModelError("", errorMessage: "Username Already Used try unique one!");
                }
                else
                {
                    registration.CreatedOn = DateTime.Now;
                    registration.RoleID = _IRoles.getRolesofUserbyRolename("Admin");
                    registration.Password = EncryptionLibrary.EncryptText(registration.Password);
                    registration.ConfirmPassword = EncryptionLibrary.EncryptText(registration.ConfirmPassword);
                    if (_IRegistration.AddUser(registration) > 0)
                    {
                        TempData["MessageRegistration"] = "Data Saved Successfully!";
                        return RedirectToAction("CreateAdmin");
                    }
                    else
                    {
                        return View("CreateAdmin", registration);
                    }
                }

                return RedirectToAction("Dashboard");
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult AssignRoles()
        {
            try
            {
                AssignRolesModel assignRolesModel = new AssignRolesModel();
                assignRolesModel.ListofAdmins = _IAssignRoles.ListofAdmins();
                assignRolesModel.ListofUser = _IAssignRoles.GetListofUnAssignedUsers(Convert.ToString(Session["OrganizationId"]));
                return View(assignRolesModel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult LoadUsersData()
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
                
                var rolesData = _IAssignRoles.GetListofUnAssignedUsers(Convert.ToString(Session["OrganizationId"]),sortColumn, sortColumnDir, searchValue);
                recordsTotal = rolesData.Count();
                var data = rolesData.Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public ActionResult AssignRoles(List<UserModel> SelectedUsers,string assignToAdmin)
        {
            try
            {
                AssignRolesModel objassign = new AssignRolesModel();
                objassign.ListofUser = SelectedUsers;
                if (objassign.ListofUser == null || string.IsNullOrEmpty(assignToAdmin))
                {
                    TempData["MessageErrorRoles"] = string.IsNullOrEmpty(assignToAdmin)?"Please select the admin to assign" : "There are no Users to Assign Roles";
                    objassign.ListofAdmins = _IAssignRoles.ListofAdmins();
                    objassign.ListofUser = _IAssignRoles.GetListofUnAssignedUsers(Convert.ToString(Session["OrganizationId"]));
                    return View(objassign);
                }

                objassign.AssignToAdmin = Convert.ToInt32(assignToAdmin);

                var SelectedCount = (from User in objassign.ListofUser
                                     where User.selectedUsers == true
                                     select User).Count();

                if (SelectedCount == 0)
                {
                    TempData["MessageErrorRoles"] = "You have not Selected any User to Assign Roles";
                    objassign.ListofAdmins = _IAssignRoles.ListofAdmins();
                    objassign.ListofUser = _IAssignRoles.GetListofUnAssignedUsers(Convert.ToString(Session["OrganizationId"]));
                    return View(objassign);
                }

                if (ModelState.IsValid)
                {
                    objassign.CreatedBy = Convert.ToInt32(Session["SuperAdmin"]);

                    _IAssignRoles.SaveAssignedRoles(objassign);
                    TempData["MessageRoles"] = "Roles Assigned Successfully!";
                }

                objassign = new AssignRolesModel();
                objassign.ListofAdmins = _IAssignRoles.ListofAdmins();
                objassign.ListofUser = _IAssignRoles.GetListofUnAssignedUsers(Convert.ToString(Session["OrganizationId"]));

                return RedirectToAction("AssignRoles");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}