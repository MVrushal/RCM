using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Models;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface.ClientAdmin;
using Integr8ed.Utility.Common;
using Integr8ed.Utility.JqueryDataTable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class UserGroupController : BaseController<UserGroupController>
    {
        #region Fields
        private readonly IUserGroupServices _IUserGroup;
        private readonly IConfiguration _config;
        private readonly IBookingDetailServices _bookingDetail;
        #endregion

        #region ctor
        public UserGroupController(IUserGroupServices userGroup, IConfiguration config, IBookingDetailServices bookingDetails)
        {
            _IUserGroup = userGroup;
            _config = config;
            _bookingDetail = bookingDetails;
        }
        #endregion

        #region Method
        public IActionResult Index()
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);

            if (IsAccess("UserGroup"))
                return View(new UserGroupDto() { Id = 0 });
            else
                return RedirectToAction("Index", "Dashboard");

        }

        [HttpGet]
        public IActionResult _AddEditUserGroupType(long id, bool isView)
        {
            if (id == 0) return View(@"Components/_AddEditUserGroupType", new UserGroupDto { Id = id, IsView = isView });
            var tempView = new UserGroupDto();
            var objResult = _IUserGroup.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.Title = objResult.Title;
            tempView.Description = objResult.Description;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditUserGroupType", tempView);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditUserGroup(UserGroupDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        UserGroup MT = new UserGroup();
                        MT.code = model.Title;
                        MT.Title = model.Title;
                        MT.Description = model.Description;
                        MT.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());


                        var meetingType = await _IUserGroup.InsertAsync(HttpContext.Session.GetString("ConnectionString"), MT, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (meetingType != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.UserGroupCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create UserGroup");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }


                    else
                    {

                        var objResult = _IUserGroup.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.code = model.Title;
                        objResult.Title = model.Title;
                        objResult.Description = model.Description;

                        var meetingType = await _IUserGroup.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (meetingType != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.UserGroupUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update UserGroup");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update UserGroup");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserGroupList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _IUserGroup.GetUserGroupList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetUserGroupList");
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
        public async Task<IActionResult> RemoveUserGroup(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var BookingExist = _bookingDetail.GetBookingDetailForDelete(HttpContext.Session.GetString("ConnectionString")).Where(x => x.UserGroupId == id).ToList().Count();
                    if (BookingExist != 0)
                    {
                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.CheckBookingAvailableUsingUserGroup);
                    }
                    else
                    {
                        var questionObj = _IUserGroup.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                        questionObj.IsDelete = true;
                        await _IUserGroup.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        txscope.Complete();

                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.UserGroupDeleted);
                    }
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-UserGroupDeleted");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        #endregion

        #region Common
        public bool CheckUserGroupTitle(string Title, long Id)
        {
            bool isExist;
            var result = _IUserGroup.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                x => x.Title.ToLower().Equals(Title.ToLower()) && x.IsDelete == false && x.BranchId == Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));

            if (result != null)
            {
                isExist = result.Title.ToLower().Trim().Equals(Title.ToLower().Trim()) ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = _IUserGroup.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                        x => x.Title.ToLower().Equals(Title.ToLower()) && x.Id == Id && x.IsDelete == false);
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