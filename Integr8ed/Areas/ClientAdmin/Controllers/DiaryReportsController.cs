using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AutoMapper.Configuration;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface;
using Integr8ed.Service.Interface.ClientAdmin;
using Integr8ed.Utility.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Rotativa.AspNetCore;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class DiaryReportsController : BaseController<DiaryReportsController>
    {

        #region Fields
        private readonly IVisitorsService _IVisitors;
        private readonly IErrorLogService _errorLog;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRoomTypesService _roomTypesService;
        private readonly IBookingDetailServices _bdService;

        #endregion

        #region ctor
        public DiaryReportsController(IVisitorsService visitors, IErrorLogService errorLog, IWebHostEnvironment webHostEnvironment, IRoomTypesService roomTypesService,
             IBookingDetailServices bdService )
        {
            _IVisitors = visitors;
            _errorLog = errorLog;
            _webHostEnvironment = webHostEnvironment;
            _roomTypesService = roomTypesService;
            _bdService = bdService;
        }
        #endregion

        #region Methods
        public IActionResult Index()
        {
            var BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
            var RoomList = _roomTypesService.GetDiaryRoomList(HttpContext.Session.GetString("ConnectionString"), BranchId);

            var RoomSelectList = RoomList.Select(x => new SelectListItem()
            {
                Text = x.Title.ToString(),
                Value = x.Id.ToString()

            }).OrderBy(x => x.Text).ToList();
            ViewBag.RoomList = RoomSelectList;


            return View();
        }


        public async Task<IActionResult> GetDiaryRoomList(RoomTypeListDto model, int isCan)
        {
            try
            {
                var BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
                var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@SelectedDate",SqlDbType.VarChar){Value =model.BookingDate},
                    new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = model.RoomTypeId},
                    new SqlParameter("@BranchId",SqlDbType.BigInt){Value = BranchId},
                    new SqlParameter("@BookingStatusId",SqlDbType.BigInt){ Value =model.BookingStatus },
                    new SqlParameter("@IsCan",SqlDbType.Bit){Value = isCan},
                };

                var RoomList = await _roomTypesService.GetWeeklyDiaryReport(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
                return JsonResponse.GenerateJsonResult(1, "Success", RoomList);
            }
            catch (Exception e)
            {

                return JsonResponse.GenerateJsonResult(0, "Something went wrong", null);
            }
        }


        [HttpPost]
        public JsonResult getBookingInfo(string BookingId)
        {
            if (BookingId != null)
            {
                var booking = _bdService.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == Convert.ToInt64(BookingId));
                return Json(booking);
            }
            else
                return Json("");
        }


        public IActionResult GetMonthlyDiaryReport()
        {
            var BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
            var RoomList = _roomTypesService.GetDiaryRoomList(HttpContext.Session.GetString("ConnectionString"), BranchId);

            ViewBag.RoomList = RoomList.Select(x => new SelectListItem()
            {
                Text = x.Title.ToString(),
                Value = x.Id.ToString()

            }).OrderBy(x => x.Text);
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GetMonthlyDiaryList(string BookingDate, long roomtypeId, int BookignStatusId, int isCan)
        {
            try
            {
                var BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
                //var bDate = DateTime.ParseExact(BookingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@Input_Date",SqlDbType.VarChar){Value =BookingDate},
                    new SqlParameter("@BranchId",SqlDbType.BigInt){Value = BranchId},
                    new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = roomtypeId},
                    new SqlParameter("@BookingStatusId",SqlDbType.BigInt){ Value = BookignStatusId },
                    new SqlParameter("@IsCan",SqlDbType.Bit){Value = isCan},

                }.ToArray();

                var slotList = await _roomTypesService.GetMonthlyDiaryList(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
                slotList.ForEach(x => x.Slot = x.Slot.Replace("&lt;", "<"));
                slotList.ForEach(x => x.Slot = x.Slot.Replace("&gt;", ">"));
                var result = slotList.OrderBy(x => x.BookingDate);
                return JsonResponse.GenerateJsonResult(1, "Success", result);
            }
            catch (Exception e)
            {

                return JsonResponse.GenerateJsonResult(0, "Something went wrong", null);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DailyReportViewAsPDF(string BookingDate, int BookingStatusId,int isCan)
        {
            try
            {
                var BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
                var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@SelectedDate",SqlDbType.VarChar){Value =BookingDate},
                    new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = 0},
                    new SqlParameter("@BranchId",SqlDbType.BigInt){Value = BranchId},
                    new SqlParameter("@BookingStatusId",SqlDbType.BigInt){Value = BookingStatusId},
                    new SqlParameter("@IsCan",SqlDbType.Bit){Value = isCan},
                };
                var comparam = new List<SqlParameter>
                {
                    new SqlParameter("@Companycode",SqlDbType.VarChar){Value =HttpContext.Session.GetString("CompanyCode").ToString()},
                };
                var Company = await _roomTypesService.GetOrganizationName(Config.GetValue<string>("ConnectionStrings:Integr8edContext"), comparam.ToArray());
                var RoomList = await _roomTypesService.GetDailyDiaryReportPDF(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
                RoomList.ForEach(x => x.ReportTitle = "Daily Booking Report");
                RoomList.ForEach(x => x.CompanyName = Company.OrganizationName);

                for (int i = 0; i < RoomList.Count(); i++)
                {
                    var decodedMeetingTitle = HttpUtility.HtmlDecode(RoomList[i].MeetingTitle);
                    var decodedUserGroup = HttpUtility.HtmlDecode(RoomList[i].UserGroup);

                    RoomList[i].MeetingTitle = decodedMeetingTitle;
                    RoomList[i].UserGroup = decodedUserGroup;
                }
                string customSwitches = string.Format("--print-media-type --allow {0} --footer-html {0} --footer-spacing -10",
               Url.Action("Footer", "Document", new { area = "" }, "https"));
                var report = new ViewAsPdf("ReportPDF", RoomList)
                {
                    FileName = "DailyReport.pdf",
                    CustomSwitches = customSwitches,
                    PageMargins = { Left = 10, Bottom = 10, Right = 10, Top = 10 },
                };
                return report;
            }
            catch (Exception e)
            {
                return JsonResponse.GenerateJsonResult(0, "Something went wrong", null);
            }

        }

        [HttpGet]
        public async Task<IActionResult> DemoViewAsPDF(string BookingDate, long RoomTypeId, int BookingStatusId,int isCan)
        {
            try
            {
                var BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
                var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@SelectedDate",SqlDbType.VarChar){Value =BookingDate},
                    new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = RoomTypeId},
                    new SqlParameter("@BranchId",SqlDbType.BigInt){Value = BranchId},
                    new SqlParameter("@BookingStatusId",SqlDbType.BigInt){ Value = BookingStatusId },
                    new SqlParameter("@IsCan",SqlDbType.Bit){Value = isCan},
                };
                var comparam =  new List<SqlParameter>
                {
                    new SqlParameter("@Companycode",SqlDbType.VarChar){Value =HttpContext.Session.GetString("CompanyCode").ToString()},
                };
                var Company = await _roomTypesService.GetOrganizationName(Config.GetValue<string>("ConnectionStrings:Integr8edContext"), comparam.ToArray());
                var RoomList = await _roomTypesService.GetWeeklyDiaryReportPDF(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
                RoomList.ForEach(x => x.ReportTitle = "Weekly Booking Report");
                RoomList.ForEach(x => x.CompanyName = Company.OrganizationName);
                
                for (int i = 0; i < RoomList.Count(); i++)
                {
                    var decodedMeetingTitle = HttpUtility.HtmlDecode(RoomList[i].MeetingTitle);
                    var decodedUserGroup = HttpUtility.HtmlDecode(RoomList[i].UserGroup);

                    RoomList[i].MeetingTitle = decodedMeetingTitle;
                    RoomList[i].UserGroup = decodedUserGroup;
                }
                string customSwitches = string.Format("--print-media-type --allow {0} --footer-html {0} --footer-spacing -10",
               Url.Action("Footer", "Document", new { area = "" }, "https"));
                var report = new ViewAsPdf("ReportPDF", RoomList)
                {
                    FileName = "Weekly.pdf",
                    CustomSwitches = customSwitches,
                    PageMargins = { Left = 10, Bottom = 10, Right = 10, Top = 10 },
                };
                return report;
                // return new ViewAsPdf("ReportPDF", RoomList);
            }
            catch (Exception e)
            {

                return JsonResponse.GenerateJsonResult(0, "Something went wrong", null);
            }

        }

        [HttpGet]
        public async Task<IActionResult> DemoViewAsMonthPDF(string BookingDate, long RoomTypeId, int BookignStatusId, int isCan)
        {
            try
            {
                var BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
                var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@SelectedDate",SqlDbType.VarChar){Value =BookingDate},
                    new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = RoomTypeId},
                    new SqlParameter("@BranchId",SqlDbType.BigInt){Value = BranchId},
                    new SqlParameter("@BookingStatusId",SqlDbType.BigInt){ Value = BookignStatusId },
                    new SqlParameter("@IsCan",SqlDbType.Bit){Value = isCan},
                };
                var comparam = new List<SqlParameter>
                {
                    new SqlParameter("@Companycode",SqlDbType.VarChar){Value =HttpContext.Session.GetString("CompanyCode").ToString()},
                };
                var Company = await _roomTypesService.GetOrganizationName(Config.GetValue<string>("ConnectionStrings:Integr8edContext"), comparam.ToArray());
                var RoomList = await _roomTypesService.GetMonthlyDiaryListPDF(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
                RoomList.ForEach(x => x.ReportTitle = "Monthly Booking Report" );
                RoomList.ForEach(x => x.CompanyName = Company.OrganizationName);


                for (int i=0;i< RoomList.Count(); i++)
                {
                    var decodedMeetingTitle = HttpUtility.HtmlDecode(RoomList[i].MeetingTitle);
                    var decodedUserGroup = HttpUtility.HtmlDecode(RoomList[i].UserGroup);

                    RoomList[i].MeetingTitle = decodedMeetingTitle;
                    RoomList[i].UserGroup = decodedUserGroup;
                }
                string customSwitches = string.Format("--print-media-type --allow {0} --footer-html {0} --footer-spacing -10",
                Url.Action("Footer", "Document", new { area = "" }, "https"));
                var report = new ViewAsPdf("ReportPDF", RoomList)
                {
                    FileName = "Monthly.pdf",
                    CustomSwitches = customSwitches,
                    PageMargins = { Left = 10, Bottom = 10, Right = 10, Top = 10 },
                };
              //  return View("ReportPDF", RoomList);
                return report;
                // return new ViewAsPdf("ReportPDF", RoomList);
            }
            catch (Exception e)
            {

                return JsonResponse.GenerateJsonResult(0, "Something went wrong", null);
            }

        }

        #endregion

        #region Common
        #endregion
    }
}
