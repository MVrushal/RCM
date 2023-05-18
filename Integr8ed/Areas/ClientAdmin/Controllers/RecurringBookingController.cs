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
using System.Globalization;
using System.Web;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class RecurringBookingController : BaseController<RecurringBookingController>
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly IRecurringBookingServices _recurringBooking;
        private readonly IRoomTypesService _roomTypes;
        private readonly IUserGroupServices _userGroup;
        private readonly IUserService _user;
        private readonly IBookingDetailServices _bookingDetail;
        private readonly IMeetingTypeServices _meetingType;
        private readonly IBookingStatusService _bookingStatus;
        

        #endregion

        #region ctor
        public RecurringBookingController(IMeetingTypeServices meetingTypeServices,IConfiguration config, IRecurringBookingServices recurringBooking, IRoomTypesService roomTypesService,
            IUserGroupServices userGroupServices, IUserService user, IBookingDetailServices bookingDetail, IBookingStatusService bookingStatus)
        {
            _config = config;
            _recurringBooking = recurringBooking;
            _roomTypes = roomTypesService;
            _userGroup = userGroupServices;
            _user = user;
            _bookingDetail = bookingDetail;
            _meetingType = meetingTypeServices;
            _bookingStatus = bookingStatus;
        }
        #endregion

        #region Method
        public IActionResult Index()
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);

            return View();
        }
        #endregion

        #region Recurring booking details

        [HttpGet]
        public async Task<IActionResult> _AddEditRecurringBookingDetails(long id, bool isView)
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
                var tempView = new RecurringBookingsDto();

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


                ViewBag.IsNameOfDaysEmpty = true;
                ViewBag.NameofDays = JsonResponse.GenerateJsonResult(0, "Data not found", JsonConvert.SerializeObject(null));

                if (id == 0) return View(@"Components/_AddEditRecurringBookingDetails", new RecurringBookingsDto { Id = id, IsView = isView });
                var objResult = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                tempView.BookingDate = objResult.BookingDate;
                tempView.TimeStart = objResult.StartTime;
                tempView.TimeEnd = objResult.FinishTime;
                tempView.RoomTypeId = objResult.RoomTypeId;
                tempView.BookingDateForDisplay = Convert.ToDateTime(objResult.BookingDate).ToString("dd-MM-yyyy");
                tempView.Id = objResult.Id;
                tempView.IsView = isView;
                return View(@"Components/_EditRecurringBookingDetails", tempView);
            }
            catch (Exception ex)
            {
                return View(@"Components/_AddEditRecurringBookingDetails", new RecurringBookingsDto { Id = id, IsView = false }); ;
            }
        }

        [HttpGet]
        public async Task<IActionResult> _EditRecurringBookingDetails(RecurringBookingsDto model)
        {
            var roomTypeList = _roomTypes.GetRoomTypeListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            var tempView = new RecurringBookingsDto();

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
            return View(@"Components/_EditRecurringBookingDetails", model);
        }

        [HttpGet] 
        public async Task<IActionResult> GetNameofDays()
        {
            var NameOfDays = System.Enum.GetValues(typeof(GlobalEnums.NameOfDays)).Cast<GlobalEnums.NameOfDays>()
                     .Select(d => (d, (int)d))
                     .ToList();

            var multiSelectDropdownModels = NameOfDays.Select(x => new MultiSelectDropdownModel()
            {
                label = x.d.ToString(),
                value = x.Item2.ToString()
            }).ToList();

            if (multiSelectDropdownModels != null)
            {
                ViewBag.IsNameOfDaysEmpty = false;
                ViewBag.NameofDays = JsonResponse.GenerateJsonResult(1, "Data found", JsonConvert.SerializeObject(multiSelectDropdownModels));
                return JsonResponse.GenerateJsonResult(1, "Data found", JsonConvert.SerializeObject(multiSelectDropdownModels));
            }
            else
            {
                ViewBag.IsNameOfDaysEmpty = true;
                ViewBag.NameofDays = JsonResponse.GenerateJsonResult(0, "Data not found", JsonConvert.SerializeObject(multiSelectDropdownModels));
                return JsonResponse.GenerateJsonResult(0, "Data not found", JsonConvert.SerializeObject(multiSelectDropdownModels));
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEditRecurringBookingDetails(List<RecurringBookingsDto> models)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    foreach (var model in models)
                    {
                        BookingDetails BD = new BookingDetails();
                        BD.UserGroupId = model.UserGroupId;
                        BD.RoomTypeId = model.RoomTypeId;
                        BD.BookingDate = DateTime.ParseExact(model.bookingDateRecurring, "yyyy-MM-dd", CultureInfo.InvariantCulture);// model.BookingDate;
                        BD.StartTime = model.TimeStart;
                        BD.FinishTime = model.TimeEnd;
                        BD.TitleOfMeeting = model.EventTitle;
                        BD.NumberOfAttending = model.NumberOfAttending;
                        BD.CarSpaceRequired = model.CarSpaceRequired;
                        BD.BookingContact = model.BookingContact;
                        BD.Mobile = model.Mobile;
                        BD.BookingStatus = model.BookingStatus;
                        BD.ExternalBookingClientId = model.ExternalBookingClientId;
                        BD.MeetingTypeId = null;
                        BD.Cost = await calculateCost(DateTime.ParseExact(model.BookingDate.ToString("MM-dd-yyyy"), "MM-dd-yyyy", CultureInfo.InvariantCulture).ToShortDateString(), model.TimeStart, model.TimeEnd,model.RoomTypeId);
                        BD.IsActive = true;
                        BD.MeetingTypeId = model.MeetingTypeId;
                        BD.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
                        


                        var bookingDetail = await _bookingDetail.InsertAsync(HttpContext.Session.GetString("ConnectionString"), BD, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (bookingDetail != null)
                        {
                            RecurringBookings RB = new RecurringBookings();
                            RB.BookingDetailId = bookingDetail.Id;

                            var recurringBooking = await _recurringBooking.InsertAsync(HttpContext.Session.GetString("ConnectionString"), RB, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                            if (recurringBooking == null)
                            {
                                txscope.Dispose();
                                ErrorLog.AddErrorLog(null, "Error in create Recurring Booking");
                                return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                            }

                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Recurring Booking");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    txscope.Complete();
                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.RecurringBookingCreated);
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create Recurring Booking");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRecurringBookingDetails(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var booking = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    booking.IsDelete = true;
                    var updatedBooking = await _bookingDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), booking, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));

                    if (updatedBooking != null)
                    {
                        var questionObj = await _recurringBooking.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.BookingDetailId == updatedBooking.Id);
                        questionObj.IsDelete = true;
                        await _recurringBooking.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        txscope.Complete();
                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.RecurringBookingDeleted);
                    }
                    txscope.Dispose();
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveRecurringBookingDetails");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecurringBookingDetailList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _recurringBooking.GetRecurringBookingDetailList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());

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
                ErrorLog.AddErrorLog(ex, "GetRecurringBookingDetailList");
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
        public async Task<IActionResult> GetBookingDetailById(long ExternalBookingClientId, long RoomTypeId)
        {
            if (ExternalBookingClientId > 0)
            {
                var User = await _user.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == ExternalBookingClientId);
                var RoomType = await _roomTypes.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == RoomTypeId);
                return JsonResponse.GenerateJsonResult(1, "", new RecurringBookingsDto()
                {
                    RoomTypeName = RoomType.Title,
                    RoomTypeId = RoomType.Id,
                    UserName = User.FirstName + " " + User.LastName,
                    ExternalBookingClientId = User.Id
                    
                });
            }
            else
            {
                var RoomType = await _roomTypes.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == RoomTypeId);
                return JsonResponse.GenerateJsonResult(1, "", new RecurringBookingsDto()
                {
                    RoomTypeName = RoomType.Title,
                    RoomTypeId = RoomType.Id
                });
            }

        }

        [HttpPost]
        public IActionResult CheckIsBookingAvailable(RecurringBookingsDto model)
        {
            var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@DateFrom",SqlDbType.VarChar){Value = model.DateFrom},
                    new SqlParameter("@DateTo",SqlDbType.VarChar){Value = model.DateTo},
                    new SqlParameter("@TimeStart",SqlDbType.VarChar){Value = model.TimeStart},
                    new SqlParameter("@TimeEnd",SqlDbType.VarChar){Value = model.TimeEnd},
                    new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = model.RoomTypeId},
                    new SqlParameter("@EventTitle",SqlDbType.VarChar){Value = model.EventTitle}
                };

            var result = _recurringBooking.CheckIsBookingAvailable(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());

            if (result == "")
            {
                return JsonResponse.GenerateJsonResult(0, "", null);
            }
            return JsonResponse.GenerateJsonResult(1, "No room available on this date", null);
        }

        [HttpGet]
        public NoofDatesForBooking GetTotalDays(DateTime startDate, DateTime endDate, String days)
        {
            var selectedDates = new List<DateTime?>();
            var selectedDays = new List<string>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                selectedDates.Add(date);
                //selectedDays.Add(date.ToString("dd/MM/yyyy"));
            }
            if (days != null)
            {
                string[] idLst = days.Split(",");
                List<string> dayList = new List<string>();
                foreach (var id in idLst)
                {
                    var NameOfDay = System.Enum.GetValues(typeof(GlobalEnums.NameOfDays)).Cast<GlobalEnums.NameOfDays>()
                     .Select(d => (d, (int)d)).Where(d => d.Item2 == Convert.ToInt32(id)).FirstOrDefault().d;
                    dayList.Add(NameOfDay.ToString());
                }
                    selectedDates.RemoveAll(x => !dayList.Contains(x.Value.ToString("dddd")));
            }
            foreach (var item in selectedDates)
            {
                selectedDays.Add(item.Value.ToString("dd/MM/yyyy"));
            }
            var NoofDatesForBooking = new NoofDatesForBooking()
            {
                DateTimes = selectedDays,
                Count = selectedDates.Count()
            };

            return NoofDatesForBooking;
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBookingDetails(RecurringBookingsDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var objResult = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);

                    objResult.RoomTypeId = model.RoomTypeId;
                    objResult.BookingDate = model.BookingDate;
                    objResult.StartTime = model.TimeStart;
                    objResult.FinishTime = model.TimeEnd;

                    var bookingDetail = await _bookingDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (bookingDetail != null)
                    {
                        txscope.Complete();
                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.RecurringBookingUpdated);
                    }
                    else
                    {
                        txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "Error in Update Recurring Booking");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Update Recurring Booking");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }


        public async Task<decimal> calculateCost(string bookingDate,string startTime,string finishTime,long roomTypeId)
        {



            var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@BookingDate",SqlDbType.VarChar){Value = Convert.ToDateTime(bookingDate).ToString("yyyy/MM/dd")},
                    new SqlParameter("@StartTime",SqlDbType.VarChar){Value = startTime},
                    new SqlParameter("@FinishTime",SqlDbType.VarChar){Value = finishTime},
                    new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = roomTypeId}
                };

            var costResult = await _bookingDetail.CalculateBookingCostByDate(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());

            return costResult;
        }

        #endregion

        #region Common
        [HttpPost]
        public async  Task<bool> CheckIsBookingDetailAvailableRecurring(RecurringBookingsDto model)
        {
            string[] idLst = model.BookingDay.Split(",");
            string dayList = "";
            foreach (var id in idLst)
            {
                var NameOfDay = System.Enum.GetValues(typeof(GlobalEnums.NameOfDays)).Cast<GlobalEnums.NameOfDays>()
                 .Select(d => (d, (int)d)).Where(d => d.Item2 == Convert.ToInt32(id)).FirstOrDefault().d;
                dayList+=NameOfDay.ToString()+",";
            }

            var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@BookingFrom",SqlDbType.VarChar){Value = model.DateFrom},
                    new SqlParameter("@BookingTo",SqlDbType.VarChar){Value = model.DateTo},
                    new SqlParameter("@StartTime",SqlDbType.VarChar){Value = model.TimeStart},
                    new SqlParameter("@FinishTime",SqlDbType.VarChar){Value = model.TimeEnd},
                    new SqlParameter("@Day",SqlDbType.VarChar){Value = dayList},
                    new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = model.RoomTypeId}
                };

            var roomResult = await _bookingDetail.CheckRecurringbookingAvailable(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
            return roomResult;
        }

        //[HttpGet]
        //public Task<List<RecurringBookingsDto>> IsBookingDetailAvailableRecurring (RecurringBookingsDto model)
        //{
        //    string[] idLst = model.BookingDay.Split(",");
        //    string dayList = "";
        //    foreach (var id in idLst)

        //    {
        //        var NameOfDay = System.Enum.GetValues(typeof(GlobalEnums.NameOfDays)).Cast<GlobalEnums.NameOfDays>()
        //         .Select(d => (d, (int)d)).Where(d => d.Item2 == Convert.ToInt32(id)).FirstOrDefault().d;
        //        dayList += NameOfDay.ToString() + ",";
        //    }

        //    var Parameters = new List<SqlParameter>
        //        {
        //            new SqlParameter("@BookingFrom",SqlDbType.VarChar){Value = model.DateFrom},
        //            new SqlParameter("@BookingTo",SqlDbType.VarChar){Value = model.DateTo},
        //            new SqlParameter("@StartTime",SqlDbType.VarChar){Value = model.TimeStart},
        //            new SqlParameter("@FinishTime",SqlDbType.VarChar){Value = model.TimeEnd},
        //            new SqlParameter("@Day",SqlDbType.VarChar){Value = dayList},
        //            new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = model.RoomTypeId}
        //        };

        //    var roomResult = _bookingDetail.CheckRecurringbookingAvailableList(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
        //    return roomResult;
        //}

        [HttpGet]
        public async Task<IActionResult> IsBookingDetailAvailableRecurring(string DateFrom,string DateTo, string TimeStart, string TimeEnd,int RoomTypeId, string BookingDay)
        {
            try
            {
                string[] idLst = BookingDay.Split(",");
                string dayList = "";
                foreach (var id in idLst)

                {
                    var NameOfDay = System.Enum.GetValues(typeof(GlobalEnums.NameOfDays)).Cast<GlobalEnums.NameOfDays>()
                     .Select(d => (d, (int)d)).Where(d => d.Item2 == Convert.ToInt32(id)).FirstOrDefault().d;
                    dayList += NameOfDay.ToString() + ",";
                }

                var BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
                var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@BookingFrom",SqlDbType.VarChar){Value = DateFrom},
                    new SqlParameter("@BookingTo",SqlDbType.VarChar){Value = DateTo},
                    new SqlParameter("@StartTime",SqlDbType.VarChar){Value = TimeStart},
                    new SqlParameter("@FinishTime",SqlDbType.VarChar){Value = TimeEnd},
                    new SqlParameter("@Day",SqlDbType.VarChar){Value = dayList},
                    new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = RoomTypeId}
                };
                var comparam = new List<SqlParameter>
                {
                    new SqlParameter("@Companycode",SqlDbType.VarChar){Value =HttpContext.Session.GetString("CompanyCode").ToString()},
                };
                var recurringBookingsList = await _recurringBooking.GetRecurringBookingReportPDF(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
                recurringBookingsList.ForEach(x => x.ReportTitle = "Existing Bookings");
                

                
                string customSwitches = string.Format("--print-media-type --allow {0} --footer-html {0} --footer-spacing -10",
               Url.Action("Footer", "Document", new { area = "" }, "https"));
                var report = new ViewAsPdf("ExistingBookingDetails", recurringBookingsList)
                {
                    FileName = "ExistingBookings.pdf",
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
        #endregion
    }
}