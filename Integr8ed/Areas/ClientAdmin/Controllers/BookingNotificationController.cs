using ClosedXML.Excel;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Models;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Enums;
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static Integr8ed.Service.Enums.GlobalEnums;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class BookingNotificationController : BaseController<BookingNotificationController>
    {
        private readonly IBookingNotificationServices _bookingNotificationServices;
        private IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IErrorLogService _errorLog;
        private readonly IBookingDetailServices _bookingDetail;
        private readonly IUserService _user;
        private readonly IRoomTypesService _roomTypes;

        public BookingNotificationController(IBookingNotificationServices bookingNotificationServices, IConfiguration config, EmailService emailService, IWebHostEnvironment webHostEnvironment,
            IErrorLogService errorLog, IBookingDetailServices bookingDetailServices, IRoomTypesService roomTypesService, IUserService user)
        {
            _bookingNotificationServices = bookingNotificationServices;
            _config = config;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _errorLog = errorLog;
            _bookingDetail = bookingDetailServices;
            _user = user;
            _roomTypes = roomTypesService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveBookingList(JQueryDataTableParamModel param)
        {
            try
            {
                var allList = _bookingNotificationServices.GetBookingList(HttpContext.Session.GetString("ConnectionString")).Result;
                var total = allList.Count();
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
                ErrorLog.AddErrorLog(ex, "Get GetActiveBookingList");
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = ""
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeBookingNotifyNo(int id = 0, int no = 0)
        {
            try
            {
                var objResult = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                objResult.NotifyDays = no;

                DateTime NotifyDate = objResult.BookingDate.Date.AddDays(-no);

                //if (objResult.ExternalBookingClientId.HasValue && NotifyDate.Day >= DateTime.Now.Day && NotifyDate.Year == DateTime.Now.Year)
                if (objResult.ExternalBookingClientId.HasValue && NotifyDate.Day >= DateTime.Now.Day && NotifyDate.Year == DateTime.Now.Year && NotifyDate.Month == DateTime.Now.Month)
                {
                #region update NotifyDays 

                var bookingDetail = await _bookingDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));

                #endregion

                var model = new BookingDetailsDto();
                model = Mapper.Map<BookingDetailsDto>(bookingDetail);
                model.BookingDateS = bookingDetail.BookingDate.Date.ToString("dd/MM/yyyy");

                await SendNotifyEmail(model);
                }
                else
                {
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.ExceedNoLimit);
                }
                return JsonResponse.GenerateJsonResult(1, ResponseConstants.BookingDetailsUpdated);
            }
            catch (Exception ex)
            {
                ErrorLog.AddErrorLog(null, "Error in Update Booking Details");
                return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
            }

        }

        public async Task SendNotifyEmail(BookingDetailsDto model)
        {
            try
            {
                var user = _user.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.ExternalBookingClientId);
                var roomTypeList = _roomTypes.GetRoomTypeListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
                var ROOMNAME = roomTypeList.FirstOrDefault(x => x.Id == model.RoomTypeId).Title;

                #region sent mail
                var resetUrl = _config["CommonProperty:PhysicalUrl"];
                var confirmationUrl = _config["CommonProperty:PhysicalUrl"] + "/ClientAdmin/BookingNotification/BookingStatusMessage?IsConfirmed=true&id=" + model.Id;
                var cancelationUrl = _config["CommonProperty:PhysicalUrl"] + "/ClientAdmin/BookingNotification/BookingStatusMessage?IsConfirmed=false&id=" + model.Id+ "&CurrentComCode="+ HttpContext.Session.GetString("CompanyCode");
                string emailTemplate = CommonMethod.ReadEmailTemplate(_errorLog, _webHostEnvironment.WebRootPath, "ConfirmBookingNotify.html", resetUrl);
                emailTemplate = emailTemplate.Replace("{CancelationURL}", cancelationUrl);
                emailTemplate = emailTemplate.Replace("{ConfirmURL}", confirmationUrl);
                emailTemplate = emailTemplate.Replace("{UserName}", user.FirstName + "  " + user.LastName);
                emailTemplate = emailTemplate.Replace("{RoomType}", ROOMNAME);
                emailTemplate = emailTemplate.Replace("{AdminEmail}", HttpContext.Session.GetString("UserName").ToString());
                emailTemplate = emailTemplate.Replace("{BookingDate}", model.BookingDateS + " From : " + model.StartTime + "  To: " + model.FinishTime);

                var file = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + @"/wwwroot/Uploads/AddVisitor.xls");
                await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                {
                    ToDisplayName = "Dear ," + user.FirstName,
                    ToAddress = user.Email,
                    Subject = "Integr8ed Room Booking Notify",
                    BodyText = emailTemplate,
                    Attachment = file,
                    AttachmentName = "VisitorList.xlsx",
                });

                #endregion
            }
            catch (Exception ex)
            {

            }
        }

        public IActionResult BookingStatusMessage(bool isConfirmed, string id, string CurrentComCode="0")
        {
            ViewBag.IsConfirmed = isConfirmed;
            ViewBag.Id = id;
            ViewBag.CurrentComCode = CurrentComCode;
            return View();
        }
        //public IActionResult BookingStatusMessage()
        //{
        //    return View();
        //}
    }
}
