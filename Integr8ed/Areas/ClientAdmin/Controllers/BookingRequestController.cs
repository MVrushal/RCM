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
using Newtonsoft.Json;
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
    public class BookingRequestController : BaseController<BookingRequestController>
    {


        private readonly IbookingRequestService _bookingService;
        private readonly IBookingDetailServices _bookingDetail;
        private readonly IBookingStatusService _bookingStatus;
        private readonly IUserService _user;
        private readonly IRoomTypesService _roomTypes;
        private readonly IUserGroupServices _userGroup;
        private readonly IMeetingTypeServices _meetingType;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEquipmentRequiredForBookingServices _equipmentRequiredForBooking;
        public BookingRequestController(IWebHostEnvironment webHostEnvironment, IConfiguration config, IRoomTypesService roomTypesService, EmailService emailService, IUserGroupServices userGroupServices,
            IMeetingTypeServices meetingTypeServices, IbookingRequestService service,
            IBookingDetailServices bookingDetailServices, IBookingStatusService bookingStatus,
            IEquipmentRequiredForBookingServices equipmentRequiredForBookingServices,
            IUserService user
            )
        {
            _bookingStatus = bookingStatus;
            _roomTypes = roomTypesService;
            _userGroup = userGroupServices;
            _meetingType = meetingTypeServices;
            _webHostEnvironment = webHostEnvironment;
            _bookingService = service;
            _emailService = emailService;
            _user = user;
            _config = config;
            _equipmentRequiredForBooking = equipmentRequiredForBookingServices;
            _bookingDetail = bookingDetailServices;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult _uploadVisitor(long id)
        {
            
            return View(@"Components/_uploadVisitor", new BranchDto { Id=id});
        }


        [HttpGet]
        public async Task<IActionResult> GetRequestList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _bookingService.GetRequestList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "Get GetRequestList");
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
        public async Task<IActionResult> _AddEditBookingDetails(long id, bool isView)
        {
            try
            {
                var userList = await _user.GetUsersList(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
                if (userList.Count() != 0)
                {
                    ViewBag.IsUserEmpty = false;
                    ViewBag.UserList = userList.Select(x => new SelectListItem()
                    {
                        Text = x.FirstName + " " + x.LastName,
                        Value = x.Id.ToString()
                    }).OrderBy(x => x.Text);
                }
                else
                {
                    ViewBag.IsUserEmpty = true;
                }

                var roomTypeList = _roomTypes.GetRoomTypeListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
                var tempView = new BookingDetailsDto();
                if (roomTypeList.Count() != 0)
                {
                    ViewBag.IsRoomTypeEmpty = false;
                    ViewBag.RoomType = roomTypeList.Select(x => new SelectListItem()
                    {
                        Text = x.Title,
                        Value = x.Id.ToString()
                    }).OrderBy(x => x.Text);
                }
                else
                {
                    ViewBag.IsRoomTypeEmpty = true;
                    tempView.IsView = true;
                }

                var userGroupList = _userGroup.GetUserGroupListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
                if (userGroupList.Count() != 0)
                {
                    ViewBag.IsUserGroupEmpty = false;
                    ViewBag.UserGroup = userGroupList.Select(x => new SelectListItem()
                    {
                        Text = x.Title,
                        Value = x.Id.ToString()
                    }).OrderBy(x => x.Text);
                }
                else
                {
                    ViewBag.IsUserGroupEmpty = true;
                    tempView.IsView = true;
                }

                var meetingTypeList = await _meetingType.GetMeetingListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
                if (meetingTypeList.Count() != 0)
                {
                    ViewBag.IsMeetingTypeEmpty = false;
                    ViewBag.MeetingType = meetingTypeList.Select(x => new SelectListItem()
                    {
                        Text = x.Title,
                        Value = x.Id.ToString()

                    }).OrderBy(x => x.Text);
                }
                else
                {
                    ViewBag.IsMeetingTypeEmpty = true;
                    tempView.IsView = true;
                }

                //var BookingStatusList = System.Enum.GetValues(typeof(GlobalEnums.BookingStatus)).Cast<GlobalEnums.BookingStatus>()
                //        .Select(d => (d, (int)d))
                //        .ToList();

                //if (BookingStatusList.Count() != 0)
                //{
                //    ViewBag.IsBookingStatusEmpty = false;
                //    ViewBag.BookingStatus = BookingStatusList.Select(x => new SelectListItem()
                //    {
                //        Text = x.d.ToString(),
                //        Value = x.Item2.ToString()

                //    }).OrderBy(x => x.Text);
                //}
                //else
                //{
                //    ViewBag.IsBookingStatusEmpty = true;
                //    tempView.IsView = true;
                //}
                
                var BookingStatusList = _bookingStatus.GetBookingStatusListForDropDown(HttpContext.Session.GetString("ConnectionString"));

                if (BookingStatusList.Count() != 0)
                {
                    ViewBag.IsBookingStatusEmpty = false;
                    ViewBag.BookingStatus = BookingStatusList.Select(x => new SelectListItem()
                    {
                        Text = x.Status.ToString(),
                        Value = x.Id.ToString()

                    }).OrderBy(x => x.Text);
                }
                else
                {
                    ViewBag.IsBookingStatusEmpty = true;
                    tempView.IsView = true;
                }

                if (id == 0) return View(@"Components/_AddEditBookingDetails", new BookingDetailsDto { Id = id, IsView = isView, StartTime = "00:00" });
                var objResult = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                tempView = Mapper.Map<BookingDetailsDto>(objResult);
                tempView.BookingDateS = objResult.BookingDate.Date.ToString("dd/MM/yyyy");
                tempView.Id = objResult.Id;
                tempView.IsView = isView;
                return View(@"Components/_AddEditBookingDetails", tempView);
            }
            catch (Exception ex)
            {
                return View(@"Components/_AddEditBookingDetails", new BookingDetailsDto { Id = id, IsView = false }); ;
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEditBookingDetails(BookingDetailsDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        BookingDetails BD = new BookingDetails();
                        BD.UserGroupId = model.UserGroupId;
                        BD.MeetingTypeId = model.MeetingTypeId;
                        BD.RoomTypeId = model.RoomTypeId;
                        BD.BookingDate = DateTime.ParseExact(model.BookingDateS, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        BD.StartTime = model.StartTime;
                        BD.FinishTime = model.FinishTime;
                        BD.TitleOfMeeting = model.TitleOfMeeting;
                        BD.NumberOfAttending = model.NumberOfAttending;
                        BD.CarSpaceRequired = model.CarSpaceRequired;
                        BD.HouseKeepingRequired = model.HouseKeepingRequired;
                        BD.SecurityRequired = model.SecurityRequired;
                        BD.Cost = model.Cost;
                        BD.LayoutInformation = model.LayoutInformation;
                        BD.TobeInvoiced = model.TobeInvoiced;
                        BD.TechnicianOnSite = model.TechnicianOnSite;
                        BD.DisabledAccess = model.DisabledAccess;
                        BD.ReturnedBookingForm = model.ReturnedBookingForm;
                        BD.BookingContact = model.BookingContact;
                        BD.Mobile = model.Mobile;
                        BD.DateFromSent = model.DateFromSent;
                        BD.BookingStatus = model.BookingStatus;
                        BD.AdditionalInformation = model.AdditionalInformation;
                        BD.CancellationDetail = model.CancellationDetail;
                        BD.ExternalBookingClientId = model.ExternalBookingClientId;
                        BD.CatererRemark = model.CatererRemark;
                        BD.IsActive = true;
                        BD.Email = model.Email;
                        BD.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());

                        var bookingDetail = await _bookingDetail.InsertAsync(HttpContext.Session.GetString("ConnectionString"), BD, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (bookingDetail != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.BookingDetailsCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Booking Details");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {
                        bool IsConfir = true;
                        var objResult = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        if (objResult.BookingStatus != model.BookingStatus && model.BookingStatus == 1)
                        {
                            var Parameters = new List<SqlParameter>
                                  {
                                    new SqlParameter("@BookingDetailId",SqlDbType.Int){Value = objResult.Id },
                                  };

                            var costResult = await _equipmentRequiredForBooking.CheckBookingStatusEquipmentAvalable(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());

                            string EquipmentAvalable = "";
                            foreach (var element in costResult)
                            {
                                if (element.NoOfItemINBooking > element.NoOfItemAvalable)
                                {
                                    EquipmentAvalable = EquipmentAvalable + "\n" + element.EquipmetName + " is only " + element.NoOfItemAvalable + " available and requirement is  " + element.NoOfItemINBooking;

                                    IsConfir = false;
                                }

                            }
                            if (IsConfir == false)
                            {
                                //   return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                                return JsonResponse.GenerateJsonResult(0, EquipmentAvalable);
                            }

                        }

                        objResult.UserGroupId = model.UserGroupId;
                        objResult.MeetingTypeId = model.MeetingTypeId;
                        objResult.RoomTypeId = model.RoomTypeId;
                   
                        objResult.StartTime = model.StartTime;
                        objResult.FinishTime = model.FinishTime;
                        objResult.TitleOfMeeting = model.TitleOfMeeting;
                        objResult.NumberOfAttending = model.NumberOfAttending;
                        objResult.CarSpaceRequired = model.CarSpaceRequired;
                        objResult.HouseKeepingRequired = model.HouseKeepingRequired;
                        objResult.SecurityRequired = model.SecurityRequired;
                        objResult.Cost = model.Cost;
                        objResult.LayoutInformation = model.LayoutInformation;
                        objResult.TobeInvoiced = model.TobeInvoiced;
                        objResult.TechnicianOnSite = model.TechnicianOnSite;
                        objResult.DisabledAccess = model.DisabledAccess;
                        objResult.ReturnedBookingForm = model.ReturnedBookingForm;
                        objResult.BookingContact = model.BookingContact;
                        objResult.Mobile = model.Mobile;
                        objResult.DateFromSent = model.DateFromSent;
                        objResult.BookingStatus = model.BookingStatus;
                        objResult.AdditionalInformation = model.AdditionalInformation;
                        objResult.CancellationDetail = "";
                     //  objResult.ExternalBookingClientId = model.ExternalBookingClientId;
                        objResult.CatererRemark = model.CatererRemark;

                        var bookingDetail = await _bookingDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (bookingDetail != null)
                        {

                            var user = _user.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == bookingDetail.ExternalBookingClientId);
                            var admin = _user.GetCompanyAdmin(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
                            var resetUrl = _config["CommonProperty:PhysicalUrl"];
                            var roomTypeList = _roomTypes.GetRoomTypeListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));

                            var ROOMNAME = roomTypeList.FirstOrDefault(x => x.Id == model.RoomTypeId).Title;
                            if (bookingDetail.BookingStatus == 3)
                            {
                                string emailTemplate = CommonMethod.ReadEmailTemplate(null, _webHostEnvironment.WebRootPath, "Bookingreject.html", resetUrl);
                                emailTemplate = emailTemplate.Replace("{UserName}", user.FirstName + "  " + user.LastName);
                                emailTemplate = emailTemplate.Replace("{RoomType}", ROOMNAME);
                                emailTemplate = emailTemplate.Replace("{BookingDate}", model.BookingDateS + " From : "+ model.StartTime+"  To: "+model.FinishTime);
                                emailTemplate = emailTemplate.Replace("{AddressLine1}", admin.AddressLine1);
                                emailTemplate = emailTemplate.Replace("{AddressLine1}", admin.AddressLine2);

                                await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                                {
                                    ToDisplayName = "Dear ," + user.FirstName,
                                    ToAddress = user.Email,
                                    Subject = "Integr8ed Room Booking Request Status",
                                    BodyText = emailTemplate
                                });
                            }
                            else if (bookingDetail.BookingStatus == 1) {

                                string emailTemplate = CommonMethod.ReadEmailTemplate(null, _webHostEnvironment.WebRootPath, "Bookingraccept.html", resetUrl);
                                emailTemplate = emailTemplate.Replace("{UserName}", user.FirstName + "  " + user.LastName);
                                emailTemplate = emailTemplate.Replace("{RoomType}", ROOMNAME);
                                emailTemplate = emailTemplate.Replace("{AdminEmail}", HttpContext.Session.GetString("UserName").ToString());
                                emailTemplate = emailTemplate.Replace("{BookingDate}", model.BookingDateS + " From : " + model.StartTime + "  To: " + model.FinishTime);
                                emailTemplate = emailTemplate.Replace("{AddressLine1}", admin.AddressLine1);
                                emailTemplate = emailTemplate.Replace("{AddressLine1}", admin.AddressLine2);
                                //var file = GetvisitorList(bookingDetail.Id);  Fatch Visitor List
                                var file = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory()+ @"/wwwroot/Uploads/AddVisitor.xls");
                                await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                                {
                                    ToDisplayName = "Dear ," + user.FirstName,
                                    ToAddress = user.Email,
                                    Subject = "Integr8ed Room Booking Request Status",
                                    BodyText = emailTemplate,
                                    Attachment = file,
                                    AttachmentName = "VisitorList.xlsx",
                                });
                            }



                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.BookingDetailsUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Booking Details");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Booking Details");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }


        public byte[] GetvisitorList(long bookingId)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "VisitorList.xlsx";

            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet =
                workbook.Worksheets.Add("Report");

                worksheet.Cell(1, 1).Value = "Name";
                worksheet.Cell(1, 2).Value = "Surname";
                worksheet.Cell(1, 3).Value = "Post code";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Telephone";
                worksheet.Cell(1, 6).Value = "Mobile";
                worksheet.Cell(1, 7).Value = "Notes";
                worksheet.Cell(1, 8).Value = "Address";
           
                int ExcelRowID = 2;



                long previousRowId = 0;
                var vlist = _bookingDetail.GetVisitorList(HttpContext.Session.GetString("ConnectionString"), bookingId);
                var resultArray = vlist.ToArray();
                for (int index = 0; index < vlist.Count; index++)
                {

                   
                    worksheet.Cell(ExcelRowID, 1).Value = vlist[index].Name;
                    worksheet.Cell(ExcelRowID, 2).Value = vlist[index].SurName;
                    worksheet.Cell(ExcelRowID, 3).Value = vlist[index].PostCode;
                    worksheet.Cell(ExcelRowID, 4).Value = vlist[index].Email;
                    worksheet.Cell(ExcelRowID, 5).Value = vlist[index].Telephone;
                    worksheet.Cell(ExcelRowID, 6).Value = vlist[index].Mobile;
                    worksheet.Cell(ExcelRowID, 7).Value = vlist[index].Notes;
                    worksheet.Cell(ExcelRowID, 8).Value = vlist[index].Address;
                   



                    ExcelRowID = ExcelRowID + 1;

                }


                var isRowCompleted = false;
            




                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(fileName);
                    //var content = stream.ToArray();
                    //File (content, contentType, fileName);
                    // return Content("TEst");
                }



                return System.IO.File.ReadAllBytes(fileName);

                //if (System.IO.File.Exists(fileName))
                //    System.IO.File.Delete(fileName);

            }
        }
    }
}
