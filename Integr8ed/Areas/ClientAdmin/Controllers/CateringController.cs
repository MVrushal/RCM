using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Models;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Enums;
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

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class CateringController : BaseController<CateringController>
    {
        #region Fields
        private readonly ICateringDetailsServices _cateringDetails;
        private readonly IMenuServices _menu;
        private readonly ICallLogsServices _callLogs;
        private readonly IEntryTypeServices _entryType;
        private readonly IProfileServices _profile;
        private readonly ICatererMenuServices _catMenu;
        private readonly IConfiguration _config;
        private readonly IErrorLogService _errorLog;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        #endregion

        #region ctor
        public CateringController(ICatererMenuServices catMenu, ICateringDetailsServices cateringDetails, IMenuServices menu, ICallLogsServices callLogs,
            IEntryTypeServices entryType, IProfileServices profile, IConfiguration config, IErrorLogService errorLog,
            EmailService emailService, IWebHostEnvironment webHostEnvironment)
        {
            _cateringDetails = cateringDetails;
            _menu = menu;
            _callLogs = callLogs;
            _entryType = entryType;
            _profile = profile;
            _catMenu = catMenu;
            _config = config;
            _errorLog = errorLog;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
        }
        #endregion

        #region Methods for Catering details
        public IActionResult Index()
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);


            if (IsAccess("CateringDetail"))
                return View(new CateringDetailsDto() { Id = 0 });
            else
                return RedirectToAction("Index", "Dashboard");

        }


        [HttpGet]
        public IActionResult _AddEditCateringDetail(long id, bool isView, bool IsEmailEdit)
        {
            if (id == 0) return View(@"Components/_AddEditCateringDetail", new CateringDetailsDto { Id = id, IsView = isView, IsEmailEdit = IsEmailEdit });
            var tempView = new CateringDetailsDto();
            var objResult = _cateringDetails.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.Title = objResult.Title;
            tempView.Description = objResult.Description;
            tempView.CatererName = objResult.CatererName;
            tempView.ContactName = objResult.ContactName;
            tempView.Telephone = objResult.Telephone;
            tempView.Email = objResult.Email;
            tempView.FaxNumber = objResult.FaxNumber;
            tempView.Address = objResult.Address;
            tempView.PostCode = objResult.PostCode;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            tempView.IsEmailEdit = IsEmailEdit;
            return View(@"Components/_AddEditCateringDetail", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditCateringDetail(CateringDetailsDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        Catering_Details MT = new Catering_Details();
                        MT.Title = model.Title;
                        MT.Description = model.Description;
                        MT.CatererName = model.CatererName;
                        MT.ContactName = model.ContactName;
                        MT.Telephone = model.Telephone;
                        MT.FaxNumber = model.FaxNumber;
                        MT.Email = model.Email;
                        MT.Address = model.Address;
                        MT.PostCode = model.PostCode;
                        MT.IsActive = true;
                        MT.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());

                        var cateringDetail = await _cateringDetails.InsertAsync(HttpContext.Session.GetString("ConnectionString"), MT, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (cateringDetail != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.CateringDetailCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Caterer Detail");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {
                        var objResult = _cateringDetails.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);

                        if (model.IsEmailEdit == true)
                        {
                            objResult.Email = model.Email;
                        }
                        else
                        {
                            objResult.FaxNumber = model.FaxNumber;
                            objResult.Address = model.Address;
                            objResult.PostCode = model.PostCode;
                            objResult.IsActive = model.IsActive;
                            objResult.Title = model.Title;
                            objResult.Description = model.Description;
                            objResult.CatererName = model.CatererName;
                            objResult.ContactName = model.ContactName;
                            objResult.Telephone = model.Telephone;
                        }

                        var cateringDetail = await _cateringDetails.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt64(HttpContext.Session.GetString("UserID")));
                        if (cateringDetail != null)
                        {
                            txscope.Complete();
                            if (model.IsEmailEdit == true)
                            {
                                var resetUrl = _config["CommonProperty:PhysicalUrl"];
                                string emailTemplate = CommonMethod.ReadEmailTemplate(_errorLog, _webHostEnvironment.WebRootPath, "ConfirmEmail.html", resetUrl);
                                emailTemplate = emailTemplate.Replace("{UserName}", cateringDetail.CatererName);
                                await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                                {
                                    ToDisplayName = cateringDetail.CatererName,
                                    ToAddress = cateringDetail.Email,
                                    Subject = "Confirm Email",
                                    BodyText = emailTemplate
                                });
                                return JsonResponse.GenerateJsonResult(1, "Caterer Email Updated Successfully");
                            }
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.CateringDetailUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Caterer Detail");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Caterer Detail");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetCateringDetailList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _cateringDetails.GetCateringDetailList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetCateringDetailList");
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
        public async Task<IActionResult> RemoveCateringDetail(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var questionObj = _cateringDetails.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _cateringDetails.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.CateringDetailDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveCateringDetail");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }


        #endregion

        #region Methods for Menu

        [HttpGet]
        public IActionResult _AddEditMenu(long id, bool isView)
        {
            if (id == 0) return View(@"Components/_AddEditMenu", new MenuDto { Id = id, IsView = isView });
            var tempView = new MenuDto();
            var objResult = _menu.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.Notes = objResult.Notes;
            tempView.DescriptionOFFood = objResult.DescriptionOFFood;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditMenu", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditMenu(MenuDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        Menu MT = new Menu();
                        MT.Notes = model.Notes;
                        MT.DescriptionOFFood = model.DescriptionOFFood;
                        MT.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());

                        var menu = await _menu.InsertAsync(HttpContext.Session.GetString("ConnectionString"), MT, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (menu != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.MenuCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Create Menu");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }


                    else
                    {

                        var objResult = _menu.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.Notes = model.Notes;
                        objResult.DescriptionOFFood = model.DescriptionOFFood;

                        var menu = await _menu.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (menu != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.MenuUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Menu");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Menu");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetMenuList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _menu.GetMenuList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetMenuList");
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
        public async Task<IActionResult> RemoveMenu(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var questionObj = _menu.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _menu.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.MenuDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveMenu");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion

        #region Methods for Call log

        [HttpGet]
        public IActionResult _AddEditCallLog(long id, bool isView)
        {
            var entryList = _entryType.GET_EntryTypeList(HttpContext.Session.GetString("ConnectionString"),Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            var tempView = new CallLogsDto();
            if (entryList.Count() != 0)
            {
                ViewBag.IsEntryTypeEmpty = false;
                ViewBag.EntryTypeList = entryList.Select(x => new SelectListItem()
                {
                    Text = x.Title,
                    Value = x.Id.ToString()
                }).OrderBy(x => x.Text);
            }
            else
            {
                ViewBag.IsEntryTypeEmpty = true;
                tempView.IsView = true;
            }

            var userlist = _profile.GetUserList(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            if (userlist.Count() != 0)
            {
                ViewBag.IsUserEmpty = false;
                ViewBag.UserList = userlist.Select(x => new SelectListItem()
                {
                    Text = x.FirstName + " " + x.LastName,
                    Value = x.ID.ToString()
                }).OrderBy(x => x.Text);
            }
            else
            {
                ViewBag.IsUserEmpty = true;
                tempView.IsView = true;
            }

            if (id == 0) return View(@"Components/_AddEditCallLog", new CallLogsDto { Id = id, IsView = isView });
            var objResult = _callLogs.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.Subject = objResult.Subject;
            tempView.EntryType = objResult.EntryType;
            tempView.DateOfentry = Convert.ToDateTime(objResult.DateOfentry);
            tempView.EntryDate = objResult.DateOfentry.ToShortDateString();
            tempView.NextconDate = objResult.NextContactDate.ToShortDateString();
            tempView.Time = objResult.Time;
            tempView.Contact = objResult.Contact;
            tempView.Address = objResult.Address;
            tempView.Comments = objResult.Comments;
            tempView.PostCode = objResult.PostCode;
            tempView.TakenBy = objResult.TakenBy;
            tempView.TakenFor = objResult.TakenFor;
            tempView.NextContactDate = objResult.NextContactDate.Date;
            tempView.ISCompleted = objResult.ISCompleted;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditCallLog", tempView);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditCallLog(CallLogsDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        CallLogs CL = new CallLogs();
                        CL.Subject = model.Subject;
                        CL.EntryType = model.EntryType;
                        CL.DateOfentry = model.DateOfentry;
                        CL.Time = model.Time;
                        CL.Contact = model.Contact;
                        CL.Address = model.Address;
                        CL.Comments = model.Comments;
                        CL.PostCode = model.PostCode;
                        CL.TakenBy = model.TakenBy;
                        CL.TakenFor = model.TakenFor;
                        CL.NextContactDate = model.NextContactDate;
                        CL.ISCompleted = model.ISCompleted;
                        CL.Associated = (int)GlobalEnums.AssociatedCallLog.CateringDetail;

                        var callLog = await _callLogs.InsertAsync(HttpContext.Session.GetString("ConnectionString"), CL, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (callLog != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.CallLogCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create CallLog");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {

                        var objResult = _callLogs.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.Subject = model.Subject;
                        objResult.EntryType = model.EntryType;
                        objResult.DateOfentry = model.DateOfentry;
                        objResult.Time = model.Time;
                        objResult.Contact = model.Contact;
                        objResult.Address = model.Address;
                        objResult.Comments = model.Comments;
                        objResult.PostCode = model.PostCode;
                        objResult.TakenBy = model.TakenBy;
                        objResult.TakenFor = model.TakenFor;
                        objResult.NextContactDate = model.NextContactDate;
                        objResult.ISCompleted = model.ISCompleted;
                        objResult.Associated = (int)GlobalEnums.AssociatedCallLog.CateringDetail;

                        var callLog = await _callLogs.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (callLog != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.CallLogUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update CallLog");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update CallLog");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetCallLogList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _callLogs.GetCallLogsListForCatereringDetail(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetCallLogsList");
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
        public async Task<IActionResult> RemoveCallLog(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var questionObj = _callLogs.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _callLogs.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.CallLogsDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveCallLog");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ManageCallLogIsCompleted(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var objResult = _callLogs.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    objResult.ISCompleted = !objResult.ISCompleted;
                    var tempView = new CallLogsDto();
                    var calllog = await _callLogs.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));

                    if (calllog != null)
                    {
                        txscope.Complete();
                        tempView = Mapper.Map<CallLogsDto>(calllog);
                        return View(@"Components/_CloseCallLogReminder", tempView);
                        //return JsonResponse.GenerateJsonResult(1, $@"Call log {(objResult.ISCompleted ? "activated" : "deactivated")} successfully.");
                    }
                    else
                    {
                        txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "Error in Update Call Log");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Update Call Log");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion

        #region Methods for Caterer Menu

        [HttpGet]
        public async Task<IActionResult> _AddEditCatererMenu(long id, bool isView)
        {
            try
            {
                var catList = await _cateringDetails.CatmenuDtoList(HttpContext.Session.GetString("ConnectionString"), id);

                var tempView = new CatererMenuDto();

                if (id != 0)
                {
                    tempView.CatererId = id;
                    tempView.catererMenuItemLists = new List<CatererMenuItemList>();
                    var temp = catList.Menulist.Select(x => new CatererMenuItemList
                    {
                        Cost = x.Cost,
                        MenuId = x.MenuId,
                        MenuName = x.MenuName

                    }).ToList();
                    tempView.catererMenuItemLists.AddRange(temp);

                }

                ViewBag.CatererList = catList.CatererList.Select(x => new SelectListItem()
                {
                    Text = x.CatererName,
                    Value = x.CatererId.ToString()
                }).OrderBy(x => x.Text);

                ViewBag.MenuList = catList.AllMenuList.Select(x => new SelectListItem()
                {
                    Text = x.MenuName,
                    Value = x.MenuId.ToString()
                }).OrderBy(x => x.Text);

                if (id == 0) return View(@"Components/_AddEditCatererMenu", new CatererMenuDto { Id = id, IsView = isView });

                var objResult = _cateringDetails.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                tempView.Id = objResult.Id;
                tempView.IsView = isView;

                return View(@"Components/_AddEditCatererMenu", tempView);
            }
            catch (Exception ex)
            {

                return View(@"Components/_AddEditCatererMenu", new CatererMenuDto { Id = id, IsView = isView });
            }

        }


        [HttpPost]
        public async Task<IActionResult> AddEditCatererMenu(CatererModelList model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    //delete all menu first
                    var isDeleted = _catMenu.DeleteAllMenu(HttpContext.Session.GetString("ConnectionString"), model.CatererId);
                    if (isDeleted)
                    {
                        if (model.CatMenuList != null)
                        {
                            foreach (var item in model.CatMenuList)
                            {
                                var obj = new CatererMenu();
                                obj.CatererId = model.CatererId;
                                obj.MenuId = item.MenuId;
                                obj.Cost = item.Cost;
                                var insertResult = await _catMenu.InsertAsync(HttpContext.Session.GetString("ConnectionString"), obj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                                if (insertResult == null)
                                {
                                    txscope.Dispose();
                                    ErrorLog.AddErrorLog(null, "Post-InsertionError");
                                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                                }
                            }
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.CaterMenuadded);
                        }
                        else
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.CaterMenuUpdated);
                        }
                    }
                    else
                    {
                        txscope.Dispose();
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-AddEditCatererMenu");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpPost]
        public IActionResult DeleteCatererMenu(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var isDeleted = _catMenu.DeleteAllMenu(HttpContext.Session.GetString("ConnectionString"), id);
                    if (isDeleted)
                    {
                        txscope.Complete();
                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.CatererMenuDeleted);
                    }
                    else
                    {
                        txscope.Dispose();
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-DeleteCatererMenu");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCatMenuList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _catMenu.GetCatMenuList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
                var model = new List<CatererModelList>();
                var test = allList.GroupBy(x => x.CatererId).Select(s => new
                {
                    catList = s.ToList()
                }).ToList();



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
                ErrorLog.AddErrorLog(ex, "GetCatMenuList");
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
        public IActionResult GetMenu(long CatererId)
        {
            var menuList = _catMenu.MenuListbyCatererId(HttpContext.Session.GetString("ConnectionString"), CatererId);
            return JsonResponse.GenerateJsonResult(1, "", menuList);

        }

        [HttpPost]
        public async Task<IActionResult> RemoveCatererMenu(long MenuId, long CatererId)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var catMenuObj = _catMenu.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.CatererId == CatererId && x.MenuId == MenuId);

                    var MenuObj = await _catMenu.DeleteAsync(HttpContext.Session.GetString("ConnectionString"), catMenuObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (MenuObj != null)
                    {
                        txscope.Complete();
                        return JsonResponse.GenerateJsonResult(1, "");
                    }
                    else
                    {
                        txscope.Dispose();
                        return JsonResponse.GenerateJsonResult(0, "");
                    }

                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveCatererMenu");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }



        #endregion

        #region Common
        public bool CheckCatererName(string CatererName, long Id)
        {
            bool isExist;
            var result = _cateringDetails.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                x => x.CatererName.ToLower().Equals(CatererName.ToLower()) && x.IsDelete == false && x.BranchId == Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            if (result != null)
            {

                isExist = result.CatererName.ToLower().Trim().Equals(CatererName.ToLower().Trim()) ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = _cateringDetails.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                        x => x.CatererName.ToLower().Equals(CatererName.ToLower()) && x.Id == Id && x.IsDelete == false);
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

        public bool CheckDescriptionofFood(string DescriptionOFFood, long Id)
        {
            bool isExist;
            var result = _menu.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                x => x.DescriptionOFFood.ToLower().Equals(DescriptionOFFood.ToLower()) && x.IsDelete == false && x.BranchId == Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            if (result != null)
            {

                isExist = result.DescriptionOFFood.ToLower().Trim().Equals(DescriptionOFFood.ToLower().Trim()) ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = _menu.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                        x => x.DescriptionOFFood.ToLower().Equals(DescriptionOFFood.ToLower()) && x.Id == Id && x.IsDelete == false);
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

        public bool CheckCatererEmail(string Email, long Id)
        {
            bool isExist;
            var result = _cateringDetails.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                x => x.Email.ToLower().Equals(Email.ToLower()) && x.IsDelete == false && x.BranchId == Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            if (result != null)
            {

                isExist = result.Email.ToLower().Trim().Equals(Email.ToLower().Trim()) ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = _cateringDetails.GetSingle(HttpContext.Session.GetString("ConnectionString"),
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