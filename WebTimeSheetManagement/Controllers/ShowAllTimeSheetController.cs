using Amazon.Runtime.Internal.Transform;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using WebTimeSheetManagement.Concrete;
using WebTimeSheetManagement.Filters;
using WebTimeSheetManagement.Interface;
using WebTimeSheetManagement.Models;
using WebTimeSheetManagement.Service;

namespace WebTimeSheetManagement.Controllers
{
    [ValidateAdminSession]
    public class ShowAllTimeSheetController : Controller
    {

        IProject _IProject;
        IUsers _IUsers;
        ITimeSheet _ITimeSheet;
        IDocument _IDocument;

        public ShowAllTimeSheetController()
        {
            _IProject = new ProjectConcrete();
            _ITimeSheet = new TimeSheetConcrete();
            _IUsers = new UsersConcrete();
            _IDocument = new DocumentConcrete();
        }

        // GET: ShowAllTimeSheet
        public ActionResult TimeSheet()
        {
            return View();
        }

        public ActionResult LoadTimeSheetData()
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

                var timesheetdata = _ITimeSheet.ShowAllTimeSheet(sortColumn, sortColumnDir, searchValue, Convert.ToInt32(Session["AdminUser"]));
                recordsTotal = timesheetdata.Count();
                var data = timesheetdata.Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }

        }

        public ActionResult Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("TimeSheet", "AllTimeSheet");
                }

                MainTimeSheetView objMT = new MainTimeSheetView();
                objMT.ListTimeSheetDetails = _ITimeSheet.TimesheetDetailsbyTimeSheetMasterID(Convert.ToInt32(id));
                objMT.ListofProjectNames = _ITimeSheet.GetProjectNamesbyTimeSheetMasterID(Convert.ToInt32(id));
                objMT.ListofPeriods = _ITimeSheet.GetPeriodsbyTimeSheetMasterID(Convert.ToInt32(id));
                objMT.ListoDayofWeek = DayofWeek();
                objMT.TimeSheetMasterID = Convert.ToInt32(id);
                ViewBag.documents = _IDocument.GetListofDocumentByExpenseID(Convert.ToInt32(id));
                return View(objMT);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [NonAction]
        public List<string> DayofWeek()
        {
            List<string> li = new List<string>();
            li.Add("Sunday");
            li.Add("Monday");
            li.Add("Tuesday");
            li.Add("Wednesday");
            li.Add("Thursday");
            li.Add("Friday");
            li.Add("Saturday");
            li.Add("Total");
            return li;
        }

        public ActionResult Download1(int ExpenseID, int DocumentID)
        {
            try
            {
                if (!string.IsNullOrEmpty(Convert.ToString(ExpenseID)) && !string.IsNullOrEmpty(Convert.ToString(DocumentID)))
                {
                    var document = _IDocument.GetDocumentByExpenseID(Convert.ToInt32(ExpenseID), Convert.ToInt32(DocumentID));
                    return File(document.DocumentBytes, System.Net.Mime.MediaTypeNames.Application.Octet, document.DocumentName);
                }
                else
                {
                    return RedirectToAction("TimeSheet", "ShowAllTimeSheet");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    public ActionResult Approval(TimeSheetApproval TimeSheetApproval)
        {
            try
            {
                if (TimeSheetApproval.Comment == null)
                {
                    return Json(false);
                }

                if (string.IsNullOrEmpty(Convert.ToString(TimeSheetApproval.TimeSheetMasterID)))
                {
                    return Json(false);
                }

                _ITimeSheet.UpdateTimeSheetStatus(TimeSheetApproval, 2); //Approve

                if (_ITimeSheet.IsTimesheetALreadyProcessed(TimeSheetApproval.TimeSheetMasterID))
                {
                    _ITimeSheet.UpdateTimeSheetAuditStatus(TimeSheetApproval.TimeSheetMasterID, TimeSheetApproval.Comment, 2);
                }
                else
                {
                    _ITimeSheet.InsertTimeSheetAuditLog(InsertTimeSheetAudit(TimeSheetApproval, 2));
                }
                var userID = _IUsers.GetUserIDbyTimesheetID(TimeSheetApproval.TimeSheetMasterID);
                var userDetails = _IUsers.GetUserDetailsByRegistrationID(userID);
                var adminDetails = _IUsers.GetAdminDetailsByRegistrationID(Convert.ToInt32(Session["AdminUser"]));
                
                EmailUtility.SendMailAsync(EmailConstants.TimesheetStatusUpdate, GetEmailTemplate(TimeSheetApproval, true, userDetails, adminDetails), new List<String>() { userDetails.EmailID }, new List<String>() { adminDetails.EmailID }, EmailUtility.EnumEmailSentType.Login);

                return Json(true);
            }
            catch (Exception)
            {

                throw;
            }
        }



        public ActionResult Rejected(TimeSheetApproval TimeSheetApproval)
        {
            try
            {
                if (TimeSheetApproval.Comment == null)
                {
                    return Json(false);
                }

                if (string.IsNullOrEmpty(Convert.ToString(TimeSheetApproval.TimeSheetMasterID)))
                {
                    return Json(false);
                }

                _ITimeSheet.UpdateTimeSheetStatus(TimeSheetApproval, 3); //Reject

                if (_ITimeSheet.IsTimesheetALreadyProcessed(TimeSheetApproval.TimeSheetMasterID))
                {
                    _ITimeSheet.UpdateTimeSheetAuditStatus(TimeSheetApproval.TimeSheetMasterID, TimeSheetApproval.Comment, 3);
                }
                else
                {
                    _ITimeSheet.InsertTimeSheetAuditLog(InsertTimeSheetAudit(TimeSheetApproval, 3));
                }
                var userID = _IUsers.GetUserIDbyTimesheetID(TimeSheetApproval.TimeSheetMasterID);
                var userDetails = _IUsers.GetUserDetailsByRegistrationID(userID);
                var adminDetails = _IUsers.GetAdminDetailsByRegistrationID(Convert.ToInt32(Session["AdminUser"]));
                EmailUtility.SendMailAsync(EmailConstants.TimesheetStatusUpdate, GetEmailTemplate(TimeSheetApproval, false, userDetails,adminDetails),new List<string>() { userDetails.EmailID }, new List<string>() { adminDetails.EmailID }, EmailUtility.EnumEmailSentType.Login);

                return Json(true);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string GetEmailTemplate(TimeSheetApproval timeSheetApproval, bool isApproved,RegistrationViewDetailsModel userDetails, RegistrationViewDetailsModel adminDetails)
        {
            
            

            List<GetPeriods> timesheetDetail = _ITimeSheet.GetPeriodsbyTimeSheetMasterID(Convert.ToInt32(timeSheetApproval.TimeSheetMasterID));
            return $"Dear {userDetails.Name}, <br/><br/><br> Your Admin  <b>" + adminDetails.Name + $"</b> has <b> {(isApproved ? "Approved" : "Rejected")}</b> the Timesheet for the week of <br/><br/><br> FromDate : " + timesheetDetail.ElementAt(0).Period.ToString()
                + "<br/><br/>Comment: " + timeSheetApproval.Comment
                + (!isApproved ? "<br/><br/><br/>Please take necessary actions on this." : "<br/><br/><br/>Great Job! Please submit furhter timesheets without failing! <br/><br/>Thanks & Regards,<br/>Tresume");

        }
        private TimeSheetAuditTB InsertTimeSheetAudit(TimeSheetApproval TimeSheetApproval, int Status)
        {
            try
            {
                TimeSheetAuditTB objAuditTB = new TimeSheetAuditTB();
                objAuditTB.ApprovalTimeSheetLogID = 0;
                objAuditTB.TimeSheetID = TimeSheetApproval.TimeSheetMasterID;
                objAuditTB.Status = Status;
                objAuditTB.CreatedOn = DateTime.Now;
                objAuditTB.Comment = TimeSheetApproval.Comment;
                objAuditTB.ApprovalUser = Convert.ToInt32(Session["AdminUser"]);
                objAuditTB.ProcessedDate = DateTime.Now;
                objAuditTB.UserID = _IUsers.GetUserIDbyTimesheetID(TimeSheetApproval.TimeSheetMasterID);
                return objAuditTB;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public JsonResult Delete(int TimeSheetMasterID)
        {
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(TimeSheetMasterID)))
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }

                var data = _ITimeSheet.DeleteTimesheetByOnlyTimeSheetMasterID(TimeSheetMasterID);

                if (data > 0)
                {
                    return Json(data: true, behavior: JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(data: false, behavior: JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public ActionResult SubmittedTimeSheet()
        {
            return View();
        }

        public ActionResult ApprovedTimeSheet()
        {
            return View();
        }

        public ActionResult RejectedTimeSheet()
        {
            return View();
        }

        public ActionResult ReminderTimesheet()
        {
            return View();
        }

        public ActionResult NotSubmittedPPL()
        {
            return View();
        }

        public ActionResult LoadSubmittedTData()
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

                var timesheetdata = _ITimeSheet.ShowAllSubmittedTimeSheet(sortColumn, sortColumnDir, searchValue, Convert.ToInt32(Session["AdminUser"]));
                recordsTotal = timesheetdata.Count();
                var data = timesheetdata.Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }

        }

        public ActionResult LoadRejectedData()
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

                var timesheetdata = _ITimeSheet.ShowAllRejectTimeSheet(sortColumn, sortColumnDir, searchValue, Convert.ToInt32(Session["AdminUser"]));
                recordsTotal = timesheetdata.Count();
                var data = timesheetdata.Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }

        }

        public ActionResult LoadApprovedData()
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

                var timesheetdata = _ITimeSheet.ShowAllApprovedTimeSheet(sortColumn, sortColumnDir, searchValue, Convert.ToInt32(Session["AdminUser"]));
                recordsTotal = timesheetdata.Count();
                var data = timesheetdata.Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }

        }

        public ActionResult LoadDefaultersData()
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
                Dictionary<string, List<string>> keyValuePairs = new Dictionary<string, List<string>>();
                Dictionary<string, RegistrationViewSummaryModel> dataValuePairs = new Dictionary<string, RegistrationViewSummaryModel>();
                IQueryable<TimeSheetMasterView> timesheetdata = _ITimeSheet.ShowAllTimeSheet(null, null, null, Convert.ToInt32(Session["AdminUser"]));
                var week = new CultureInfo("en-US").Calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                var grpData = timesheetdata.Where(d => (d.SubmittedWeek) >= (week - 4) ).GroupBy(d => d.Username);
                var rolesData = _IUsers.ShowallUsersUnderAdmin(null, null, null, Convert.ToInt32(Session["AdminUser"])).ToList();
                DateTime fday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                List<string> data = new List<string>();
                for (int i = 1; i <= 4; i++)
                {
                    data.Add(String.Format("{0:yyyy-MM-dd}", fday.AddDays(-7 * i)));
                }

                rolesData.ForEach(roles =>
                {
                    keyValuePairs[roles.Username] = new List<string>(data);
                    dataValuePairs[roles.Username] = roles;
                });

                grpData.ForEach(grp =>
                {
                    grp.ForEach(g =>
                    {
                        if(g.TimeSheetStatus.ToLower().Equals("approved")|| g.TimeSheetStatus.ToLower().Equals("submitted"))
                            keyValuePairs[g.Username].Remove(g.FromDate);
                    });
                });

                IQueryable<DefaulterModel> defaulterModels;
                List<DefaulterModel> defaulterModelsList=new List<DefaulterModel>();
                keyValuePairs.Keys.ForEach(roles =>
                {
                    var defaultModel = new DefaulterModel();
                    defaultModel.UserName = dataValuePairs[roles].Name;
                    defaultModel.missedDatesToFill = keyValuePairs[roles];
                    defaultModel.EmployeeStatus = dataValuePairs[roles].Status;
                    defaultModel.LegalStatus = dataValuePairs[roles].LegalStatus;
                    defaulterModelsList.Add(defaultModel);
                });
                defaulterModels = defaulterModelsList.AsQueryable<DefaulterModel>();
                var data1 = defaulterModels.Skip(skip).Take(pageSize).ToList();
                
                if (!string.IsNullOrEmpty(searchValue))
                {
                    defaulterModels = defaulterModels.Where(m => (m.UserName.ToLower().Contains(searchValue) || m.LegalStatus
                    .ToLower().Contains(searchValue)));
                    data1= defaulterModels.Skip(skip).Take(pageSize).ToList();
                }
                return Json(new { draw = draw, recordsFiltered = defaulterModelsList.Count(), recordsTotal = defaulterModelsList.Count(), data = data1 });
            }
            catch (Exception)
            {
                throw;
            }

        }


        public ActionResult SendReminderEmail()
        {
            Dictionary<string, List<string>> keyValuePairs = new Dictionary<string, List<string>>();
            Dictionary<string, RegistrationViewSummaryModel> dataValuePairs = new Dictionary<string, RegistrationViewSummaryModel>();
            IQueryable<TimeSheetMasterView> timesheetdata = _ITimeSheet.ShowAllTimeSheet(null, null, null, Convert.ToInt32(Session["AdminUser"]));
            var week = new CultureInfo("en-US").Calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            var grpData = timesheetdata.Where(d => (d.SubmittedWeek) >= (week - 4)).GroupBy(d => d.Username);
            var rolesData = _IUsers.ShowallUsersUnderAdmin(null, null, null, Convert.ToInt32(Session["AdminUser"])).ToList();
            DateTime fday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            List<string> data = new List<string>();
            for (int i = 1; i <= 4; i++)
            {
                data.Add(String.Format("{0:yyyy-MM-dd}", fday.AddDays(-7 * i)));
            }

            rolesData.ForEach(roles =>
            {
                keyValuePairs[roles.Username] = new List<string>(data);
                dataValuePairs[roles.Username] = roles;
            });

            grpData.ForEach(grp =>
            {
                grp.ForEach(g =>
                {
                    if (g.TimeSheetStatus.ToLower().Equals("approved") || g.TimeSheetStatus.ToLower().Equals("submitted"))
                        keyValuePairs[g.Username].Remove(g.FromDate);
                });
            });

            keyValuePairs.Keys.ForEach(roles =>
            {
                EmailUtility.SendMailAsync(EmailConstants.TimesheetReminder, GetReminderEmailTemplate(dataValuePairs[roles], keyValuePairs[roles]), new List<string>() { dataValuePairs[roles].EmailID }, null, EmailUtility.EnumEmailSentType.Login);
            });

            return Json(true);

        }

        private string GetReminderEmailTemplate(RegistrationViewSummaryModel dataValue, List<string> timePeriod)
        {
            return $"Dear {dataValue.Name}, <br/><br/><br/> Following timesheets are missing.Details of the missing hours below.Please submit the timesheets at the earliest <br/><br/> {String.Join("<br/>", timePeriod.ToArray())}<br/><br/>Thanks and Regards,<br/>{Session["Username"]}";

        }
    }
}