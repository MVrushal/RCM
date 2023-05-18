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
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class InvoiceItemController : BaseController<InvoiceItemController>
    {
        #region Fields
        private readonly IInvoiceServices _invoice;
        private readonly IConfiguration _config;
        #endregion

        #region ctor
        public InvoiceItemController(IInvoiceServices invoice, IConfiguration config)
        {
            _invoice = invoice;
            _config = config;
        }
        #endregion


        #region Method
        public IActionResult Index()
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);


            if (IsAccess("InvoiceItem"))
                return View(new InvoiceDto() { Id = 0 });
            else
                return RedirectToAction("Index", "Dashboard");
          
        }


        [HttpGet]
        public IActionResult _AddEditInvoice(long id, bool isView)
        {
            if (id == 0) return View(@"Components/_AddEditInvoice", new InvoiceDto { Id = id, IsView = isView });
            var tempView = new InvoiceDto();
            var objResult = _invoice.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.Title = objResult.Title;
            tempView.Description = objResult.Description;
            tempView.Vate = objResult.Vate;
            tempView.IteamCost = objResult.IteamCost;
            tempView.BudgetRate = objResult.BudgetRate;
            tempView.IsIteamVatable = objResult.IsIteamVatable;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditInvoice", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditInvoice(InvoiceDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        Invoice In = new Invoice();
                        In.code = model.Title;
                        In.Title = model.Title;
                        In.Description = model.Description;
                        In.Vate = model.Vate;
                        In.IteamCost = model.IteamCost;
                        In.BudgetRate = model.BudgetRate;
                        In.IsIteamVatable = model.IsIteamVatable;
                        In.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());

                        var invoice = await _invoice.InsertAsync(HttpContext.Session.GetString("ConnectionString"), In, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (invoice != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.InvoiceCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Invoice");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {

                        var objResult = _invoice.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.code = model.Title;
                        objResult.Title = model.Title;
                        objResult.Description = model.Description;
                        objResult.Vate = model.Vate;
                        objResult.IteamCost = model.IteamCost;
                        objResult.BudgetRate = model.BudgetRate;
                        objResult.IsIteamVatable = model.IsIteamVatable;

                        var invoice = await _invoice.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (invoice != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.InvoiceUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Invoice");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Invoice");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoiceList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _invoice.GetInvoiceList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetInvoiceList");
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
        public async Task<IActionResult> RemoveInvoice(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var invoiceObj = _invoice.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    invoiceObj.IsDelete = true;
                    await _invoice.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), invoiceObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.InvoiceDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-InvoiceDeleted");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> ManageInvoiceIsIteamVatable(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var objResult = _invoice.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    objResult.IsIteamVatable = !objResult.IsIteamVatable;

                    var invoice = await _invoice.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (invoice != null)
                    {
                        txscope.Complete();
                        return JsonResponse.GenerateJsonResult(1, $@"Invoice Item {(objResult.IsIteamVatable ? "activated" : "deactivated")} successfully.");
                    }
                    else
                    {
                        txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "Error in Update Invoice Item Vatable");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Update Invoice Item Vatable");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion

        #region Common
        public bool CheckInvoiceTitle(string Title, long Id)
        {
            bool isExist;
            var result = _invoice.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                x => x.Title.ToLower().Equals(Title.ToLower()) && x.IsDelete == false && x.BranchId == Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            if (result != null)
            {

                isExist = result.Title.ToLower().Trim().Equals(Title.ToLower().Trim()) ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = _invoice.GetSingle(HttpContext.Session.GetString("ConnectionString"),
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