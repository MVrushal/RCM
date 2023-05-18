using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Models;
using Integr8ed.Service;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface;
using Integr8ed.Service.Interface.ClientAdmin;
using Integr8ed.Service.Utility;
using Integr8ed.Utility.Common;
using Integr8ed.Utility.JqueryDataTable;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class DashboardController : BaseController<DashboardController>
    {
        #region Fields
        private readonly IClientAdminDashboardServices _clientAdminDashboardServices;
        private readonly IConfiguration _config;
        private readonly ICallLogsServices _callLogs;
        private readonly IUserService _userService;
        private readonly ICompanyUserService _companyUser;
        private readonly IBranchService _branchService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EmailService _emailService;

        #endregion

        #region CTOR

        public DashboardController(IWebHostEnvironment webHostEnvironment, ICompanyUserService companyUser, EmailService emailService, IUserService userService, IClientAdminDashboardServices clientAdminDashboardServices, IConfiguration config, ICallLogsServices callLogs,
            IBranchService branchService
            )
        {
            _userService = userService;
            _companyUser = companyUser;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _clientAdminDashboardServices = clientAdminDashboardServices;
            _config = config;
            _callLogs = callLogs;
            _branchService = branchService;
        }

        #endregion

        #region Method
        public async Task<IActionResult> Index()
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);
            DashboardDto dt = new DashboardDto();
            var Parameters = new List<SqlParameter>
            {

                // new SqlParameter("@BookingDetailId",SqlDbType.Int){Value =  HttpContext.Session.GetInt32("BookingDetailId") },

            };


            // dt = await _clientAdminDashboardServices.GetClientAdminDashboardData(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
            var userID = HttpContext.Session.GetString("UserID");
            var branchList = _branchService.getBranchList(HttpContext.Session.GetString("ConnectionString"), userID);
            ViewBag.BranchList = branchList.Select(x => new SelectListItem()
            {
                Text = x.BranchName,
                Value = x.Id.ToString()
            }).OrderBy(x => x.Text);
            long BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
            return View(BranchId);
        }


        [HttpGet]
        public IActionResult changeBranch(long id)
        {
        
            HttpContext.Session.SetString("BranchId", Convert.ToString(id));
            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
        }


        public async Task<IActionResult> _DashboardDetails(string startDate, string Enddate, int BookingType)
        {
            bool status = CheckISSessionExpired();
            try
            {
                if (status)
                    return Redirect(_config["CommonProperty:PhysicalUrl"]);

                DashboardDto dt = new DashboardDto();
                if (startDate == null || Enddate == null)
                {
                    startDate = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    Enddate = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }


                var Parameters = new List<SqlParameter>
                {

                     new SqlParameter("@BranchId",SqlDbType.BigInt){Value =  Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString())},
                     new SqlParameter("@startDate",SqlDbType.VarChar){Value = Convert.ToDateTime(startDate).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)},
                     new SqlParameter("@enddate",SqlDbType.VarChar){Value =  Convert.ToDateTime(Enddate).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                     new SqlParameter("@bookingType",SqlDbType.Int){Value =  BookingType }
                };

                dt = await _clientAdminDashboardServices.GetClientAdminDashboardData(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());

                return PartialView("_DashboardDetails", dt);
            }
            catch (Exception e)
            {
                return PartialView("_DashboardDetails", null);
            }


        }


        public IActionResult _LoadChartJSData()
        {
            return PartialView("_LoadChartJSData");
        }


        public async Task<IActionResult> _GetBookingCountDetail(JQueryDataTableParamModel param, int flag, string startDate, string endDate)
        {
            try
            {
                bool status = CheckISSessionExpired();
                if (status)
                    return Redirect(_config["CommonProperty:PhysicalUrl"]);
                DashboardDto dt = new DashboardDto();
                if (startDate == null || endDate == null)
                {
                    startDate = DateTime.UtcNow.AddMonths(-6).ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                    endDate = DateTime.UtcNow.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                }
                else
                {

                    startDate = Convert.ToDateTime(startDate).ToString("yyyy/MM/dd");
                    endDate = Convert.ToDateTime(endDate).ToString("yyyy/MM/dd");

                }
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;

                parameters.Add(new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                parameters.Add(new SqlParameter("@flag", SqlDbType.VarChar) { Value = flag });
                parameters.Add(new SqlParameter("@startDate", SqlDbType.VarChar) { Value = startDate });
                parameters.Add(new SqlParameter("@enddate", SqlDbType.VarChar) { Value = endDate });


                var allList = await _clientAdminDashboardServices.GetBookingCountDetail(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
                var total = allList.FirstOrDefault()?.TotalRecords ?? 0;
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = total,
                    aaData = allList
                });
            }
            catch (Exception ex)
            {
                ErrorLog.AddErrorLog(ex, "_GetBookingCountDetail");
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = ""
                });
            }
        }


        public async Task<IActionResult> BookingCountToolTip(string RoomName, int BookingType)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var data = await _clientAdminDashboardServices.GetBookingCountToolTipdetail(HttpContext.Session.GetString("ConnectionString"), RoomName, BookingType);

                    ViewBag.IsmenuEmpty = true;

                    return JsonResponse.GenerateJsonResult(0, "Data not found", data);

                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveEquipment");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        public async Task<IActionResult> MeetingBookingCountToolTip(string MeetingName)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var data = await _clientAdminDashboardServices.GetMeetingCountToolTipdetail(HttpContext.Session.GetString("ConnectionString"), MeetingName);

                    ViewBag.IsmenuEmpty = true;

                    return JsonResponse.GenerateJsonResult(0, "Data not found", data);

                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveEquipment");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCallLogsListForReminder(JQueryDataTableParamModel param)

        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Add(new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar) { Value = HttpContext.Session.GetString("FullName") });
                parameters.Add(new SqlParameter("@isDashboard", SqlDbType.Bit) { Value = 1});
                var allList = await _callLogs.GetCallLogsListForReminder(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
                var total = allList.FirstOrDefault()?.TotalRecords ?? 0;
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = total,
                    aaData = allList
                });
            }
            catch (Exception ex)
            {
                ErrorLog.AddErrorLog(ex, "GetCallLogsListForReminder");
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = ""
                });
            }
        }


        [HttpGet]
        public JsonResult GetBranchList(string userID)
        {
            var branchList = _branchService.getBranchList(HttpContext.Session.GetString("ConnectionString"), userID);
            return Json(branchList.Select(x => new { BranchName = x.BranchName, Id = x.Id }).ToArray());
        }

        [HttpGet]
        public int GetBookingRequestCounts(string userID)
        {
            var counts = _branchService.GetBookingRequestCounts(HttpContext.Session.GetString("ConnectionString"), userID, Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            return counts;
        }
        [HttpGet]
        public JsonResult GetBranchListForUserAccess()
        {
            var branchList = _branchService.getBranchList(HttpContext.Session.GetString("ConnectionString"),"");
            return Json(branchList.Select(x => new MultiSelectDropdownModel { label = x.BranchName, value = x.Id.ToString() }).ToArray());
        }


        [HttpGet]
        public IActionResult ReferalCode()
        {

            var referalLink = _config["CommonProperty:PhysicalUrl"] + @"/ClientAdmin/Dashboard/Register/" + HttpContext.Session.GetString("CompanyCode");
            var model = new GuestUserDto();
            model.ReferalLink = referalLink;
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExternalUser(ExternalUserDto model)
        {
            try {
                string Connectionstring = "";

                Connectionstring = _config["ConnectionStrings:ExternalUserConnection"];
                Connectionstring = Connectionstring.Replace("{DbName}", model.CompanyCode.Replace("#", ""));

                model.Password = RandomString(8);
                var isnertResult = _userService.AddExternalUser(Connectionstring, model);
                if (isnertResult)
                {
                    var resetUrl = _config["CommonProperty:PhysicalUrl"];
                    string emailTemplate = CommonMethod.ReadEmailTemplate(null, _webHostEnvironment.WebRootPath, "CreateUser.html", resetUrl);
                    emailTemplate = emailTemplate.Replace("{UserName}", model.FirstName + "  " + model.LastName);
                    emailTemplate = emailTemplate.Replace("{UserEmail}", model.Email);
                    emailTemplate = emailTemplate.Replace("{UserPassword}", model.Password);
                    emailTemplate = emailTemplate.Replace("{CompanyCode}", model.CompanyCode.Replace("#", ""));

                    await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                    {
                        ToDisplayName = "Dear ," + model.FirstName,
                        ToAddress = model.Email,
                        Subject = "You are successfully registered with Integr8ed",
                        BodyText = emailTemplate
                    });
                    return JsonResponse.GenerateJsonResult(1, " You're successfully registered , please check your email and find the credentials to login to the system ", _config["CommonProperty:PhysicalUrl"]);

                }
                else
                {
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }

            }
            catch (Exception ex) {

                return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
            }


        }


        [HttpGet]
        public IActionResult BookRoom()
        {

            return View();
        }


        public bool CheckForEmail(string Email, long ID, string Companycode)
        {
            string Connectionstring = "";

            Connectionstring = _config["ConnectionStrings:ExternalUserConnection"];
            Connectionstring = Connectionstring.Replace("{DbName}", Companycode);


            bool isExist;
            var result = _companyUser.GetSingle(Connectionstring,
                x => x.Email.ToLower().Equals(Email.ToLower()) && x.IsDelete == false);
            if (result != null)
            {

                isExist = result.Email.ToLower().Trim().Equals(Email.ToLower().Trim()) ? true : false;
                if (isExist && ID != 0)
                {
                    var resultExist = _companyUser.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                        x => x.Email.ToLower().Equals(Email.ToLower()) && x.Id == ID && x.IsDelete == false);
                    return resultExist == null ? false : true;
                }
                else
                {
                    return result == null ? true : false;
                }
            }
            else
            {
                return result == null ? true : false;
            }
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion
    }
}