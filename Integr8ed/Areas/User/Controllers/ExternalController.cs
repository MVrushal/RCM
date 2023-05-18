using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Integr8ed.Utility.Common;
using Integr8ed.Service.Interface.ClientAdmin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Integr8ed.Service.Dto;
using Microsoft.Data.SqlClient;
using System.Data;
using Integr8ed.Service.Enums;
using System.Globalization;
using Integr8ed.Data.DbModel.ClientAdmin;
using System.Transactions;
using static Integr8ed.Service.Enums.GlobalEnums;
using Newtonsoft.Json;

namespace Integr8ed.Areas.User.Controllers
{
    [Area("User")]
    public class ExternalController : BaseController<ExternalController>
    {
        private readonly IConfiguration _config;
        private readonly IBranchService _branchService;
        private readonly IRoomTypesService _roomTypeService;
        private readonly IUserGroupServices _userGroupService;
        private readonly IMeetingTypeServices _meetingType;
        private readonly IBookingDetailServices _bookingRequest;
        private readonly IBookingStatusService _bookingStatus;
        public ExternalController(IConfiguration config, IBranchService branchService,
            IRoomTypesService roomTypesService,
            IUserGroupServices userGroupServices,
            IMeetingTypeServices meetingType,
            IBookingDetailServices ibookingRequestService,
            IBookingStatusService bookingStatus
            )
        {
            _config = config;
            _roomTypeService = roomTypesService;
            _branchService = branchService;
            _userGroupService = userGroupServices;
            _userGroupService = userGroupServices;
            _meetingType = meetingType;
            _bookingRequest = ibookingRequestService;
            _bookingStatus = bookingStatus;
        }

        public IActionResult Index()
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);


            var branchList = _branchService.getBranchList(HttpContext.Session.GetString("ConnectionString"),"");
            ViewBag.BranchList = branchList.Select(x => new SelectListItem()
            {
                Text = x.BranchName,
                Value = x.Id.ToString()
            }).OrderBy(x => x.Text);
            return View(new BranchDto());
        }

        [HttpPost]
        public IActionResult GetRoomList(long Id)
        {
            if(Id==0) return JsonResponse.GenerateJsonResult(0, "Invalid Branch Id", null);
            var roomResult= _branchService.GetRoomList(HttpContext.Session.GetString("ConnectionString"),Id);
            return JsonResponse.GenerateJsonResult(1,"",roomResult);
        }

        [HttpPost]
        public async Task<IActionResult> CheckBoking(string date,long roomId)
        {
            var Parameters = new List<SqlParameter>
            {
                new SqlParameter("@Input_Date",SqlDbType.VarChar){Value = date},
                new SqlParameter("@RoomId",SqlDbType.BigInt){Value = roomId},
            };
            var checklist =await  _roomTypeService.CheckBooking(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
            return JsonResponse.GenerateJsonResult(1, "", checklist);
        }

        [HttpGet]
        public async Task<IActionResult> _AddEditBookingDetails(string BookingDate,long rmId,long b)
        {
            try
            {
                var roomTypeList = _roomTypeService.GetRoomTypeListForDropDown(HttpContext.Session.GetString("ConnectionString"), b);
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

                var userGroupList = _userGroupService.GetUserGroupListForDropDown(HttpContext.Session.GetString("ConnectionString"),b);
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

                var meetingTypeList = await _meetingType.GetMeetingListForDropDown(HttpContext.Session.GetString("ConnectionString"), b);
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
                     //= JsonConvert.SerializeObject(data);
                }
                else
                {
                    ViewBag.IsBookingStatusEmpty = true;
                    tempView.IsView = true;
                }


                tempView.RoomTypeId = rmId;
                tempView.BookingDateForDisplay = BookingDate;
                tempView.BranchId = b;
                tempView.BookingStatus = 3;


                return View(@"Components/_AddEditBookingDetails", tempView);
            }
            catch (Exception ex)
            {
                return View(@"Components/_AddEditBookingDetails", new BookingDetailsDto { Id = 0}); ;
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddEditBookingDetails(BookingDetailsDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
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
                        BD.ExternalBookingClientId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                        BD.CatererRemark = model.CatererRemark;
                        BD.IsActive = true;
                        BD.BranchId = model.BranchId;
                        BD.Email = model.Email;
                       // BD.RequestStatus =(int)UserMenu.RequestStatus.Pending;

                        var bookingDetail = await _bookingRequest.InsertAsync(HttpContext.Session.GetString("ConnectionString"), BD, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (bookingDetail != null)
                        {
     
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.BookingRequest);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Booking Details");
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

    }
}