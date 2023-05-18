using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Models;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface.ClientAdmin;
using Integr8ed.Utility.Common;
using Integr8ed.Utility.JqueryDataTable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class BookingStatusController : BaseController<BookingStatusController>
    {
        #region Fields

        private readonly IConfiguration _config;
        private readonly IBookingStatusService _bookingStatus;
        private readonly IBookingDetailServices _bookingDetail;

        #endregion

        #region ctor
        public BookingStatusController(IBookingStatusService bookingStatus, IConfiguration config, IBookingDetailServices bookingDetails)
        {
            _bookingStatus = bookingStatus;
            _config = config;
            _bookingDetail = bookingDetails;
        }
        #endregion

        #region Method
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingStatusList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                //parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _bookingStatus.GetBookingStatusList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetStandardEquipmentList");
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
        public async Task<IActionResult> RemoveBookingStatus(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var BookingExist = _bookingDetail.GetBookingDetailForDelete(HttpContext.Session.GetString("ConnectionString")).Where(x => x.BookingStatus == id).ToList().Count();
                    if (BookingExist != 0)
                    {
                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.CheckBookingAvailableUsingBookingStatus);
                    }
                    else
                    {
                        if (id == 1 || id == 2 || id == 3)
                        {
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.BookingStatusNotDeleted);
                        }
                        //var recordExist = 
                        else
                        {
                            var Obj = _bookingStatus.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                            Obj.IsDelete = true;
                            await _bookingStatus.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), Obj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.BookingStatusDeleted);
                        }
                    }
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveRoomType");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public IActionResult _AddEditBookingStatus(long id, bool isView)
        {
            if (id == 0) return View(@"_AddEditBookingStatus", new BookingStatusDto { Id = id, IsView = isView, IsActive = true });
            var tempView = new BookingStatusDto();
            var objResult = _bookingStatus.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView = Mapper.Map<BookingStatusDto>(objResult);//now check
            tempView.IsView = isView;
            return View(@"_AddEditBookingStatus", tempView);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditBookingStatus(BookingStatusDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        var bookingStatusObj = new BookingStatus();
                        bookingStatusObj.Status = model.Status;
                        bookingStatusObj.ColorCode= model.ColorCode;

                        var statusCreated = await _bookingStatus.InsertAsync(HttpContext.Session.GetString("ConnectionString"), bookingStatusObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (statusCreated != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.BookingStatusCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Post - AddEditBookingStatus / Else  Create part");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {
                        var objResult = _bookingStatus.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.Status = model.Status;
                        objResult.ColorCode = model.ColorCode;
                        objResult.IsActive = model.IsActive;
                        
                        var statusUpdated = await _bookingStatus.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (statusUpdated != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.BookingStatusUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Post - AddEditBookingStatus / Else  Update part");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-AddEditBookingStatus");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        #endregion

        #region Common
        public bool CheckTitle(string Status, long Id)
        {
            bool isExist;
            var result = _bookingStatus.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                x => x.Status.ToLower().Equals(Status.ToLower()) && x.IsDelete == false);

            if (result != null)
            {
                isExist = result.Status.ToLower().Trim().Equals(Status.ToLower().Trim()) ? true : false;
                if (isExist)
                {
                    var resultExist = _bookingStatus.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                        x => x.Status.ToLower().Equals(Status.ToLower()) && x.Id == Id && x.IsDelete == false);
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
    }
}
