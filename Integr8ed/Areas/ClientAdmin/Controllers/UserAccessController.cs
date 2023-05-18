using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface.ClientAdmin;
using Microsoft.AspNetCore.Http;
using Integr8ed.Utility.Common;
using Integr8ed.Models;
using Integr8ed.Utility.JqueryDataTable;
using System.Transactions;
using Integr8ed.Service;
using Integr8ed.Service.Enums;
using Microsoft.Extensions.Configuration;
using Integr8ed.Service.Interface;
using Integr8ed.Service.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class UserAccessController : BaseController<UserAccessController>
    {
        #region Fields
        private readonly IUserAccessService _userAccess;
        private readonly ICompanyUserService _companyUser;
        private readonly IConfiguration _config;
        private readonly IErrorLogService _errorLog;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IBranchService _branchService;

        #endregion

        #region ctor
        public UserAccessController(IUserAccessService userAccess, ICompanyUserService companyUser, IConfiguration config, IErrorLogService errorLog,
            EmailService emailService, IWebHostEnvironment webHostEnvironment, IBranchService branchService)
        {
            _userAccess = userAccess;
            _companyUser = companyUser;
            _config = config;
            _errorLog = errorLog;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _branchService = branchService;
        }
        #endregion


        #region Method
        public IActionResult Index()
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);

            if (IsAccess("UserAccess"))
                return View();
            else
                return RedirectToAction("Index", "Dashboard");

        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(UserDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.ID != 0)
                    {
                        var UserObj = _companyUser.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.ID);
                        if (model.IsEmailEdit == true)
                        {
                            UserObj.Email = model.Email;
                            UserObj.UserName = model.Email;
                        }
                        else
                        {
                            UserObj.FirstName = model.FirstName;
                            UserObj.LastName = model.LastName;

                            UserObj.AddressLine1 = model.AddressLine1;
                            UserObj.MobileNumber = model.MobileNumber;
                            UserObj.IsActive = true;
                            UserObj.IsAdmin = model.IsAdmin;
                            if (model.IsAdmin)
                            {
                                UserObj.AdminBranchId = model.AdminBranchId;
                            }
                        }

                        var UpdateResult = await _companyUser.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), UserObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (UpdateResult != null)
                        {
                            txscope.Complete();
                            if (model.IsEmailEdit == true)
                            {
                                var resetUrl = _config["CommonProperty:PhysicalUrl"];
                                string emailTemplate = CommonMethod.ReadEmailTemplate(_errorLog, _webHostEnvironment.WebRootPath, "ChangeEmail.html", resetUrl);
                                emailTemplate = emailTemplate.Replace("{UserName}", UpdateResult.FirstName + "  " + UpdateResult.LastName);
                                await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                                {
                                    ToDisplayName = UpdateResult.FirstName + "  " + UpdateResult.LastName,
                                    ToAddress = UpdateResult.Email,
                                    Subject = "Change Email",
                                    BodyText = emailTemplate
                                });
                                return JsonResponse.GenerateJsonResult(1, "User Email Updated Successfully");
                            }
                            return JsonResponse.GenerateJsonResult(1, "User Updated Successfully");
                        }
                    }

                    var companyUser = new Users();
                    companyUser.FirstName = model.FirstName;
                    companyUser.LastName = model.LastName;
                    companyUser.Email = model.Email;
                    companyUser.UserName = model.Email;
                    companyUser.Password = model.Password;
                    companyUser.AddressLine1 = model.AddressLine1;
                    companyUser.AddressLine2 = model.AddressLine2;
                    companyUser.MobileNumber = model.MobileNumber;
                    companyUser.IsActive = true;
                    companyUser.IsAdmin = model.IsAdmin;
                    companyUser.BranchId =Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
                    if (model.IsAdmin)
                    {
                        companyUser.AdminBranchId = model.AdminBranchId;
                    }
                    var insertResult = await _companyUser.InsertAsync(HttpContext.Session.GetString("ConnectionString"), companyUser, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (insertResult != null)
                    {
                        var resetUrl = _config["CommonProperty:PhysicalUrl"];
                        string emailTemplate = CommonMethod.ReadEmailTemplate(_errorLog, _webHostEnvironment.WebRootPath, "CreateUser.html", resetUrl);
                        emailTemplate = emailTemplate.Replace("{UserName}", insertResult.FirstName + "  " + insertResult.LastName);
                        emailTemplate = emailTemplate.Replace("{UserEmail}", insertResult.Email);
                        emailTemplate = emailTemplate.Replace("{UserPassword}", insertResult.Password);
                        emailTemplate = emailTemplate.Replace("{CompanyCode}", HttpContext.Session.GetString("CompanyCode").ToString());

                        await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                        {
                            ToDisplayName = insertResult.FirstName + "  " + insertResult.LastName,
                            ToAddress = insertResult.Email,
                            Subject = "Create User",
                            BodyText = emailTemplate
                        });
                        txscope.Complete();
                        return JsonResponse.GenerateJsonResult(1, "User Added Successfully");
                    }
                    else
                    {
                        txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "POST/AddEditEquipment");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }

                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(e, "POST/AddEditEquipment");
                    return JsonResponse.GenerateJsonResult(0, e.Message);

                }
            }
        }

        [HttpGet]
        public IActionResult _AddUser(long Id, bool IsView, bool IsEmailEdit)
        {
            if (Id == 0) return View(@"Components/_AddUser", new UserDto {
                ID = 0, 
                IsView = IsView,
                IsEmailEdit =IsEmailEdit,
                /*IsAdmin=false*/ });
            var UserObj = _companyUser.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == Id);
            UserDto tempView = new UserDto();
            tempView.ID = UserObj.Id;
            tempView.Email = UserObj.Email;
            tempView.FirstName = UserObj.FirstName;
            tempView.LastName = UserObj.LastName;
            tempView.Password = UserObj.Password;
            tempView.AddressLine1 = UserObj.AddressLine1;
            tempView.MobileNumber = UserObj.MobileNumber;
          //  tempView.IsAdmin = UserObj.IsAdmin??false;
            tempView.IsView = IsView;
            tempView.IsEmailEdit = IsEmailEdit;
            return View(@"Components/_AddUser", tempView);
        }

        [HttpGet]
        public IActionResult _AddAccess(long id)
        {
            var branchList = _branchService.getBranchList(HttpContext.Session.GetString("ConnectionString"),"");
            ViewBag.BranchList = branchList.Select(x => new SelectListItem()
            {
                Text = x.BranchName,
                Value = x.Id.ToString()
            }).OrderBy(x => x.Text);


            if (id == 0) return View(@"Components/_AddAccess", new UserAccessDto());
            var UserAccessList = _userAccess.GetUserList(HttpContext.Session.GetString("ConnectionString"), id);

            if (UserAccessList.UserId != 0)
            {

                return View(@"Components/_AddAccess", UserAccessList);
            }


            return View(@"Components/_AddAccess", new UserAccessDto { UserId = id });
        }


        [HttpPost]
        public async Task<IActionResult> RemoveUser(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var questionObj = _companyUser.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    var Updateresult = await _companyUser.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (Updateresult != null)
                    {
                        txscope.Complete();
                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.UserDeleted);
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
                    ErrorLog.AddErrorLog(ex, "Post-RemoveEquipment");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }




        [HttpPost]
        public async Task<IActionResult> AddAccess(UserAccessDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var users = _companyUser.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.UserId);
                    
                    await _companyUser.MakeBranchAdmin(HttpContext.Session.GetString("ConnectionString"),model.UserId, model.BranchAdmin,model.BranchListID);
                    
                      //  await _companyUser.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), users, Accessor, User.GetUserId());
                    
                    _userAccess.DeleteAllAccess(HttpContext.Session.GetString("ConnectionString"), model.UserId);
                    if (model.RoomType)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.RoomType;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.EquipmentRequirment)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.EquipmentRequirement;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.UserGroup)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.UserGroup;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.MeetingType)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.MeetingType;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.DelegetsCodes)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.DelegetsCodes;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.EntryType)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.EntryType;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.InvoiceItem)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.InvoiceItem;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.CateringDetail)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.CateringDetail;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.RoomAvailability)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.RoomAvailability;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.Internal_ExternalBooking)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.Internal_ExternalBooking;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.RecurringBooking)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.RecurringBooking;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.ManageBranch)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.ManageBranch;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.BookingDiary)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.BookingDiary;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }
                    if (model.Reports)
                    {
                        var UserAccess = new UserAccess();
                        UserAccess.UserId = model.UserId;
                        UserAccess.MenuId = (int)UserMenu.Access.Reports;
                        await _userAccess.InsertAsync(HttpContext.Session.GetString("ConnectionString"), UserAccess, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    }

                    txscope.Complete();
                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.UpdateUserAccess);

                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-AddAccess");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyUserList(JQueryDataTableParamModel param)
        {

            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _companyUser.GetCompanyUserList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
                var total = allList.FirstOrDefault()?.TotalRecords ?? 0;
                var newList = allList.Where(x => x.ID != 1).ToList();

                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = total,
                    aaData = newList
                });
            }
            catch (Exception ex)
            {
                ErrorLog.AddErrorLog(ex, "GetCompanyUserList");
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = ""
                });
            }
        }

        public bool CheckForEmail(string Email, long ID)
        {
            bool isExist;
            var result = _companyUser.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                x => x.Email.ToLower().Equals(Email.ToLower()) && x.IsDelete == false && x.BranchId == Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            if (result != null)
            {

                isExist = result.Email.ToLower().Trim().Equals(Email.ToLower().Trim()) ? true : false;
                if (isExist && ID != 0)
                {
                    var resultExist = _companyUser.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                        x => x.Email.ToLower().Equals(Email.ToLower()) && x.Id == ID && x.IsDelete == false);
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


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserEmail(UserDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var UserObj = _companyUser.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.ID);

                    UserObj.Email = model.Email;

                    var UpdateResult = await _companyUser.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), UserObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    if (UpdateResult != null)
                    {

                        txscope.Complete();
                        return JsonResponse.GenerateJsonResult(1, "User Email Updated Successfully");
                    }
                    else
                    {
                        txscope.Dispose();
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(e, "POST/AddEditEquipment");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);

                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> ManageIsAdmin(long id)
        {
            try
            {
                var users = _companyUser.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);

                users.IsAdmin = !users.IsAdmin;
                await _companyUser.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), users, Accessor, User.GetUserId());
                return JsonResponse.GenerateJsonResult(1, "User Updated");
            }
            catch (Exception ex)
            {
                ErrorLog.AddErrorLog(ex, "Post-ManageIsAdmin");
                return JsonResponse.GenerateJsonResult(0, "something went  wrong !");
            }
        }

        #endregion
    }
}