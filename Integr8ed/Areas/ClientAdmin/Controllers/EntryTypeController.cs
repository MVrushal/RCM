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
    public class EntryTypeController : BaseController<EntryTypeController>
    {
        #region Fields
        private readonly IEntryTypeServices _EntryType;
        private readonly IConfiguration _config;

        #endregion

        #region ctor
        public EntryTypeController(IEntryTypeServices EntryType, IConfiguration config)
        {
            _EntryType = EntryType;
            _config = config;

        }
        #endregion


        #region Method
        public IActionResult Index()
        {

            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);


            if (IsAccess("EntryType"))
                return View(new EntryTypeDto() { Id = 0 });
            else
                return RedirectToAction("Index", "Dashboard");
            
        }


        [HttpGet]
        public IActionResult _AddEditEntryType(long id, bool isView)
        {
            if (id == 0) return View(@"Components/_AddEditEntryType", new EntryTypeDto { Id = id, IsView = isView });
            var tempView = new EntryTypeDto();
            var objResult = _EntryType.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.Title = objResult.Title;
            tempView.Description = objResult.Description;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditEntryType", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditEntryType(EntryTypeDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        Entry_Type MT = new Entry_Type();
                        MT.code = model.Title;
                        MT.Title = model.Title;
                        MT.Description = model.Description;
                        MT.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());


                        var EntryType = await _EntryType.InsertAsync(HttpContext.Session.GetString("ConnectionString"), MT, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (EntryType != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.EntryTypeCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Entry Type");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {
                        var objResult = _EntryType.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.code = model.Title;
                        objResult.Title = model.Title;
                        objResult.Description = model.Description;

                        var Equipment = await _EntryType.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (Equipment != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.EntryTypeUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update clientadmin");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update clientadmin");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetEntryTypeList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _EntryType.GetEntryTypeList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetEntryTypeList");
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
        public async Task<IActionResult> RemoveEntryType(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var questionObj = _EntryType.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _EntryType.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.EntryTypeDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveEquipment");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        #endregion

        #region  Common
        public bool CheckEntryTypeTitle(string Title, long Id)
       {
            bool isExist;
            var result = _EntryType.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Title.ToLower().Equals(Title.ToLower()) && x.IsDelete == false
            && x.BranchId == Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));

            if (result != null)
            {

                isExist = result.Title.ToLower().Trim().Equals(Title.ToLower().Trim()) ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = _EntryType.GetSingle(HttpContext.Session.GetString("ConnectionString"),
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