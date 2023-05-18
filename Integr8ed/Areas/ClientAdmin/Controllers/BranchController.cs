using System;
using System.Collections.Generic;
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

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class BranchController : BaseController<BranchController>
    {
        private readonly IBranchService _branchService;

        #region Fields
        #endregion

        #region Ctor
        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }
        #endregion

        #region Methods

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult _AddEditBranch(long id, bool isView)
        {
            if (id == 0) return View(@"Components/_AddEditBranch", new BranchDto { Id = id, IsView = isView });
            var tempView = new BranchDto();
            var objResult = _branchService.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.BranchName = objResult.BranchName;
            
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditBranch", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditBranch(BranchDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        BranchMaster Obj = new BranchMaster();
                        Obj.BranchName = model.BranchName;
                        Obj.IsActive = true;
                        

                        var Branch = await _branchService.InsertAsync(HttpContext.Session.GetString("ConnectionString"), Obj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (Branch != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.BranchCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Branch Master");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {
                        var objResult = _branchService.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.BranchName = model.BranchName;
                        
                        var Branch = await _branchService.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (Branch != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.BranchUpdated);
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
        public async Task<IActionResult> GetBranchMasterList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                var allList = await _branchService.BranchMasterList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetBranchMasterList");
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
        public async Task<IActionResult> DeactivateBranch(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var questionObj = _branchService.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsActive = !questionObj.IsActive;
                    var updateResult=await _branchService.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, updateResult.IsActive ? ResponseConstants.BranchActivated :ResponseConstants.BranchDeActivated);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveBranch");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
    #endregion
    #region  Common
    public bool CheckBranchName(string BranchName, long Id)
    {
        bool isExist;
        var result = _branchService.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.BranchName.ToLower().Equals(BranchName.ToLower()) && x.IsDelete == false);
        if (result != null)
        {

            isExist = result.BranchName.ToLower().Trim().Equals(BranchName.ToLower().Trim()) ? true : false;
            if (isExist && Id != 0)
            {
                var resultExist = _branchService.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                    x => x.BranchName.ToLower().Equals(BranchName.ToLower()) && x.Id == Id && x.IsDelete == false);
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
