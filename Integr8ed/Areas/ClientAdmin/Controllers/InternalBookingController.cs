using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Models;
using Integr8ed.Service;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface.ClientAdmin;
using Integr8ed.Utility.Common;
using Integr8ed.Utility.JqueryDataTable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Rotativa.AspNetCore;
using Integr8ed.Service.Enums;
using static Integr8ed.Enums.Enum;
using Integr8ed.Service.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Integr8ed.Data.Utility;
using System.IO;
using ExcelDataReader;
using System.Globalization;
using static Integr8ed.Service.Enums.GlobalEnums;
using Integr8ed.Service.Utility;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class InternalBookingController : BaseController<InternalBookingController>
    {
        #region Fields

        private readonly IBranchService _branchDetails;
        private readonly IRoomTypesService _roomTypes;
        private readonly IBookingStatusService _bookingStatus;
        private readonly IUserGroupServices _userGroup;
        private readonly IMeetingTypeServices _meetingType;
        private readonly IBookingDetailServices _bookingDetail;
        private readonly ICatering_RequirementsServices _CRservice;
        private readonly ICateringDetailsServices _cateringDetails;
        private readonly IMenuServices _menu;
        private readonly IInvoiceDetailServices _invoiceDetail;
        private readonly IInvoiceServices _invoice;
        private readonly IItemsToInvoiceServices _itemsToInvoice;
        private readonly ISecurityServices _security;
        private readonly ICat_Req_MenuServices _cat_Req_Menu;
        private readonly INotesServices _notes;
        private readonly IContactDetailsServices _contactDetails;
        private readonly IVisitorBookingServices _visitorBooking;
        private readonly IVisitorsService _visitors;
        private readonly ICatererMenuServices _catererMenu;
        private readonly ICallLogsServices _callLogs;
        private readonly IEntryTypeServices _entryType;
        private readonly IProfileServices _profile;
        private readonly IEquipmentRequiredForBookingServices _equipmentRequiredForBooking;
        private readonly IEquipServices _equip;
        private readonly IUserService _user;
        private IConfiguration _config;
        private IHostingEnvironment Environment;
        private readonly IBookingNotificationServices _bookingNotification;
        private readonly IErrorLogService _errorLog;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EmailService _emailService;


        #endregion

        #region ctor
        public InternalBookingController(IRoomTypesService roomTypesService, IBookingStatusService bookingStatus, IUserGroupServices userGroupServices,
            IMeetingTypeServices meetingTypeServices, IBookingDetailServices bookingDetailServices,
            ICateringDetailsServices cateringDetails, IMenuServices menuServices,
            ICatering_RequirementsServices catering_RequirementsServices, ISecurityServices security, ICat_Req_MenuServices cat_Req_MenuServices,
            IInvoiceDetailServices invoiceDetailServices, IInvoiceServices invoiceServices, IItemsToInvoiceServices itemsToInvoiceServices,
            INotesServices notesServices, IContactDetailsServices contactDetailsServices, IVisitorBookingServices visitorBookingServices,
            IVisitorsService visitorsService, ICatererMenuServices catererMenu, ICallLogsServices callLogs,
            IEntryTypeServices entryType, IProfileServices profile, IEquipmentRequiredForBookingServices equipmentRequiredForBookingServices,
            IEquipServices equip, IUserService user, IConfiguration config,
            IHostingEnvironment _environment, IBookingNotificationServices bookingNotification,
            IBranchService branchService, IErrorLogService errorLog, IWebHostEnvironment webHostEnvironment, EmailService emailService

            )
        {

            _roomTypes = roomTypesService;
            _bookingStatus = bookingStatus;
            _userGroup = userGroupServices;
            _meetingType = meetingTypeServices;
            _bookingDetail = bookingDetailServices;
            _cateringDetails = cateringDetails;
            _menu = menuServices;
            _CRservice = catering_RequirementsServices;
            _invoiceDetail = invoiceDetailServices;
            _invoice = invoiceServices;
            _itemsToInvoice = itemsToInvoiceServices;
            _security = security;
            _cat_Req_Menu = cat_Req_MenuServices;
            _notes = notesServices;
            _contactDetails = contactDetailsServices;
            _visitorBooking = visitorBookingServices;
            _visitors = visitorsService;
            _catererMenu = catererMenu;
            _callLogs = callLogs;
            _entryType = entryType;
            _profile = profile;
            _equipmentRequiredForBooking = equipmentRequiredForBookingServices;
            _equip = equip;
            _user = user;
            _config = config;
            Environment = _environment;
            _bookingNotification = bookingNotification;
            _branchDetails = branchService;
            _errorLog = errorLog;
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService;
        }
        #endregion

        #region Method
        public async Task<IActionResult> Index(long selectedFromDiaryId = 0)
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);

            HttpContext.Session.Remove("BookingDetailId");
            HttpContext.Session.Remove("BookingDateFromSent");
            HttpContext.Session.Remove("BookingContact");
            //var BookingStatusList = System.Enum.GetValues(typeof(GlobalEnums.BookingStatus)).Cast<GlobalEnums.BookingStatus>()
            //        .Select(d => (d, (int)d))
            //        .ToList();
            var BookingStatusList = _bookingStatus.GetBookingStatusListForDropDown(HttpContext.Session.GetString("ConnectionString"));
            if (BookingStatusList.Count() != 0)
            {
                ViewBag.IsBookingStatusEmpty = false;
                var data = BookingStatusList.Select(x => new SelectListItem()
                {
                    Text = x.Status.ToString(),
                    Value = x.Id.ToString()

                }).OrderBy(x => x.Text);
                ViewBag.BookingStatus = JsonConvert.SerializeObject(data);
            }
            else
            {
                ViewBag.IsBookingStatusEmpty = true;
            }
            var userList = await _user.GetUsersList(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            if (userList.Count() != 0)
            {
                ViewBag.IsUserEmpty = false;

                var userdata = userList.Select(x => new SelectListItem()
                {
                    Text = x.FirstName + " " + x.LastName,
                    Value = x.Id.ToString()
                }).OrderBy(x => x.Text);
                ViewBag.userList = JsonConvert.SerializeObject(userdata);
            }
            else
            {
                ViewBag.IsUserEmpty = true;
            }

            return View(selectedFromDiaryId);

        }
        #endregion

        #region booking details

        [HttpGet]
        public async Task<IActionResult> _AddEditBookingDetails(string startTime, string bookingDate, long id, bool isView, bool isInternalBooking)
        {
            try
            {
                var userList = await _user.GetUsersList(HttpContext.Session.GetString("ConnectionString"), 0);
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
                if (id == 0 & startTime != null) return View(@"Components/_AddEditBookingDetails", new BookingDetailsDto { Id = id, IsView = isView, StartTime = startTime, BookingDateS = bookingDate, IsInternalBooking = isInternalBooking });
                if (id == 0) return View(@"Components/_AddEditBookingDetails", new BookingDetailsDto { Id = id, IsView = isView, StartTime = "00:00", IsInternalBooking = isInternalBooking });
                var objResult = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                tempView = Mapper.Map<BookingDetailsDto>(objResult);
                tempView.BookingDateS = objResult.BookingDate.Date.ToString("dd-MM-yyyy");
                tempView.Id = objResult.Id;
                tempView.DateFromSent = objResult.DateFromSent ?? "";
                tempView.IsView = isView;
                ViewBag.CreatedDate = objResult.CreatedDate.ToString("dd-MM-yyyy");
                ViewBag.IsInternalBooking = isInternalBooking;

                var createdusername = await _user.GetUserName(HttpContext.Session.GetString("ConnectionString"));
                if (createdusername.Count() != 0)
                {
                    ViewBag.IsUserEmpty = false;

                    //ViewBag.CreatedUserName = createdusername.Select(x => new SelectListItem()
                    //{
                    //    Text = x.FirstName + " " + x.LastName,
                    //    Value = x.Id.ToString()
                    //}).OrderBy(x => x.Text).Where(x => x.Value == (objResult.CreatedBy).ToString()).FirstOrDefault();
                    ViewBag.CreateduserName = createdusername.Where(x => x.Id == objResult.CreatedBy).FirstOrDefault().FullName;
                }
                else
                {
                    ViewBag.IsUserEmpty = true;
                }
                //ViewBag.CreatedUserName = ; 

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
                        BD.Email = model.Email;
                        BD.Mobile = model.Mobile;
                        BD.DateFromSent = model.DateFromSent;
                        BD.BookingStatus = model.BookingStatus;
                        BD.AdditionalInformation = model.AdditionalInformation;
                        BD.CancellationDetail = model.CancellationDetail;
                        BD.ExternalBookingClientId = model.ExternalBookingClientId;
                        BD.CatererRemark = model.CatererRemark;
                        BD.IsActive = true;
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
                        objResult.BookingDate = DateTime.ParseExact(model.BookingDateS, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        /*objResult.BookingDate = DateTime.ParseExact(model.BookingDateS, "dd-MM-yyyy", CultureInfo.InvariantCulture); ;*/ // DateTime.ParseExact(model.BookingDate.ToString("MM/dd/yyyy"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
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
                        objResult.Email = model.Email;
                        objResult.Mobile = model.Mobile;
                        objResult.DateFromSent = model.DateFromSent;
                        objResult.BookingStatus = model.BookingStatus;
                        objResult.AdditionalInformation = model.AdditionalInformation;
                        objResult.CancellationDetail = model.CancellationDetail;
                        objResult.ExternalBookingClientId = model.ExternalBookingClientId;
                        objResult.CatererRemark = model.CatererRemark;

                        var bookingDetail = await _bookingDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (bookingDetail != null)
                        {
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

        [HttpPost]
        public async Task<IActionResult> RemoveBookingDetails(int id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    // var CompanyCode = Convert.ToInt64(HttpContext.Session.GetString("CompanyCode").ToString());
                    //if (currentComCode != "0")
                    //{
                    //    var Parameters = new List<SqlParameter>
                    //    {
                    //        new SqlParameter("@Id",SqlDbType.VarChar){Value = id},
                    //        new SqlParameter("@CurrentComCode",SqlDbType.VarChar){Value = CompanyCode}


                    //};

                    //    var costResult = await _bookingNotification.DeleteBookingByCompanyCodeANDId(_config["ConnectionStrings:Integr8edContext"], Parameters.ToArray());
                    //    txscope.Complete();
                    //}
                    //else
                    //{
                    var questionObj = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _bookingDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();
                    //}


                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.BookingDetailsDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "POST-RemoveBookingDetails");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeBookingstatus(long id = 0, int Status = 0)
        {
            BookingDetails BD = new BookingDetails();
            bool IsConfir = true;
            if (Status == 1)
            {
                var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@BookingDetailId",SqlDbType.Int){Value = id },
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

            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var data = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);

                data.BookingStatus = Status;
                var bookingDetail = await _bookingDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), data, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                if (bookingDetail != null)
                {
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

        [HttpGet]
        public async Task<IActionResult> GetBookingDetailList(JQueryDataTableParamModel param, int RoomTypeId, int BookingStatus, string StartDate, string EndDate, int UserName, long sID = 0)
        {
            try
            {
                if ((StartDate == null || EndDate == null) && sID == 0)
                {
                    StartDate = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    EndDate = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                if (sID != 0)
                {
                    StartDate = null;
                    EndDate = null;
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

                //var BookingStatusList = System.Enum.GetValues(typeof(GlobalEnums.BookingStatus)).Cast<GlobalEnums.BookingStatus>()
                //        .Select(d => (d, (int)d))
                //        .ToList();
                var BookingStatusList = _bookingStatus.GetBookingStatusListForDropDown(HttpContext.Session.GetString("ConnectionString"));
                if (BookingStatusList.Count() != 0)
                {
                    ViewBag.IsBookingStatusEmpty = false;
                    ViewBag.BookingStatus = BookingStatusList.Select(x => new SelectListItem()
                    {
                        Text = x.Status,
                        Value = x.Id.ToString()

                    }).OrderBy(x => x.Text);
                }
                else
                {
                    ViewBag.IsBookingStatusEmpty = true;
                    tempView.IsView = true;
                }

                param.sSearch = param.sSearch == null ? "" : param.sSearch.Trim();
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                //  parameters.Add(new SqlParameter("@sID", SqlDbType.BigInt) { Value = sID });
                parameters.Insert(0, new SqlParameter("@sID", SqlDbType.BigInt) { Value = sID });
                parameters.Insert(1, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                parameters.Insert(2, new SqlParameter("@RoomTypeId", SqlDbType.BigInt) { Value = RoomTypeId });
                parameters.Insert(3, new SqlParameter("@BookingStatus", SqlDbType.BigInt) { Value = BookingStatus });
                parameters.Insert(4, new SqlParameter("@StartDate", SqlDbType.VarChar) { Value = StartDate });
                parameters.Insert(5, new SqlParameter("@EndDate", SqlDbType.VarChar) { Value = EndDate });
                parameters.Insert(6, new SqlParameter("@UserName", SqlDbType.BigInt) { Value = UserName });



                var allList = await _bookingDetail.GetBookingDetailList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());

                var total = allList.FirstOrDefault()?.TotalRecords ?? 0;

                //var BookingStatusList = System.Enum.GetValues(typeof(GlobalEnums.BookingStatus)).Cast<GlobalEnums.BookingStatus>()
                //    .Select(d => (d, (int)d))
                //    .ToList();

                //if (BookingStatusList.Count() != 0)
                //{
                //    ViewBag.IsBookingStatusEmpty = false;
                //    var data = BookingStatusList.Select(x => new SelectListItem()
                //    {
                //        Text = x.d.ToString(),
                //        Value = x.Item2.ToString()

                //    }).OrderBy(x => x.Text);
                //    ViewBag.BookingStatus = JsonConvert.SerializeObject(data);
                //}
                //else
                //{
                //    ViewBag.IsBookingStatusEmpty = true;
                //}

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
                ErrorLog.AddErrorLog(ex, "GetBookingDetailList");
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = ""
                });
            }
        }
        //[HttpGet]
        //public async Task<IActionResult> GetBookingDetailList(JQueryDataTableParamModel param, long sID = 0)
        //{
        //    try
        //    {

        //        param.sSearch = param.sSearch == null ? "" : param.sSearch.Trim();
        //        var parameters = CommonMethod.GetJQueryDatatableParamList(param, "bookingDateForDisplay").Parameters;
        //        //  parameters.Add(new SqlParameter("@sID", SqlDbType.BigInt) { Value = sID });
        //        parameters.Insert(0, new SqlParameter("@sID", SqlDbType.BigInt) { Value = sID });
        //        parameters.Insert(1, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
        //        var allList = await _bookingDetail.GetBookingDetailList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());

        //        var total = allList.FirstOrDefault()?.TotalRecords ?? 0;

        //        var BookingStatusList = System.Enum.GetValues(typeof(GlobalEnums.BookingStatus)).Cast<GlobalEnums.BookingStatus>()
        //            .Select(d => (d, (int)d))
        //            .ToList();

        //        if (BookingStatusList.Count() != 0)
        //        {
        //            ViewBag.IsBookingStatusEmpty = false;
        //            var data = BookingStatusList.Select(x => new SelectListItem()
        //            {
        //                Text = x.d.ToString(),
        //                Value = x.Item2.ToString()

        //            }).OrderBy(x => x.Text);
        //            ViewBag.BookingStatus = JsonConvert.SerializeObject(data);
        //        }
        //        else
        //        {
        //            ViewBag.IsBookingStatusEmpty = true;
        //        }

        //        return Json(new
        //        {
        //            param.sEcho,
        //            iTotalRecords = total,
        //            iTotalDisplayRecords = total,
        //            aaData = allList
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.AddErrorLog(ex, "GetBookingDetailList");
        //        return Json(new
        //        {
        //            param.sEcho,
        //            iTotalRecords = 0,
        //            iTotalDisplayRecords = 0,
        //            aaData = ""
        //        });
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> CalculateBookingCostByDate(string bookingDate, string startTime, string finishTime, long roomTypeId)
        {



            var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@BookingDate",SqlDbType.VarChar){Value = bookingDate},
                    new SqlParameter("@StartTime",SqlDbType.VarChar){Value = startTime},
                    new SqlParameter("@FinishTime",SqlDbType.VarChar){Value = finishTime},
                    new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = roomTypeId}
                };

            var costResult = await _bookingDetail.CalculateBookingCostByDate(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
            //Add 2% Vat
            costResult += costResult % 2;

            return JsonResponse.GenerateJsonResult(1, costResult.ToString());
        }

        [HttpPost]
        public void HandleBookingDetail(int id, string bookingContact, string dateFromSent, string securityRequired, string createdUserName, string createdDate)
        {
            HttpContext.Session.SetInt32("BookingDetailId", id);
            HttpContext.Session.SetString("BookingContact", bookingContact ?? "");
            HttpContext.Session.SetString("BookingDateFromSent", dateFromSent ?? "");
            HttpContext.Session.SetString("SecurityReqForBooking", securityRequired ?? "");
            HttpContext.Session.SetString("Bookedby", createdUserName ?? "");
            HttpContext.Session.SetString("CreatedDate", createdDate ?? "");


            //var bookedBy = ViewBag.CreateduserName;
            //HttpContext.Session.SetString("BookedBy", bookedBy  ?? "");
        }

        #endregion

        #region Methods for Catering Requirements

        [HttpGet]
        public async Task<IActionResult> _AddEditCateringRequirements(long id, bool isView)
        {
            var catererList = await _cateringDetails.GetCatererList(HttpContext.Session.GetString("ConnectionString"));
            var tempView = new Catering_RequirementsDto();
            tempView.Id = id;
            tempView.IsView = isView;

            var CRList = await _CRservice.GET_CateReqList(HttpContext.Session.GetString("ConnectionString"), (long)HttpContext.Session.GetInt32("BookingDetailId"));
            var CRObj = await _CRservice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                x => x.Id == id && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.IsDelete == false);

            var NewCRList = new List<Catering_Details>();
            if (catererList.Count() != 0)
            {
                ViewBag.IsCatererDetailEmpty = false;
                if (CRList.Count() != 0)
                {
                    var temp = CRList.Where(x => !x.IsDelete).Select(y => y.CatererId);
                    if (catererList.Where(x => !temp.Contains(x.Id)).Count() != 0)
                        NewCRList.AddRange(catererList.Where(x => !temp.Contains(x.Id)));
                    if (CRObj != null)
                        NewCRList.Add(catererList.FirstOrDefault(x => x.Id == CRObj.CatererId));

                    ViewBag.IsCatererDetailEmpty = NewCRList.Count() == 0 ? true : false;
                    ViewBag.CatererDetail = NewCRList.Select(x => new SelectListItem()
                    {
                        Text = x.CatererName,
                        Value = x.Id.ToString()
                    }).OrderBy(x => x.Text);
                }
                else
                {
                    ViewBag.CatererDetail = catererList.Select(x => new SelectListItem()
                    {
                        Text = x.CatererName,
                        Value = x.Id.ToString()
                    }).OrderBy(x => x.Text);
                }
            }

            else
            {
                ViewBag.IsCatererDetailEmpty = true;
                tempView.IsView = true;
            }

            ViewBag.IsmenuEmpty = true;
            ViewBag.MenuDetail = JsonResponse.GenerateJsonResult(0, "Data not found", JsonConvert.SerializeObject(null));


            if (id == 0) return View(@"Components/_AddEditCateringRequirements", tempView);
            var objResult = await _CRservice.GET_CateReqAndMenuItemById(HttpContext.Session.GetString("ConnectionString"), id);
            tempView = objResult;
            tempView.IsView = isView;
            var strLst = objResult.Cat_Req_Menu.Select(s => s.CatererMenuId.ToString()).ToList();
            var menuItLst = objResult.MenuItemAndCosts.Select(s => s.Menu).ToList();
            tempView.MenuItem = string.Join(" ", menuItLst);
            //string[] str = strLst.ToArray();
            tempView.MenuId = string.Join(",", strLst);
            return View(@"Components/_AddEditCateringRequirements", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditCateringRequirements(Catering_RequirementsDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        Catering_Requirements CR = new Catering_Requirements();
                        CR.CatererId = model.CatererId;
                        CR.Notes = model.Notes;
                        CR.TimeFor = model.TimeFor;
                        CR.TimeCollected = model.TimeCollected;
                        CR.NumberOfPeople = model.NumberOfPeople;
                        CR.Cost = model.Cost;
                        CR.BookingDetailId = HttpContext.Session.GetInt32("BookingDetailId");

                        var catering_Requirements = await _CRservice.InsertAsync(HttpContext.Session.GetString("ConnectionString"), CR, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (catering_Requirements != null)
                        {
                            string[] idLst = model.MenuId.Split(",");
                            foreach (var id in idLst)
                            {
                                Cat_Req_Menu CRM = new Cat_Req_Menu();
                                CRM.CatererMenuId = Convert.ToInt32(id);
                                CRM.Cat_ReqId = catering_Requirements.Id;
                                var cat_Req_Menu = await _cat_Req_Menu.InsertAsync(HttpContext.Session.GetString("ConnectionString"), CRM, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                                if (cat_Req_Menu == null)
                                {
                                    txscope.Dispose();
                                    ErrorLog.AddErrorLog(null, "Error in create Catering Requirement");
                                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                                }
                            }
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.CateringRequirementCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Catering Requirement");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }


                    else
                    {

                        var objResult = await _CRservice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.CatererId = model.CatererId;
                        objResult.Notes = model.Notes;
                        objResult.TimeFor = model.TimeFor;
                        objResult.TimeCollected = model.TimeCollected;
                        objResult.NumberOfPeople = model.NumberOfPeople;
                        objResult.Cost = model.Cost;

                        var catering_Requirements = await _CRservice.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (catering_Requirements != null)
                        {
                            string[] idLst = model.MenuId.Split(",");
                            var catReqMenuLst = await _cat_Req_Menu.GetCat_Req_MenuListByCatReqId(HttpContext.Session.GetString("ConnectionString"), model.Id);

                            foreach (var id in idLst)
                            {
                                var catReqMenuObj = await _cat_Req_Menu.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                                    x => x.CatererMenuId == Convert.ToInt64(id) && x.Cat_ReqId == catering_Requirements.Id);

                                if (catReqMenuObj == null)
                                {
                                    Cat_Req_Menu CRM = new Cat_Req_Menu();
                                    CRM.CatererMenuId = Convert.ToInt64(id);
                                    CRM.Cat_ReqId = catering_Requirements.Id;
                                    var cat_Req_Menu = await _cat_Req_Menu.InsertAsync(HttpContext.Session.GetString("ConnectionString"), CRM, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                                    if (cat_Req_Menu == null)
                                    {
                                        txscope.Dispose();
                                        ErrorLog.AddErrorLog(null, "Error in create Catering Requirement");
                                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                                    }
                                }
                                else
                                {
                                    var test = catReqMenuLst.RemoveAll(x => x.CatererMenuId == catReqMenuObj.CatererMenuId && x.Cat_ReqId == catReqMenuObj.Cat_ReqId);
                                }
                            }
                            foreach (var catReqMenu in catReqMenuLst)
                            {
                                await _cat_Req_Menu.DeleteAsync(HttpContext.Session.GetString("ConnectionString"), catReqMenu, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));

                            }
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.CateringRequirementUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Catering Requirement");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Catering Requirement");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCateringRequirementsList(JQueryDataTableParamModel param)
        {
            try
            {
                //if (HttpContext.Session.GetInt32("BookingDetailId") == null)
                //{
                //    return Json(new
                //    {
                //        param.sEcho,
                //        iTotalRecords = 0,
                //        iTotalDisplayRecords = 0,
                //        aaData = new List<Catering_RequirementsDto>()
                //    });
                //}

                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                //parameters.Add(new SqlParameter("@BookingDetailId", SqlDbType.BigInt) { Value = HttpContext.Session.GetInt32("BookingDetailId") });
                parameters.Insert(0, new SqlParameter("@BookingDetailId", SqlDbType.BigInt) { Value = HttpContext.Session.GetInt32("BookingDetailId") ?? 0 });
                parameters.Insert(1, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _CRservice.GetCatering_RequirementsList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());

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
                ErrorLog.AddErrorLog(ex, "GetCateringRequirementsList");
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
        public IActionResult GetCateringMenu(long CatererId)
        {
            var menuList = _CRservice.GetCateringReqMenu(HttpContext.Session.GetString("ConnectionString"), CatererId);
            return JsonResponse.GenerateJsonResult(1, "", menuList);

        }


        [HttpPost]
        public async Task<IActionResult> RemoveCateringRequirements(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var questionObj = await _CRservice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _CRservice.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.CateringRequirementDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveCateringRequirements");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCatererMenuListByCatererId(long catererId)
        {
            var menus = await _catererMenu.GetCatererMenuListByCatererId(HttpContext.Session.GetString("ConnectionString"), catererId);

            var multiSelectDropdownModels = menus.Select(x => new MultiSelectDropdownModel()
            {
                label = x.DescriptionOFFood + "(" + x.Cost + ")",
                value = x.Id.ToString()
            }).OrderBy(x => x.label).ToList();

            if (multiSelectDropdownModels != null)
            {
                ViewBag.IsmenuEmpty = false;
                ViewBag.MenuDetail = JsonResponse.GenerateJsonResult(1, "Data found", JsonConvert.SerializeObject(multiSelectDropdownModels));
                return JsonResponse.GenerateJsonResult(1, "Data found", JsonConvert.SerializeObject(multiSelectDropdownModels));
            }
            else
            {
                ViewBag.IsmenuEmpty = true;
                ViewBag.MenuDetail = JsonResponse.GenerateJsonResult(0, "Data not found", JsonConvert.SerializeObject(multiSelectDropdownModels));
                return JsonResponse.GenerateJsonResult(0, "Data not found", JsonConvert.SerializeObject(multiSelectDropdownModels));
            }

        }

        [HttpGet]
        public async Task<IActionResult> CatReqViewAsPDF(long id)
        {
            var tempView = new Catering_RequirementsDto();
            var objResult = await _CRservice.GET_CateReqAndMenuItemById(HttpContext.Session.GetString("ConnectionString"), id);

            return new ViewAsPdf("CatReqViewAsPDF", objResult) { FileName = objResult.Id + ".pdf", Model = objResult };
        }




        #endregion

        #region Methods for Invoice Detail


        [HttpGet]
        public async Task<IActionResult> _AddEditInvoiceItem(long id, bool isView, long invoiceDetailId)
        {
            var invoices = await _invoice.GetInvoiceListForDropDown(HttpContext.Session.GetString("ConnectionString"));
            var tempView = new ItemsToInvoiceDto();
            var invoice = new InvoiceDto();

            //var itemToInvoiceList = await _itemsToInvoice.GET_ItemToInvoiceList(HttpContext.Session.GetString("ConnectionString"));
            //var itemToInvoiceObj = _itemsToInvoice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id && x.IsDelete == false && x.InvoiceDetailsId == invoiceDetailId);
            //var NewInvoiceItemList = new List<Invoice>();
            if (invoices.Count() != 0)
            {
                ViewBag.IsinvoiceEmpty = false;
                //if (itemToInvoiceList.Count() != 0)
                //{
                //    var temp = itemToInvoiceList.Where(x => !x.IsDelete).Select(y => y.InvoiceMasterId);
                //    if (invoices.Where(x => temp.Contains(x.Id)).Count() != 0)
                //        NewInvoiceItemList.AddRange(invoices.Where(x => temp.Contains(x.Id)));
                //    if (itemToInvoiceObj != null)
                //        NewInvoiceItemList.Add(invoices.FirstOrDefault(x => x.Id == itemToInvoiceObj.InvoiceMasterId));

                //    ViewBag.IsinvoiceEmpty = NewInvoiceItemList.Count() == 0 ? true : false;
                ViewBag.InvoiceList = invoices.Select(x => new SelectListItem()
                {
                    Text = x.Title,
                    Value = x.Id.ToString()
                }).OrderBy(x => x.Text);


                //}
            }
            else
            {
                ViewBag.IsinvoiceEmpty = true;
                tempView.IsViewForInvItem = true;
            }

            if (id == 0) return View(@"Components/_AddEditInvoiceItem", new ItemsToInvoiceDto { Id = id, IsViewForInvItem = isView, InvoiceDetailsId = invoiceDetailId });

            var itemResult = await _itemsToInvoice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            var objResult = await _invoice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == itemResult.InvoiceMasterId);
            invoice.Title = objResult.Title;
            invoice.Description = objResult.Description;
            invoice.Vate = objResult.Vate;
            invoice.IteamCost = objResult.IteamCost;
            invoice.BudgetRate = objResult.BudgetRate;
            invoice.IsIteamVatable = objResult.IsIteamVatable;
            tempView.Invoice = invoice;
            tempView.Quantity = itemResult.Quantity;
            tempView.InvoiceDetailsId = itemResult.InvoiceDetailsId;
            tempView.InvoiceMasterId = itemResult.InvoiceMasterId;
            tempView.BookingDetailId = itemResult.BookingDetailId;
            tempView.Id = itemResult.Id;
            tempView.IsViewForInvItem = isView;
            return View(@"Components/_AddEditInvoiceItem", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditInvoiceItem(ItemsToInvoiceDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        ItemsToInvoice In = new ItemsToInvoice();
                        In.InvoiceMasterId = model.InvoiceMasterId;
                        In.Quantity = model.Quantity;
                        In.BookingDetailId = HttpContext.Session.GetInt32("BookingDetailId");

                        var invoice = await _itemsToInvoice.InsertAsync(HttpContext.Session.GetString("ConnectionString"), In, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (invoice != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.InvoiceItemCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Invoice Item");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }


                    else
                    {

                        var objResult = await _itemsToInvoice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.InvoiceMasterId = model.InvoiceMasterId;
                        objResult.Quantity = model.Quantity;
                        objResult.BookingDetailId = HttpContext.Session.GetInt32("BookingDetailId");

                        var invoice = await _itemsToInvoice.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (invoice != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.InvoiceItemUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Invoice Item");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }

                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Invoice Item");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveInvoiceItem(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var invoiceObj = await _itemsToInvoice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    invoiceObj.IsDelete = true;
                    await _itemsToInvoice.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), invoiceObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.InvoiceItemDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-InvoiceItemDeleted");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetInvoiceItemList(JQueryDataTableParamModel param, long invoiceDetailId)
        {
            try
            {
                if (param.sSearch == "Y" || param.sSearch == "y")
                {
                    param.sSearch = "1";
                }
                if (param.sSearch == "N" || param.sSearch == "n")
                {
                    param.sSearch = "0";
                }

                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Add(new SqlParameter("@InvoiceDetaillId", SqlDbType.BigInt) { Value = invoiceDetailId });
                //    parameters.Add(new SqlParameter("@BookingId", SqlDbType.BigInt) { Value = HttpContext.Session.GetInt32("BookingDetailId") });

                var allList = await _itemsToInvoice.GetItemsToInvoiceList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetInvoiceItemList");
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
        public async Task<IActionResult> _AddEditInvoiceDetail(long id, bool isView)
        {
            var tempView = new InvoiceDetailDto();
            tempView.Id = id;
            tempView.IsView = isView;
            if (id == 0)
                return View(@"Components/_AddEditInvoiceDetail", tempView);

            else
            {
                var objResult = await _invoiceDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                tempView = Mapper.Map<InvoiceDetailDto>(objResult);
                return View(@"Components/_AddEditInvoiceDetail", tempView);
            }
        }

        private async Task CalculateInvoiceAmountAndGrossAmount()
        {
            var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@BookingId",SqlDbType.BigInt){Value = HttpContext.Session.GetInt32("BookingDetailId")}
                };

            var invItemResult = await _itemsToInvoice.CalculateInvoiceAmountAndGrossAmount(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());

            if (invItemResult != null)
            {
                ViewBag.InvoiceAmount = invItemResult.InvoiceAmount;
                ViewBag.VatAmount = invItemResult.VatAmount;
                ViewBag.GrossAmount = invItemResult.GrossAmount;
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddEditInvoiceDetail(InvoiceDetailDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        InvoiceDetail ID = new InvoiceDetail();
                        ID.InvoiceAddress = model.InvoiceAddress;
                        ID.InvoicePostCode = model.InvoicePostCode;
                        ID.ContactName = model.ContactName;
                        ID.InvoiceTo = model.InvoiceTo;
                        ID.Mobile = model.Mobile;
                        ID.Fax = model.Fax;
                        ID.Email = model.Email;
                        ID.HireofFacilities = model.HireofFacilities;
                        ID.CostCentreCode = model.CostCentreCode;
                        ID.BudgetCode = model.BudgetCode;
                        ID.VatRate = model.VatRate;
                        ID.ProfitValue = model.ProfitValue;
                        ID.InvoiceNotes = model.InvoiceNotes;
                        ID.InvoiceAmount = model.InvoiceAmount;
                        ID.VatAmount = model.VatAmount;
                        ID.GrossAmount = model.GrossAmount;
                        ID.InvoiceRequestDate = model.InvoiceRequestDate;
                        ID.BookingDetailId = HttpContext.Session.GetInt32("BookingDetailId");

                        var invoiceDetail = await _invoiceDetail.InsertAsync(HttpContext.Session.GetString("ConnectionString"), ID, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (invoiceDetail != null)
                        {
                            // var objResult = await _itemsToInvoice.GetAllItemToInvoiceByBookingId(HttpContext.Session.GetString("ConnectionString"), 1);
                            foreach (var item in model.Invoices)
                            {
                                ItemsToInvoice INV = new ItemsToInvoice();

                                INV.InvoiceDetailsId = invoiceDetail.Id;
                                INV.InvoiceMasterId = item.Id;
                                INV.Quantity = item.Quantity;

                                var itemsToInvoice = await _itemsToInvoice.InsertAsync(HttpContext.Session.GetString("ConnectionString"), INV, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                            }
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.InvoiceDetailCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Invoice Detail");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }


                    else
                    {
                        var objResult = await _invoiceDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.InvoiceAddress = model.InvoiceAddress;
                        objResult.InvoicePostCode = model.InvoicePostCode;
                        objResult.ContactName = model.ContactName;
                        objResult.InvoiceTo = model.InvoiceTo;
                        objResult.Mobile = model.Mobile;
                        objResult.Fax = model.Fax;
                        objResult.Email = model.Email;
                        objResult.HireofFacilities = model.HireofFacilities;
                        objResult.CostCentreCode = model.CostCentreCode;
                        objResult.BudgetCode = model.BudgetCode;
                        objResult.VatRate = model.VatRate;
                        objResult.ProfitValue = model.ProfitValue;
                        objResult.InvoiceNotes = model.InvoiceNotes;
                        objResult.InvoiceAmount = model.InvoiceAmount;
                        objResult.VatAmount = model.VatAmount;
                        objResult.GrossAmount = model.GrossAmount;
                        objResult.InvoiceRequestDate = model.InvoiceRequestDate;

                        var invoiceDetail = await _invoiceDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (invoiceDetail != null)
                        {
                            var itemsToInvoiceResult = await _itemsToInvoice.GetAllItemToInvoiceByInvoiceDetailId(HttpContext.Session.GetString("ConnectionString"), invoiceDetail.Id);
                            foreach (var item in itemsToInvoiceResult)
                            {
                                ItemsToInvoice INV = new ItemsToInvoice();

                                INV.InvoiceDetailsId = invoiceDetail.Id;
                                INV.InvoiceMasterId = item.Id;
                                INV.Quantity = item.Quantity;
                                var getitemToInvoice = await _itemsToInvoice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                                    x => x.InvoiceDetailsId == invoiceDetail.Id && x.InvoiceMasterId == item.Id && x.Quantity == item.Quantity);
                                if (getitemToInvoice != null)
                                {
                                    var itemsToInvoice = await _itemsToInvoice.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), INV, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                                }
                            }
                            if (model.Invoices != null)
                            {
                                var test = model.Invoices.ToList();
                                foreach (var item in test)
                                {
                                    var itemsToInvoices = itemsToInvoiceResult.Where(x => x.InvoiceMasterId != item.Id && x.InvoiceDetailsId != invoiceDetail.Id).ToList();
                                    if (itemsToInvoices.Count() == 0)
                                    {
                                        var obj = new ItemsToInvoice()
                                        {
                                            InvoiceDetailsId = invoiceDetail.Id,
                                            InvoiceMasterId = item.Id,
                                            BookingDetailId = HttpContext.Session.GetInt32("BookingDetailId"),
                                            Quantity = item.Quantity
                                        };
                                        var itemToInv = await _itemsToInvoice.InsertAsync(HttpContext.Session.GetString("ConnectionString"), obj
                                          , Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));

                                        //_itemsToInvoice.DeleteRange(HttpContext.Session.GetString("ConnectionString"),
                                        //    itemsToInvoiceResult.Where(x => x.InvoiceMasterId != item.Id && x.InvoiceDetailsId == invoiceDetail.Id));
                                    }
                                }
                            }
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.InvoiceDetailUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Invoice Detail");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }


                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Invoice Detail");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetailList(JQueryDataTableParamModel param)
        {
            try
            {
                if (HttpContext.Session.GetInt32("BookingDetailId") == null)
                {
                    return Json(new
                    {
                        param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = new List<InvoiceDetailDto>()
                    });
                }
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Add(new SqlParameter("@BookingDetailId", SqlDbType.BigInt) { Value = HttpContext.Session.GetInt32("BookingDetailId") });

                var allList = await _invoiceDetail.GetInvoiceDetailList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetInvoiceDetailList");
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
        public async Task<IActionResult> RemoveInvoiceDetail(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var invoiceDetail = await _invoiceDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    invoiceDetail.IsDelete = true;
                    await _invoiceDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), invoiceDetail, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.InvoiceDetailDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveInvoiceDetail");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<InvoiceDto> GetInvoiceItemDetailById(int Id)
        {
            CalculateInvoiceAmountAndGrossAmount();
            var invoiceDetail = await _invoice.GetInvoiceItemDetailById(HttpContext.Session.GetString("ConnectionString"), Id);
            return invoiceDetail;
        }

        [HttpGet]
        public async Task<GetCatMenuListDto> GetMenuById(string Ids)
        {
            decimal cost = 0;
            var menu = new GetCatMenuListDto();
            if (Ids != null)
            {
                string[] idLst = Ids.Split(",");

                foreach (var id in idLst)
                {
                    menu = await _menu.GetMenuDetailById(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt32(id));
                    cost = cost + menu.Cost;
                }
                menu.Cost = cost;
                return menu;
            }
            menu.Cost = 0;
            return menu;

        }



        [HttpGet]
        public async Task<IActionResult> InvoiceViewAsPDF(long id)
        {
            var tempView = new InvoiceDetailDto();
            var objResult = await _invoiceDetail.GET_InvoiceDetailAndItemById(HttpContext.Session.GetString("ConnectionString"), id);

            return new ViewAsPdf("InvoiceViewAsPDF", objResult) { FileName = "Invoice_" + objResult.Id + ".pdf", Model = objResult };
        }

        //Booking Details
        [HttpGet]
        public IActionResult BookingDetailsAsPDF(long id)
        {
            var tempView = new InvoiceDetailDto();
            var bd = _bookingDetail.GetSingle(HttpContext.Session.GetString("ConnectionString"), X => X.Id == id);
            BookingDetailsDto model = new BookingDetailsDto();

            model.Id = bd.Id;
            model.Cost = bd.Cost;
            model.NumberOfAttending = bd.NumberOfAttending;
            model.TitleOfMeeting = bd.TitleOfMeeting;
            model.BookingContact = bd.BookingContact;
            model.BookingDateForDisplay = bd.BookingDate.ToString("dd/MM/yyyy");
            DateTime startTime = (DateTime.Parse(bd.StartTime));
            model.StartTime = startTime.ToString("HH:mm");

            DateTime finishTime = (DateTime.Parse(bd.FinishTime));
            model.FinishTime = finishTime.ToString("HH:mm");

            TimeSpan duration = DateTime.Parse(bd.FinishTime).Subtract(DateTime.Parse(bd.StartTime));
            model.Duration = DateTime.Today.Add(duration).ToString("hh:mm");


            var rmObj = _roomTypes.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == bd.RoomTypeId);
            model.RoomType = rmObj.Title;
            model.HourlyRate = rmObj.HourlyRate.ToString();

            var brObj = _branchDetails.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == bd.BranchId);
            model.branchName = brObj.BranchName;

            UpdateBookingFormDate(model);

            return new ViewAsPdf("BookingDetails", model) { FileName = "Booking Details_Bno_" + model.Id + ".pdf", Model = model };
        }

        [HttpGet]
        public async Task<IActionResult> EmailBookingDetails(long id)
        {

            var tempView = new InvoiceDetailDto();
            var bd = _bookingDetail.GetSingle(HttpContext.Session.GetString("ConnectionString"), X => X.Id == id);

            if (bd.Email != null)
            {
                BookingDetailsDto model = new BookingDetailsDto();
                model.Id = bd.Id;
                model.BookingContact = bd.BookingContact;
                model.BookingDateForDisplay = bd.BookingDate.ToString("dd/MM/yyyy");
                model.NumberOfAttending = bd.NumberOfAttending;
                DateTime startTime = (DateTime.Parse(bd.StartTime));
                model.StartTime = startTime.ToString("HH:mm");

                DateTime finishTime = (DateTime.Parse(bd.FinishTime));
                model.FinishTime = finishTime.ToString("HH:mm");

                var rmObj = _roomTypes.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == bd.RoomTypeId);
                model.RoomType = rmObj.Title;
                model.HourlyRate = rmObj.HourlyRate.ToString();

                var brObj = _branchDetails.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == bd.BranchId);
                model.branchName = brObj.BranchName;

                UpdateBookingFormDate(model);

                var BookingDateForDisplay = bd.BookingDate.ToShortDateString();
                string emailTemplate = CommonMethod.ReadEmailTemplate(_errorLog, _webHostEnvironment.WebRootPath, "CreateBookingForm.html", BookingDateForDisplay);
                emailTemplate = emailTemplate.Replace("{UserName}", model.BookingContact);
                emailTemplate = emailTemplate.Replace("{RoomType}", model.RoomType);
                emailTemplate = emailTemplate.Replace("{BranchName}", model.branchName);
                emailTemplate = emailTemplate.Replace("{BookingDate}", model.BookingDateForDisplay);
                emailTemplate = emailTemplate.Replace("{StartTime}", model.StartTime);
                emailTemplate = emailTemplate.Replace("{FinishTime}", model.FinishTime);
                emailTemplate = emailTemplate.Replace("{NumberOfAttending}", (model.NumberOfAttending).ToString());
                bool data = await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                {
                    ToDisplayName = bd.BookingContact,
                    ToAddress = bd.Email,
                    Subject = "Booking Confirm Form",
                    BodyText = emailTemplate
                });

                return JsonResponse.GenerateJsonResult(1, ResponseConstants.CheckMailForBookingConfirm);
            }
            else
            {
                return JsonResponse.GenerateJsonResult(0, ResponseConstants.PleaseAddEmail);
            }
            //return new ViewAsPdf("BookingDetails", model) { FileName = "Booking details_Bno_" + model.Id + ".pdf", Model = model };
        }

        //Booking Invoice
        [HttpGet]
        public IActionResult BookingDetailsViewAsPDF(long id)
        {
            var tempView = new InvoiceDetailDto();
            var bd = _bookingDetail.GetSingle(HttpContext.Session.GetString("ConnectionString"), X => X.Id == id);
            BookingDetailsDto model = new BookingDetailsDto();
            model.Id = bd.Id;
            model.Cost = bd.Cost;
            model.NumberOfAttending = bd.NumberOfAttending;
            model.TitleOfMeeting = bd.TitleOfMeeting;
            model.BookingContact = bd.BookingContact;
            model.BookingDateForDisplay = bd.BookingDate.ToString("dd/MM/yyyy");
            model.StartTime = bd.StartTime;
            model.FinishTime = bd.FinishTime;

            var rmObj = _roomTypes.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == bd.RoomTypeId);
            model.RoomType = rmObj.Title;
            model.HourlyRate = rmObj.HourlyRate.ToString();
            model.SaturdayRate = rmObj.SaturdayRate.ToString();
            model.SundayRate = rmObj.SundayRate.ToString();


            return new ViewAsPdf("BookingDetailsViewAsPDF", model) { FileName = "BookingInvoice details_Bno_" + model.Id + ".pdf", Model = model };
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBookingFormDate(BookingDetailsDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var objResult = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                    objResult.DateFromSent = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    var bookingDetail = await _bookingDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (bookingDetail != null)
                    {
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
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Booking Details");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> CateringRequirementViewAsPDF(long id)
        {
            var tempView = new CatererRequirementInvoiceDto();
            var bd = await _CRservice.CateringRequirementViewAsPDF(HttpContext.Session.GetString("ConnectionString"), id);
            return new ViewAsPdf("CateringRequirementViewAsPDF", bd) { FileName = "Catering Requirement_" + bd.FirstOrDefault().BookingDetailId + ".pdf", Model = bd };
        }



        #endregion

        #region Methods for Security

        [HttpGet]
        public async Task<IActionResult> _AddEditSecurity(long id, bool isView)
        {
            if (id == 0) return View(@"Components/_AddEditSecurity", new SecurityDto { Id = id, IsView = isView });
            var tempView = new SecurityDto();
            var objResult = await _security.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView = Mapper.Map<SecurityDto>(objResult);
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditSecurity", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditSecurity(SecurityDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        Security CR = new Security();
                        CR.DateCollected = model.DateCollected;
                        CR.DateReturned = model.DateReturned;
                        CR.TimeCollected = model.TimeCollected;
                        CR.TimeReturned = model.TimeReturned;
                        CR.CollectedBy = model.CollectedBy;
                        CR.ReturnedBy = model.ReturnedBy;
                        CR.SecurityNotes = model.SecurityNotes;
                        CR.BookingDetailId = HttpContext.Session.GetInt32("BookingDetailId");

                        var security = await _security.InsertAsync(HttpContext.Session.GetString("ConnectionString"), CR, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (security != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.SecurityCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Security");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {

                        var objResult = await _security.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.DateCollected = model.DateCollected;
                        objResult.DateReturned = model.DateReturned;
                        objResult.TimeCollected = model.TimeCollected;
                        objResult.TimeReturned = model.TimeReturned;
                        objResult.CollectedBy = model.CollectedBy;
                        objResult.ReturnedBy = model.ReturnedBy;
                        objResult.SecurityNotes = model.SecurityNotes;

                        var security = await _security.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (security != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.SecurityUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Security");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Security");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetSecurityList(JQueryDataTableParamModel param)
        {
            try
            {
                if (HttpContext.Session.GetInt32("BookingDetailId") == null)
                {
                    return Json(new
                    {
                        param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = new List<SecurityDto>()
                    });
                }
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Add(new SqlParameter("@BookingDetailId", SqlDbType.BigInt) { Value = HttpContext.Session.GetInt32("BookingDetailId") });

                var allList = await _security.GetSecurityList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());

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
                ErrorLog.AddErrorLog(ex, "GetSecurityList");
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
        public async Task<IActionResult> RemoveSecurity(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var questionObj = await _security.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _security.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.SecurityDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveSecurity");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion

        #region Methods for Notes

        [HttpGet]
        public async Task<IActionResult> _AddEditNotes(long id, bool isView)
        {
            if (id == 0) return View(@"Components/_AddEditNotes", new NotesDto { Id = id, IsView = isView });
            var tempView = new NotesDto();
            var objResult = await _notes.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.Note = objResult.Note;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditNotes", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditNotes(NotesDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        Notes notes = new Notes();
                        notes.Note = model.Note;
                        notes.BookingDetailId = HttpContext.Session.GetInt32("BookingDetailId");

                        var notesObj = await _notes.InsertAsync(HttpContext.Session.GetString("ConnectionString"), notes, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (notesObj != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.NotesCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Notes");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {

                        var objResult = await _notes.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.Note = model.Note;

                        var notes = await _notes.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (notes != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.NotesUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Notes");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Notes");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetNotesList(JQueryDataTableParamModel param)
        {
            try
            {
                if (HttpContext.Session.GetInt32("BookingDetailId") == null)
                {
                    return Json(new
                    {
                        param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = new List<NotesDto>()
                    });
                }

                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;

                parameters.Add(new SqlParameter("@BookingDetailId", SqlDbType.BigInt) { Value = HttpContext.Session.GetInt32("BookingDetailId") });

                var allList = await _notes.GetNotesList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());

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
                ErrorLog.AddErrorLog(ex, "GetNotesList");
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
        public async Task<IActionResult> RemoveNotes(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var notes = await _notes.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    notes.IsDelete = true;
                    await _notes.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), notes, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.NotesDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveNotes");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion

        #region Methods for Contact Details

        [HttpGet]
        public async Task<IActionResult> _AddEditContactDetails(long id, bool isView)
        {
            if (id == 0) return View(@"Components/_AddEditContactDetails", new ContactDetailsDto { Id = id, IsView = isView });
            var tempView = new ContactDetailsDto();
            var objResult = await _contactDetails.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView = Mapper.Map<ContactDetailsDto>(objResult);
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditContactDetails", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditContactDetails(ContactDetailsDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        ContactDetails CD = new ContactDetails();
                        CD.Name = model.Name;
                        CD.Department = model.Department;
                        CD.Mobile = model.Mobile;
                        CD.Email = model.Email;
                        CD.BookingDetailId = HttpContext.Session.GetInt32("BookingDetailId");

                        var contactDetails = await _contactDetails.InsertAsync(HttpContext.Session.GetString("ConnectionString"), CD, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (contactDetails != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.ContactDetailsCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Contact Details");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {

                        var objResult = await _contactDetails.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.Name = model.Name;
                        objResult.Department = model.Department;
                        objResult.Mobile = model.Mobile;
                        objResult.Email = model.Email;

                        var security = await _contactDetails.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (security != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.ContactDetailsUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Contact Details");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Contact Details");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetContactDetailsList(JQueryDataTableParamModel param)
        {
            try
            {
                if (HttpContext.Session.GetInt32("BookingDetailId") == null)
                {
                    return Json(new
                    {
                        param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = new List<ContactDetailsDto>()
                    });
                }

                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Add(new SqlParameter("@BookingDetailId", SqlDbType.BigInt) { Value = HttpContext.Session.GetInt32("BookingDetailId") });
                var allList = await _contactDetails.GetContactDetailsList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());

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
                ErrorLog.AddErrorLog(ex, "GetContactDetailsList");
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
        public async Task<IActionResult> RemoveContactDetails(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var contactDetails = await _contactDetails.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    contactDetails.IsDelete = true;
                    await _contactDetails.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), contactDetails, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.SecurityDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveContactDetails");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion


        #region Methods for VisitorBooking

        [HttpGet]
        public IActionResult _AddEditVisitorBooking(long id, bool isView)
        {
            return View(@"Components/_AddEditVisitorBooking", new VisitorDto { Id = id, IsView = isView });
        }

        [HttpPost]
        public async Task<IActionResult> AddEditVisitorAndVisitorBooking(VisitorDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        Visitors MT = new Visitors();
                        MT.Description = model.Description;
                        MT.Name = model.Name;
                        MT.SurName = model.SurName;
                        MT.Address = model.Address;
                        MT.PostCode = model.PostCode;
                        MT.Email = model.Email;
                        MT.Telephone = model.Telephone;
                        MT.Mobile = model.Mobile;
                        MT.Notes = model.Notes;
                        MT.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());

                        var visitors = await _visitors.InsertAsync(HttpContext.Session.GetString("ConnectionString"), MT, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (visitors != null)
                        {
                            VisitorBooking VB = new VisitorBooking();
                            VB.BookingDetailId = (long)HttpContext.Session.GetInt32("BookingDetailId");
                            VB.VisitorId = visitors.Id;

                            var visitorBooking = await _visitorBooking.InsertAsync(HttpContext.Session.GetString("ConnectionString"), VB, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                            if (visitorBooking != null)
                            {
                                txscope.Complete();
                                return JsonResponse.GenerateJsonResult(1, ResponseConstants.VisitorBookingCreated);
                            }
                            else
                            {
                                txscope.Dispose();
                                ErrorLog.AddErrorLog(null, "Error in create Visitor Booking");
                                return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                            }
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Visitor Booking");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else if (model.Id != 0)
                    {
                        VisitorBooking VB = new VisitorBooking();
                        VB.BookingDetailId = (long)HttpContext.Session.GetInt32("BookingDetailId");
                        VB.VisitorId = model.Id;

                        var visitorBooking = await _visitorBooking.InsertAsync(HttpContext.Session.GetString("ConnectionString"), VB, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (visitorBooking != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.VisitorBookingCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Visitor Booking");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {
                        txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "Error in create Visitor Booking");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Visitor Booking");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddEditVisitorBooking(long visitorId)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (visitorId > 0)
                    {
                        VisitorBooking VB = new VisitorBooking();
                        VB.BookingDetailId = (long)HttpContext.Session.GetInt32("BookingDetailId");
                        VB.VisitorId = visitorId;

                        var visitorBooking = await _visitorBooking.InsertAsync(HttpContext.Session.GetString("ConnectionString"), VB, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (visitorBooking != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.VisitorBookingCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Visitor Booking");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {
                        txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "Error in create Visitor Booking");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Visitor Booking");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVisitorBookingList(JQueryDataTableParamModel param, string search)
        {
            try
            {
                if (HttpContext.Session.GetInt32("BookingDetailId") == null)
                {
                    return Json(new
                    {
                        param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = new List<VisitorBookingDto>()
                    });
                }

                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                if (search == null)
                    parameters.Add(new SqlParameter("@BookingDetailId", SqlDbType.BigInt) { Value = (long)HttpContext.Session.GetInt32("BookingDetailId") });
                else
                    parameters.Add(new SqlParameter("@BookingDetailId", SqlDbType.BigInt) { Value = 0 });
                parameters.Add(new SqlParameter("@CustomSearch", SqlDbType.VarChar) { Value = search });


                var allList = await _visitorBooking.GetVisitor_BookingList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetVisitorBookingList");
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
        public async Task<IActionResult> RemoveVisitorBooking(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var questionObj = await _visitorBooking.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _visitorBooking.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.VisitorBookingDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveVisitorBooking");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion

        #region Methods for CallLog

        [HttpGet]
        public async Task<IActionResult> _AddEditCallLog(long id, bool isView)
        {
            var entryList = _entryType.GET_EntryTypeList(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            var tempView = new CallLogsDto();
            if (entryList.Count() != 0)
            {
                ViewBag.IsEntryTypeEmpty = false;
                ViewBag.EntryTypeList = entryList.Select(x => new SelectListItem()
                {
                    Text = x.Title,
                    Value = x.Id.ToString()
                }).OrderBy(x => x.Text);
            }
            else
            {
                ViewBag.IsEntryTypeEmpty = true;
                tempView.IsView = true;
            }

            var userlist = _profile.GetUserList(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            if (userlist.Count() != 0)
            {
                ViewBag.IsUserEmpty = false;
                ViewBag.UserList = userlist.Select(x => new SelectListItem()
                {
                    Text = x.FirstName + " " + x.LastName,
                    Value = x.ID.ToString()
                }).OrderBy(x => x.Text);
            }
            else
            {
                ViewBag.IsUserEmpty = true;
                tempView.IsView = true;
            }

            if (id == 0) return View(@"Components/_AddEditCallLog", new CallLogsDto { Id = id, IsView = isView });
            var objResult = await _callLogs.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.Subject = objResult.Subject;
            tempView.EntryType = objResult.EntryType;
            tempView.DateOfentry = Convert.ToDateTime(objResult.DateOfentry);
            tempView.EntryDate = objResult.DateOfentry.ToShortDateString();
            tempView.NextconDate = objResult.NextContactDate.ToShortDateString();
            tempView.Time = objResult.Time;
            tempView.Contact = objResult.Contact;
            tempView.Address = objResult.Address;
            tempView.Comments = objResult.Comments;
            tempView.PostCode = objResult.PostCode;
            tempView.TakenBy = objResult.TakenBy;
            tempView.TakenFor = objResult.TakenFor;
            tempView.NextContactDate = objResult.NextContactDate.Date;
            tempView.ISCompleted = objResult.ISCompleted;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            tempView.BookingDetailId = objResult.BookingDetailId.Value;
            return View(@"Components/_AddEditCallLog", tempView);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditCallLog(CallLogsDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        CallLogs CL = new CallLogs();
                        CL.Subject = model.Subject;
                        CL.EntryType = model.EntryType;
                        CL.DateOfentry = model.DateOfentry;
                        CL.Time = model.Time;
                        CL.Contact = model.Contact;
                        CL.Address = model.Address;
                        CL.Comments = model.Comments;
                        CL.PostCode = model.PostCode;
                        CL.TakenBy = model.TakenBy;
                        CL.TakenFor = model.TakenFor;
                        CL.NextContactDate = model.NextContactDate;
                        CL.ISCompleted = model.ISCompleted;
                        CL.Associated = (int)GlobalEnums.AssociatedCallLog.BookingDetail;
                        CL.BookingDetailId = HttpContext.Session.GetInt32("BookingDetailId");

                        var callLog = await _callLogs.InsertAsync(HttpContext.Session.GetString("ConnectionString"), CL, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (callLog != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.CallLogCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create CallLog");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {

                        var objResult = await _callLogs.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.Subject = model.Subject;
                        objResult.EntryType = model.EntryType;
                        objResult.DateOfentry = model.DateOfentry;
                        objResult.Time = model.Time;
                        objResult.Contact = model.Contact;
                        objResult.Address = model.Address;
                        objResult.Comments = model.Comments;
                        objResult.PostCode = model.PostCode;
                        objResult.TakenBy = model.TakenBy;
                        //objResult.TakenFor = model.TakenFor;
                        objResult.NextContactDate = model.NextContactDate;
                        objResult.ISCompleted = model.ISCompleted;
                        objResult.Associated = (int)GlobalEnums.AssociatedCallLog.BookingDetail;

                        var callLog = await _callLogs.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (callLog != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.CallLogUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update CallLog");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update CallLog");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCallLogList(JQueryDataTableParamModel param)
        {
            try
            {
                if (HttpContext.Session.GetInt32("BookingDetailId") == null)
                {
                    return Json(new
                    {
                        param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = new List<CallLogsDto>()
                    });
                }

                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Add(new SqlParameter("@BookingDetailId", SqlDbType.BigInt) { Value = HttpContext.Session.GetInt32("BookingDetailId") });

                var allList = await _callLogs.GetCallLogsListForBookingDetail(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetCallLogsList");
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
        public async Task<IActionResult> RemoveCallLog(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var questionObj = await _callLogs.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _callLogs.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.CallLogsDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveCallLog");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ManageCallLogIsCompleted(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var objResult = await _callLogs.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    objResult.ISCompleted = !objResult.ISCompleted;

                    var calllog = await _callLogs.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (calllog != null)
                    {
                        txscope.Complete();
                        return JsonResponse.GenerateJsonResult(1, $@"Call log {(objResult.ISCompleted ? "activated" : "deactivated")} successfully.");
                    }
                    else
                    {
                        txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "Error in Update Call Log");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Update Call Log");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion

        #region Equipment Requirement

        [HttpGet]
        public async Task<IActionResult> _AddEditEquipmentRequirements(long id, bool isView)
        {
            var equLST = await _equip.GET_EuipmentList(HttpContext.Session.GetString("ConnectionString"));
            var tempView = new EquipmentRequiredForBookingDto();
            tempView.Id = id;
            tempView.IsView = isView;

            var requiredForBookings = await _equipmentRequiredForBooking.GET_EuipmentReqForBookingList(HttpContext.Session.GetString("ConnectionString"), (long)HttpContext.Session.GetInt32("BookingDetailId"));
            var EquipObj = await _equipmentRequiredForBooking.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                x => x.Id == id && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.IsDelete == false);

            var NewRoolList = new List<EquipmentRequiredForBookingDto>();
            if (equLST.Count() != 0)
            {
                ViewBag.IsEquipEmpty = false;
                if (requiredForBookings.Count() != 0)
                {
                    var temp = requiredForBookings.Where(x => !x.IsDelete).Select(y => y.EquipmentRequiredId);
                    if (equLST.Where(x => !temp.Contains(x.EquipId)).Count() != 0)
                        NewRoolList.AddRange(equLST.Where(x => !temp.Contains(x.EquipId)));
                    if (EquipObj != null)
                        NewRoolList.Add(equLST.FirstOrDefault(x => x.EquipId == EquipObj.EquipmentRequiredId));

                    ViewBag.IsEquipEmpty = NewRoolList.Count() == 0 ? true : false;
                    ViewBag.EquipList = NewRoolList.Select(x => new SelectListItem()
                    {
                        Text = x.EqupTitle,
                        Value = x.EquipmentRequiredId.ToString()
                    }).OrderBy(x => x.Text);


                }
                else
                {
                    ViewBag.EquipList = equLST.Select(x => new SelectListItem()
                    {
                        Text = x.EqupTitle,
                        Value = x.EquipId.ToString()
                    }).OrderBy(x => x.Text);

                }

            }
            else
            {
                ViewBag.IsEquipEmpty = true;
                tempView.IsView = true;
            }

            if (id == 0) return View(@"Components/_AddEditEquipmentRequirements", tempView);
            var objResult = await _equipmentRequiredForBooking.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.EquipmentRequiredId = objResult.EquipmentRequiredId;
            tempView.BookingDetailId = objResult.BookingDetailId.Value;
            tempView.NoofItem = objResult.NoofItem;
            return View(@"Components/_AddEditEquipmentRequirements", tempView);

        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditEquipmentRequirements(EquipmentRequiredForBookingDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        EquipmentRequiredForBooking ERB = new EquipmentRequiredForBooking();
                        ERB.BookingDetailId = HttpContext.Session.GetInt32("BookingDetailId");
                        ERB.EquipmentRequiredId = model.EquipmentRequiredId;
                        ERB.NoofItem = model.NoofItem;

                        var equipment = await _equipmentRequiredForBooking.InsertAsync(HttpContext.Session.GetString("ConnectionString"), ERB, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (equipment != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.EquipmentRequirementCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Equipment Requirement");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {

                        var objResult = await _equipmentRequiredForBooking.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.BookingDetailId = HttpContext.Session.GetInt32("BookingDetailId");
                        objResult.EquipmentRequiredId = model.EquipmentRequiredId;
                        objResult.NoofItem = model.NoofItem;

                        var security = await _equipmentRequiredForBooking.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (security != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.EquipmentRequirementUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Equipment Requirement");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Equipment Requirement");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEquipmentRequirementList(JQueryDataTableParamModel param)
        {
            try
            {
                if (HttpContext.Session.GetInt32("BookingDetailId") == null)
                {
                    return Json(new
                    {
                        param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = new List<EquipmentRequiredForBookingDto>()
                    });
                }

                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Add(new SqlParameter("@BookingDetailId", SqlDbType.BigInt) { Value = HttpContext.Session.GetInt32("BookingDetailId") });

                var allList = await _equipmentRequiredForBooking.GetEquipmentRequiredForBookingList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());

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
                ErrorLog.AddErrorLog(ex, "GetEquipmentRequirementList");
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
        public async Task<IActionResult> RemoveEquipmentRequirement(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var equipment = await _equipmentRequiredForBooking.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    equipment.IsDelete = true;
                    await _equipmentRequiredForBooking.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), equipment, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.EquipmentRequirementDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveEquipmentRequirement");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckEquipmentAvailable(int EquipmentID)
        {
            var Parameters = new List<SqlParameter>
                {

                    new SqlParameter("@BookingDetailId",SqlDbType.Int){Value =  HttpContext.Session.GetInt32("BookingDetailId") },
                     new SqlParameter("@EquipmentRequiredId",SqlDbType.Int){Value =EquipmentID},

                };

            var costResult = await _equipmentRequiredForBooking.CheckISEquipmentAvalable(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());

            return JsonResponse.GenerateJsonResult(1, "Sucess", costResult.ToString());
        }

        #endregion

        #region  Common


        public async Task<bool> CheckInvoiceTitle(int InvoiceMasterId, long Id, long InvoiceDetailsId)
        {
            bool isExist;
            var result = await _itemsToInvoice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                x => x.InvoiceMasterId == InvoiceMasterId && x.IsDelete == false && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.InvoiceDetailsId == InvoiceDetailsId);
            if (result != null)
            {

                isExist = result.InvoiceMasterId == InvoiceMasterId && result.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = await _itemsToInvoice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                        x => x.InvoiceMasterId == InvoiceMasterId && x.Id == Id && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.IsDelete == false && x.InvoiceDetailsId == InvoiceDetailsId);
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

        public async Task<bool> CheckInvoiceDetailEmail(string Email, long Id)
        {
            bool isExist;
            var result = await _invoiceDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                x => x.Email.ToLower().Equals(Email.ToLower()) && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.IsDelete == false);

            if (result != null)
            {

                isExist = result.Email.ToLower().Trim().Equals(Email.ToLower().Trim()) && result.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = await _invoiceDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                        x => x.Email.ToLower().Equals(Email.ToLower()) && x.Id == Id && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.IsDelete == false);

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

        public async Task<bool> CheckCatererName(int CateringDetailId, long Id)
        {
            bool isExist;
            var result = await _CRservice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                x => x.CatererId == CateringDetailId && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.IsDelete == false);
            if (result != null)
            {

                isExist = result.CatererId == CateringDetailId && result.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = await _CRservice.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                        x => x.CatererId == CateringDetailId && x.Id == Id && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.IsDelete == false);
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

        public async Task<bool> validateMaxPerson(int TotalPerson, long RoomTypeId)
        {

            var result = await _roomTypes.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                x => x.Id == RoomTypeId && x.IsDelete == false);
            if (result != null)
            {
                return result.Maxperson < TotalPerson ? false : true;
            }
            else
            {
                return result == null ? true : false;
            }
        }

        [HttpGet]
        public async Task<List<VisitorDto>> loadUserMobile(string Mobile)
        {
            var result = await _visitors.loadUserMobile(HttpContext.Session.GetString("ConnectionString"), Mobile);
            //  return result;

            return result;
        }

        public async Task<bool> CheckBookingDetailUserGroup(int UserGroupId, long Id, string bookingDate, string timeStart, string finishTime)
        {
            var Parameters = new List<SqlParameter>
            {
                new SqlParameter("@MeetingTypeId",SqlDbType.BigInt){Value = 0},
                new SqlParameter("@UserGroupId",SqlDbType.BigInt){Value = UserGroupId},
                new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = 0},
                new SqlParameter("@FinishTime",SqlDbType.VarChar){Value = finishTime},
                new SqlParameter("@StartTime",SqlDbType.VarChar){Value = timeStart},
                new SqlParameter("@BookingDate",SqlDbType.VarChar){Value = bookingDate},
                new SqlParameter("@BookingDetailId",SqlDbType.BigInt){Value = Id}
            };

            var result = await _bookingDetail.CheckIsBookingDetailAvailable(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
            if (result.Count() > 0) return false;
            return true;
        }

        public async Task<bool> CheckBookingDetailRoomType(int RoomTypeId, long Id, string bookingDate, string timeStart, string finishTime)
        {
            var Parameters = new List<SqlParameter>
            {
                new SqlParameter("@BookingDetailId",SqlDbType.BigInt){Value = Id},
                new SqlParameter("@BookingDate",SqlDbType.VarChar){Value = bookingDate},
                new SqlParameter("@StartTime",SqlDbType.VarChar){Value = timeStart},
                new SqlParameter("@FinishTime",SqlDbType.VarChar){Value = finishTime},
                new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = RoomTypeId},
                new SqlParameter("@UserGroupId",SqlDbType.BigInt){Value = 0},
                new SqlParameter("@MeetingTypeId",SqlDbType.BigInt){Value = 0}
            };

            var result = await _bookingDetail.CheckIsBookingDetailAvailable(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
            if (result.Count() > 0) return false;
            return true;
        }

        public async Task<bool> CheckBookingDetailMeetingType(int MeetingTypeId, long Id, string bookingDate, string timeStart, string finishTime)
        {
            var Parameters = new List<SqlParameter>
            {
                new SqlParameter("@MeetingTypeId",SqlDbType.BigInt){Value = MeetingTypeId},
                new SqlParameter("@UserGroupId",SqlDbType.BigInt){Value = 0},
                new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = 0},
                new SqlParameter("@FinishTime",SqlDbType.VarChar){Value = finishTime},
                new SqlParameter("@StartTime",SqlDbType.VarChar){Value = timeStart},
                new SqlParameter("@BookingDate",SqlDbType.VarChar){Value = bookingDate},
                new SqlParameter("@BookingDetailId",SqlDbType.BigInt){Value = Id}
            };

            var result = await _bookingDetail.CheckIsBookingDetailAvailable(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
            if (result.Count() > 0) return false;
            return true;
        }

        public async Task<bool> CheckBookingDetailMeetingTitle(string TitleOfMeeting, long Id)
        {
            bool isExist;
            var result = await _bookingDetail.CheckBookingDetailMeetingTitle(HttpContext.Session.GetString("ConnectionString"), TitleOfMeeting, Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));

            if (result != null)
            {
                isExist = result.TitleOfMeeting == TitleOfMeeting ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                        x => x.TitleOfMeeting == TitleOfMeeting && x.Id == Id && x.IsDelete == false);
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

        public async Task<bool> CheckContactDetailEmail(string Email, long Id)
        {
            bool isExist;
            var result = await _contactDetails.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                x => x.Email.ToLower().Equals(Email.ToLower()) && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.IsDelete == false);

            if (result != null)
            {

                isExist = result.Email.ToLower().Trim().Equals(Email.ToLower().Trim()) && result.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = await _contactDetails.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                        x => x.Email.ToLower().Equals(Email.ToLower()) && x.Id == Id && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.IsDelete == false);

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

        public async Task<bool> CheckEquipmentRequirement(int EquipmentRequiredId, long Id)
        {
            bool isExist;
            var result = await _equipmentRequiredForBooking.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                x => x.EquipmentRequiredId == EquipmentRequiredId && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.IsDelete == false);
            if (result != null)
            {

                isExist = result.EquipmentRequiredId == EquipmentRequiredId && result.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = await _equipmentRequiredForBooking.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"),
                        x => x.EquipmentRequiredId == EquipmentRequiredId && x.BookingDetailId == HttpContext.Session.GetInt32("BookingDetailId") && x.Id == Id && x.IsDelete == false);
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

        #endregion


        [HttpPost]
        public async Task<IActionResult> UploadExcelFile(IFormFile postedFile, long Id)
        {
            List<VisitorDtoExcel> VisitorDtoExcel = new List<VisitorDtoExcel>();
            ExcelUpload up = new ExcelUpload();



            if (postedFile != null)
            {
                try
                {
                    string path = Path.Combine(this.Environment.WebRootPath, "Uploads");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    using (var stream = new FileStream(path + "\\AddVisitor.xlsx", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        await postedFile.CopyToAsync(stream);
                    }

                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var stream = System.IO.File.Open(path + "\\AddVisitor.xlsx", FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            int i = 0;
                            while (reader.Read()) //Each row of the file
                            {
                                if (i > 0)
                                {
                                    VisitorDtoExcel.Add(new VisitorDtoExcel
                                    {
                                        Name = reader.GetValue(0).ToString(),
                                        SurName = reader.GetValue(1).ToString(),
                                        PostCode = reader.GetValue(2).ToString(),
                                        Email = reader.GetValue(3).ToString(),
                                        Telephone = reader.GetValue(4).ToString(),
                                        Mobile = reader.GetValue(5).ToString(),
                                        Notes = reader.GetValue(6).ToString(),
                                        Address = reader.GetValue(5).ToString(),
                                    });
                                }
                                i = i + 1;
                            }
                        }
                    }

                    foreach (VisitorDtoExcel model in VisitorDtoExcel)
                    {

                        Visitors MT = new Visitors();
                        MT.Description = model.Description;
                        MT.Name = model.Name;
                        MT.SurName = model.SurName;
                        MT.Address = model.Address;
                        MT.PostCode = model.PostCode;
                        MT.Email = model.Email;
                        MT.Telephone = model.Telephone;
                        MT.Mobile = model.Mobile;
                        MT.Notes = model.Notes;
                        MT.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
                        var visitors = await _visitors.InsertAsync(HttpContext.Session.GetString("ConnectionString"), MT, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (visitors != null)
                        {
                            VisitorBooking VB = new VisitorBooking();
                            VB.BookingDetailId = Id;
                            VB.VisitorId = visitors.Id;

                            var visitorBooking = await _visitorBooking.InsertAsync(HttpContext.Session.GetString("ConnectionString"), VB, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        }

                    }

                    return JsonResponse.GenerateJsonResult(1, "File Uploaded");


                }
                catch (Exception ex)
                {
                    return JsonResponse.GenerateJsonResult(0, "Something went wrong");
                }
            }
            else
            {
                return JsonResponse.GenerateJsonResult(0, "No files Selected");
            }


        }


        [HttpGet]
        public JsonResult GetRoomTypeList()
        {
            var roomTypeList = _roomTypes.GetRoomTypeListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            return Json(roomTypeList.Select(x => new { Title = x.Title, Id = x.Id }).ToArray());
        }

        [HttpGet]
        public JsonResult GetBookingStatusList()
        {
            var bookingStatusList = _bookingStatus.GetBookingStatusListForDropDown(HttpContext.Session.GetString("ConnectionString"));
            return Json(bookingStatusList.Select(x => new { Title = x.Status, Id = x.Id }).ToArray());
        }

        [HttpGet]
        public JsonResult GetUserList()
        {
            var userList = _roomTypes.GetUserListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            return Json(userList.Select(x => new { Title = x.FullName, Id = x.Id.ToString() }).ToArray());
        }

    }
}