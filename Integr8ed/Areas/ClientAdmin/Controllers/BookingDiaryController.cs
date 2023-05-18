using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Data.Utility;
using Integr8ed.Models;
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
using Rotativa.AspNetCore;
using static Integr8ed.Service.Enums.GlobalEnums;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class BookingDiaryController : BaseController<BookingDiaryController>
    {
        #region Fields
        private readonly IVisitorsService _IVisitors;
        private readonly IConfiguration _config;
        private readonly IErrorLogService _errorLog;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRoomTypesService _roomTypesService;
        private readonly IBookingStatusService _bookingStatus;
        private readonly IBookingDetailServices _bdService;
        private readonly IEquipmentRequiredForBookingServices _equipmentRequiredForBooking;
        private readonly IUserService _user;
        private readonly IBookingDetailServices _bookingDetail;

        #endregion

        #region ctor
        public BookingDiaryController(IVisitorsService visitors, IConfiguration config, IErrorLogService errorLog,
            EmailService emailService, IWebHostEnvironment webHostEnvironment, IRoomTypesService roomTypesService, IBookingStatusService bookingStatus,
            IBookingDetailServices bdService, IEquipmentRequiredForBookingServices equipmentRequiredForBookingServices, IUserService user, IBookingDetailServices bookingDetail
        )
        {
            _IVisitors = visitors;
            _config = config;
            _errorLog = errorLog;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _roomTypesService = roomTypesService;
            _bookingStatus = bookingStatus;
            _bdService = bdService;
            _equipmentRequiredForBooking = equipmentRequiredForBookingServices;
            _user = user;
            _bookingDetail = bookingDetail;
        }
        #endregion

        #region Method
        public IActionResult Index()
        {

            var test = _roomTypesService.GetRoomTypeListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()))
                        .Select(x => new Room_TypeDto()
                        {
                            Id = x.Id,
                            Title = x.Title
                        }).OrderBy(x => x.Title).ToList();
            return View(test);

            var BookingStatusList = _bookingStatus.GetBookingStatusListForDropDown(HttpContext.Session.GetString("ConnectionString"));
            if (BookingStatusList.Count() != 0)
            {
                ViewBag.IsBookingStatusEmpty = false;
                var data = BookingStatusList.Select(x => new SelectListItem()
                {
                    Text = x.Status.ToString(),
                    Value = x.ColorCode.ToString(),

                }).OrderBy(x => x.Text);
                ViewBag.BookingStatus = JsonConvert.SerializeObject(data);
            }            
        }

        public IActionResult GetRoomList()
        {
            var test = _roomTypesService.GetRoomTypeListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()))
                           .Select(x => new Room_TypeDto()
                           {
                               Id = x.Id,
                               Title = x.Title,
                               RoomOrder = x.RoomOrder
                           }).OrderBy(x => x.RoomOrder).ToList();
            return Json(test);
        }

        [HttpGet]
        public ActionResult GetCapacity(long rId)
        {
            var cap = _roomTypesService.GetById(HttpContext.Session.GetString("ConnectionString"),rId).Maxperson;
            return Json(cap);
        }


        [HttpGet]
        public async Task<IActionResult> BindDiary(string BookingDate,int BookingStatusId,int isCan)
        {
            try
            {

                //if(BookingDate == null)
                //{
                //    BookingDate = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                //}

                var BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
                var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@Input_Date",SqlDbType.VarChar){Value = BookingDate},
                    new SqlParameter("@BranchId",SqlDbType.BigInt){Value = BranchId},
                    new SqlParameter("@BookingStatusId",SqlDbType.Int){Value = BookingStatusId},
                    new SqlParameter("@IsCan",SqlDbType.Bit){Value = isCan},

                }.ToArray();


                var roomlist = await _roomTypesService.BindDiary(HttpContext.Session.GetString("ConnectionString"), Parameters);
                return JsonResponse.GenerateJsonResult(1, ResponseConstants.Success, roomlist);
            }
            catch (Exception e)
            {

                return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong, null);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateDiary(UpdateDiaryDto model)
        {
            var bookingDetailObj = _bdService.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.BookingId);
            if (bookingDetailObj == null)
            {
                return JsonResponse.GenerateJsonResult(2, ResponseConstants.NotFound, null);
            }
            else
            {
                bookingDetailObj.StartTime = model.StartTime;
                bookingDetailObj.FinishTime = model.EndTime;
                bookingDetailObj.RoomTypeId = model.RoomTypeId;

                bookingDetailObj.Cost = await calculateCost(model.BookingDate, model.StartTime, model.EndTime, model.RoomTypeId);

                var updateResult = await _bdService.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), bookingDetailObj, Accessor, Convert.ToInt64(HttpContext.Session.GetString("UserID")));
                if (updateResult != null)
                {
                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.Success, null);
                }
                else
                {
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong, null);
                }
            }
        }

        [HttpGet]
        public IActionResult _CopyBookingDetail(long id)
        {
            try
            {
                var roomTypeList = _roomTypesService.GetRoomTypeListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
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

                if (id == 0) return View(@"Components/_CopyBookingDetail", new BookingDetailsDto { Id = id });
                var objResult = _bdService.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                tempView = Mapper.Map<BookingDetailsDto>(objResult);
                tempView.BookingDateS = objResult.BookingDate.Date.ToString();
                tempView.Id = objResult.Id;
                tempView.RoomTypeId = objResult.RoomTypeId;
                return View(@"Components/_CopyBookingDetail", tempView);
            }
            catch (Exception ex)
            {
                return View(@"Components/_CopyBookingDetail", new BookingDetailsDto { Id = id, IsView = false }); ;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CopyBookingDetail(BookingDetailsDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                  
                    var objResult = await _bdService.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                    var isExistCount = _bdService.IsExistBooking(HttpContext.Session.GetString("ConnectionString"), model);
                   

                    var aStart = TimeSpan.Parse(objResult.StartTime);
                    var aEnd = TimeSpan.Parse(objResult.FinishTime);
                    var bStart = TimeSpan.Parse(model.StartTime);
                    var bEnd = TimeSpan.Parse(model.FinishTime);


                    if (isExistCount > 0)
                    {
                        txscope.Dispose();
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.BookingAlreadyExist);
                    }
                    else
                    {
                        objResult.BookingDate = DateTime.Parse(model.BookingDateForDisplay);
                        objResult.RoomTypeId = model.RoomTypeId;
                        objResult.Id = 0;
                        var bookingDetail = await _bdService.InsertAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (bookingDetail != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.BookingDetailsCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Method - CopyBookingDetail/Error in create Booking Details.");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create Booking Details");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public JsonResult BookingStatusListForBookingDiary()
        {
            var bookingStatusList = _bookingStatus.GetBookingStatusListForDropDown(HttpContext.Session.GetString("ConnectionString"));
            return Json(bookingStatusList.Select(x => new { Title = x.Status, Id = x.ColorCode }).ToArray());
        }

        //Update Booking Details While Using Drag and Drop Functionality
        [HttpPost]
        public async Task<IActionResult> UpdateBookingDetailsFromDiary(int Id, string StartTime, string FinishTime, int RoomTypeId)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var bookingObj = await _bookingDetail.GetSingleAsync(HttpContext.Session.GetString("ConnectionString"), x => x.Id == Id);
                    bookingObj.StartTime = StartTime;
                    bookingObj.FinishTime = FinishTime;
                    bookingObj.RoomTypeId = RoomTypeId;

                    await _bookingDetail.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), bookingObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.BookingDetailsUpdated);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "POST-RemoveBookingDetails");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        //public async Task<IActionResult> UpdateDiary(UpdateDiaryDto model)
        //{
        //    var bookingDetailObj = _bdService.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.BookingId);
        //    if (bookingDetailObj == null)
        //    {
        //        return JsonResponse.GenerateJsonResult(2, ResponseConstants.NotFound, null);
        //    }
        //    else
        //    {
        //        bookingDetailObj.StartTime = model.StartTime;
        //        bookingDetailObj.FinishTime = model.EndTime;
        //        bookingDetailObj.RoomTypeId = model.RoomTypeId;

        //        bookingDetailObj.Cost = await calculateCost(model.BookingDate, model.StartTime, model.EndTime, model.RoomTypeId);

        //        var updateResult = await _bdService.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), bookingDetailObj, Accessor, Convert.ToInt64(HttpContext.Session.GetString("UserID")));
        //        if (updateResult != null)
        //        {
        //            return JsonResponse.GenerateJsonResult(1, ResponseConstants.Success, null);
        //        }
        //        else
        //        {
        //            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong, null);
        //        }
        //    }
        //}
        #endregion

        #region  Common


        public async Task<decimal> calculateCost(string bookingDate, string startTime, string finishTime, long roomTypeId)
        {
            var Parameters = new List<SqlParameter>
                {
                    new SqlParameter("@BookingDate",SqlDbType.VarChar){Value = Convert.ToDateTime(bookingDate).ToString("yyyy/MM/dd")},
                    new SqlParameter("@StartTime",SqlDbType.VarChar){Value = startTime},
                    new SqlParameter("@FinishTime",SqlDbType.VarChar){Value = finishTime},
                    new SqlParameter("@RoomTypeId",SqlDbType.BigInt){Value = roomTypeId}
                };

            var costResult = await _bdService.CalculateBookingCostByDate(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());

            return costResult;
        }

        #endregion
    }
}