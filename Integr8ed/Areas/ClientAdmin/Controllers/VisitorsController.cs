using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class VisitorsController : BaseController<VisitorsController>
    {
        #region Fields
        private readonly IVisitorsService _IVisitors;
        private readonly IConfiguration _config;
        private readonly IErrorLogService _errorLog;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        #endregion

        #region ctor
        public VisitorsController(IVisitorsService visitors, IConfiguration config, IErrorLogService errorLog,
            EmailService emailService, IWebHostEnvironment webHostEnvironment)
        {
            _IVisitors = visitors;
            _config = config;
            _errorLog = errorLog;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
        }
        #endregion


        #region Method
        public IActionResult Index()
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);


            if (IsAccess("DelegetsCodes"))
                return View(new VisitorDto() { Id = 0 });
            else
                return RedirectToAction("Index", "Dashboard");

        }


        [HttpGet]
        public IActionResult _AddEditVisitor(long id, bool isView, bool IsEmailEdit)
        {
            if (id == 0) return View(@"Components/_AddEditVisitor", new VisitorDto { Id = id, IsView = isView, IsEmailEdit = IsEmailEdit });
            var tempView = new VisitorDto();
            var objResult = _IVisitors.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.Description = objResult.Description;
            tempView.Name = objResult.Name;
            tempView.SurName = objResult.SurName;
            tempView.Address = objResult.Address;
            tempView.PostCode = objResult.PostCode;
            tempView.Email = objResult.Email;
            tempView.Telephone = objResult.Telephone;
            tempView.Mobile = objResult.Mobile;
            tempView.Notes = objResult.Notes;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            tempView.IsEmailEdit = IsEmailEdit;
            return View(@"Components/_AddEditVisitor", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditVisitor(VisitorDto model)
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

                        var visitors = await _IVisitors.InsertAsync(HttpContext.Session.GetString("ConnectionString"), MT, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (visitors != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.VisitorCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Visitor");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {
                        var objResult = _IVisitors.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        if (model.IsEmailEdit == true)
                        {
                            objResult.Email = model.Email;
                        }
                        else
                        {
                            objResult.Description = model.Description;
                            objResult.Name = model.Name;
                            objResult.SurName = model.SurName;
                            objResult.Address = model.Address;
                            objResult.PostCode = model.PostCode;
                            objResult.Telephone = model.Telephone;
                            objResult.Mobile = model.Mobile;
                            objResult.Notes = model.Notes;
                        }
                        var visitors = await _IVisitors.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (visitors != null)
                        {
                            txscope.Complete();
                            if (model.IsEmailEdit == true)
                            {
                                var resetUrl = _config["CommonProperty:PhysicalUrl"];
                                string emailTemplate = CommonMethod.ReadEmailTemplate(_errorLog, _webHostEnvironment.WebRootPath, "ChangeEmail.html", resetUrl);
                                emailTemplate = emailTemplate.Replace("{UserName}", visitors.Name + " " + visitors.SurName);
                                await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                                {
                                    ToDisplayName = visitors.Name + " " + visitors.SurName,
                                    ToAddress = visitors.Email,
                                    Subject = "Change Email",
                                    BodyText = emailTemplate
                                });
                                return JsonResponse.GenerateJsonResult(1, "Visitor Email Updated Successfully");
                            }
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.VisitorUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Visitor");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Visitor");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetVisitorList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _IVisitors.GetVisitorList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetVisitorList");
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
        public async Task<IActionResult> RemoveVisitor(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var questionObj = _IVisitors.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _IVisitors.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.VisitorDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-VisitorDeleted");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        #endregion

        #region  Common


     
        public bool CheckVisitorEmail(string Email, long Id)
        {
            bool isExist;
            var result = _IVisitors.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                x => x.Email.ToLower().Equals(Email.ToLower()) && x.IsDelete == false);

            if (result != null)
            {

                isExist = result.Email.ToLower().Trim().Equals(Email.ToLower().Trim()) ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = _IVisitors.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                        x => x.Email.ToLower().Equals(Email.ToLower()) && x.Id == Id && x.IsDelete == false);

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