using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Models;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface;
using Integr8ed.Service.Interface.ClientAdmin;
using Integr8ed.Utility.Common;
using Integr8ed.Utility.JqueryDataTable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace Integr8ed.Areas.User.Controllers
{
    [Area("User")]
    public class InternalController : BaseController<InternalController>
    {
        #region Fields
        private readonly ICatering_RequirementsServices _catering_Requirements;
        private readonly ICateringDetailsServices _cateringDetails;
        private readonly IMenuServices _menu;
        private readonly IBookingDetailServices _bookingDetail;
        private readonly IMeetingTypeServices _meetingType;
        private readonly IUserGroupServices _userGroup;
        private readonly IRoomTypesService _roomTypes;
        private readonly IBookingStatusService _bookingStatus;
        #endregion

        #region ctor
        public InternalController(ICatering_RequirementsServices catering_Requirements, ICateringDetailsServices cateringDetails,
            IMenuServices menu, IBookingDetailServices bookingDetail, IMeetingTypeServices meetingType, IUserGroupServices userGroup,
            IRoomTypesService roomTypes, IBookingStatusService bookingStatus)
        {
            _catering_Requirements = catering_Requirements;
            _cateringDetails = cateringDetails;
            _menu = menu;
            _bookingDetail = bookingDetail;
            _meetingType = meetingType;
            _userGroup = userGroup;
            _roomTypes = roomTypes;
            _bookingStatus = bookingStatus;
        }
        #endregion

        #region Methods for Booking Details
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> _AddEditBookingDetails(long id, bool isView)
        {
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

            if (id == 0) return View(@"Components/_AddEditBookingDetail", new BookingDetailsDto { Id = id, IsView = isView });
            var objResult = _bookingDetail.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.UserGroupId = objResult.UserGroupId;
            tempView.MeetingTypeId = objResult.MeetingTypeId.Value;
            tempView.RoomTypeId = objResult.RoomTypeId;
            tempView.StartTime = objResult.StartTime;
            tempView.FinishTime = objResult.FinishTime;
            tempView.NumberOfAttending = objResult.NumberOfAttending;
            tempView.CarSpaceRequired = objResult.CarSpaceRequired;
            tempView.HouseKeepingRequired = objResult.HouseKeepingRequired;
            tempView.SecurityRequired = objResult.SecurityRequired;
            tempView.Cost = objResult.Cost;
            tempView.LayoutInformation = objResult.LayoutInformation;
            tempView.TobeInvoiced = objResult.TobeInvoiced;
            tempView.TechnicianOnSite = objResult.TechnicianOnSite;
            tempView.DisabledAccess = objResult.DisabledAccess;
            tempView.ReturnedBookingForm = objResult.ReturnedBookingForm;
            tempView.Mobile = objResult.Mobile;
            tempView.DateFromSent = objResult.DateFromSent;
            tempView.BookingStatus = objResult.BookingStatus;
            tempView.AdditionalInformation = objResult.AdditionalInformation;
            tempView.CancellationDetail = objResult.CancellationDetail;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditBookingDetail", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> _AddEditBookingDetails(BookingDetailsDto model)
        {
            try
            {
                if (model.Id == 0)
                {
                    BookingDetails BD = new BookingDetails();
                    BD.UserGroupId = model.UserGroupId;
                    BD.MeetingTypeId = model.MeetingTypeId;
                    BD.RoomTypeId = model.RoomTypeId;
                    BD.StartTime = model.StartTime;
                    BD.FinishTime = model.FinishTime;
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
                    BD.Mobile = model.Mobile;
                    BD.DateFromSent = model.DateFromSent;
                    BD.BookingStatus = model.BookingStatus;
                    BD.AdditionalInformation = model.AdditionalInformation;
                    BD.CancellationDetail = model.CancellationDetail;
                    BD.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());

                    var bookingDetails = await _bookingDetail.InsertAsync(HttpContext.Session.GetString("ConnectionString"), BD, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (bookingDetails != null)
                    {

                        return JsonResponse.GenerateJsonResult(1, "Booking Details Created");
                    }
                    else
                        ErrorLog.AddErrorLog(null, "Error in create Booking Details");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }


                else
                {

                    var objResult = _bookingDetail.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                    objResult.UserGroupId = model.UserGroupId;
                    objResult.MeetingTypeId = model.MeetingTypeId;
                    objResult.RoomTypeId = model.RoomTypeId;
                    objResult.StartTime = model.StartTime;
                    objResult.FinishTime = model.FinishTime;
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
                    objResult.Mobile = model.Mobile;
                    objResult.DateFromSent = model.DateFromSent;
                    objResult.BookingStatus = model.BookingStatus;
                    objResult.AdditionalInformation = model.AdditionalInformation;
                    objResult.CancellationDetail = model.CancellationDetail;

                    var callLog = await _bookingDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (callLog != null)
                    {
                        return JsonResponse.GenerateJsonResult(1, "Booking Details Updated");
                    }
                    else
                    {
                        // txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "Error in Update Booking Details");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }

                }
            }
            catch (Exception e)
            {
                throw e;

            }
        }
        [HttpGet]
        public async Task<IActionResult> GetBookingDetailList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _bookingDetail.GetBookingDetailList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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

        [HttpPost]
        public async Task<IActionResult> RemoveBookingDetail(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var booking = _bookingDetail.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    booking.IsDelete = true;
                    await _bookingDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), booking, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.CallLogsDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveBookingDetail");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion

        #region Methods for Catering Requirements

        [HttpGet]
        public async Task<IActionResult> _AddEditCateringRequirements(long id, bool isView)
        {
            var catererList = await _cateringDetails.GetCatererList(HttpContext.Session.GetString("ConnectionString"));
            var tempView = new Catering_RequirementsDto();
            if (catererList.Count() != 0)
            {
                ViewBag.IsCatererDetailEmpty = false;
                ViewBag.CatererDetail = catererList.Select(x => new SelectListItem()
                {
                    Text = x.Title,
                    Value = x.Id.ToString()
                }).OrderBy(x => x.Text);
            }
            else
            {
                ViewBag.IsCatererDetailEmpty = true;
                tempView.IsView = true;
            }

            var menuList = await _menu.GetMenuListForDropDown(HttpContext.Session.GetString("ConnectionString"));
            if (menuList.Count() != 0)
            {
                ViewBag.IsmenuEmpty = false;
                ViewBag.MenuDetail = menuList.Select(x => new SelectListItem()
                {
                    Text = x.DescriptionOFFood,
                    Value = x.Id.ToString()
                }).OrderBy(x => x.Text);
            }
            else
            {
                ViewBag.IsmenuEmpty = true;
                tempView.IsView = true;
            }

            if (id == 0) return View(@"Components/_AddEditCateringRequirements", new Catering_RequirementsDto { Id = id, IsView = isView });
            var objResult = _catering_Requirements.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.CatererId = objResult.CatererId;
            tempView.Notes = objResult.Notes;
            tempView.TimeFor = objResult.TimeFor;
            tempView.TimeCollected = objResult.TimeCollected;
            tempView.NumberOfPeople = objResult.NumberOfPeople;
            tempView.Cost = objResult.Cost;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditCateringRequirements", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditCateringRequirements(Catering_RequirementsDto model)
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

                    var callLog = await _catering_Requirements.InsertAsync(HttpContext.Session.GetString("ConnectionString"), CR, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (callLog != null)
                    {

                        return JsonResponse.GenerateJsonResult(1, "Catering Requirement Created");
                    }
                    else
                        ErrorLog.AddErrorLog(null, "Error in create Catering Requirement");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }


                else
                {

                    var objResult = _catering_Requirements.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                    objResult.CatererId = model.CatererId;
                    objResult.Notes = model.Notes;
                    objResult.TimeFor = model.TimeFor;
                    objResult.TimeCollected = model.TimeCollected;
                    objResult.NumberOfPeople = model.NumberOfPeople;
                    objResult.Cost = model.Cost;

                    var callLog = await _catering_Requirements.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (callLog != null)
                    {
                        return JsonResponse.GenerateJsonResult(1, "Catering Requirement Updated");
                    }
                    else
                    {
                        // txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "Error in Update Catering Requirement");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }

                }
            }
            catch (Exception e)
            {
                throw e;

            }
        }
        [HttpGet]
        public async Task<IActionResult> GetCateringRequirementsList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                var allList = await _catering_Requirements.GetCatering_RequirementsList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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

        [HttpPost]
        public async Task<IActionResult> RemoveCateringRequirements(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var questionObj = _catering_Requirements.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _catering_Requirements.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.CallLogsDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveCateringRequirements");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion
    
    }
}