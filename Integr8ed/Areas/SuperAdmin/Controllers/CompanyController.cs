using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Integr8ed.Service.Dto;
using Integr8ed.Models;
using Integr8ed.Utility.Common;
using Integr8ed.Utility.JqueryDataTable;
using Integr8ed.Service.Interface;
using System.Transactions;
using Integr8ed.Data.DbModel.SuperAdmin;
using Integr8ed.Service;
using Integr8ed.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Integr8ed.Data.DbModel.ClientAdmin;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using Integr8ed.Service.Utility;
using Microsoft.AspNetCore.Hosting;
using Integr8ed.Service.Interface.ClientAdmin;

namespace Integr8ed.Areas.SuperAdmin
{

    [Area("SuperAdmin"), Authorize]
    public class CompanyController : BaseController<CompanyController>
    {
        #region Fields
        private readonly ICompnayService _company;
        private readonly IDatabaseService _database;
        private readonly ISuparAdminUserService _user;
        private readonly UserManager<ApplicationUser> _userManager;
        private IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IErrorLogService _errorLog;
        private readonly IBranchService _branchService;

        private readonly IUserService _userService;

        #endregion

        #region Ctor
        public CompanyController(UserManager<ApplicationUser> userManager, ICompnayService company, ISuparAdminUserService user, IDatabaseService database
            , IUserService userService, IConfiguration config, EmailService emailService, IWebHostEnvironment webHostEnvironment,
            IErrorLogService errorLog, IBranchService branch
            )
        {
            _userManager = userManager;
            _company = company;
            _user = user;
            _userService = userService;
            _database = database;
            _config = config;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _errorLog = errorLog;
            _branchService = branch;
        }
        #endregion

        #region Methods
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                var allList = await _company.GetCompanyList(parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetCompanyList");
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
        public IActionResult _AddEditCompany(long id, bool IsView)
        {
            if (id == 0) return View(@"Components/_AddEditCompany", new CompanyDto { Id = id, IsView = IsView });
            var tempView = new CompanyDto();
            var compnayObj = _company.GetSingle(x => x.UserId == id);
            var appuser = _user.GetSingle(x => x.Id == id);

            tempView.Id = appuser.Id;
            tempView.FirstName = appuser.FirstName;
            tempView.SurName = appuser.LastName;
            tempView.Email = appuser.Email;
            tempView.Telephone = appuser.MobileNumber;
            tempView.PostCode = compnayObj.PostCode;
            tempView.Address = compnayObj.Address;
            tempView.Email = appuser.Email;
            tempView.OrganisationName = compnayObj.OrganisationName;
            tempView.IsView = IsView;

            return View(@"Components/_AddEditCompany", tempView);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditCompany(CompanyDto model)
        {
            // using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //  {
            try
            {
                var appUser = _userManager.FindByIdAsync(model.Id.ToString()).Result;
                if (appUser != null)
                {


                    appUser.FirstName = model.FirstName;
                    appUser.LastName = model.SurName;
                    appUser.MobileNumber = model.Telephone;
                    appUser.AddressLine1 = model.Address;
                    appUser.AddressLine2 = model.Address;
                    var updateResult = await _userManager.UpdateAsync(appUser);
                    if (updateResult != null)
                    {

                        var company = _company.GetSingle(x => x.UserId == appUser.Id);
                        company.OrganisationName = model.OrganisationName;
                        company.Address = model.Address;
                        var companyResult = await _company.UpdateAsync(company, Accessor, User.GetUserId());

                        ///txscope.Complete();
                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.UpdateClientAdmin);
                    }
                    else
                    {
                        // txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "Error in Update clientadmin");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }

                }
                else
                {

                    #region Create ClientAdmin

                    ApplicationUser user = new ApplicationUser();
                    user.MobileNumber = model.Telephone;
                    user.FirstName = model.FirstName;
                    user.LastName = model.SurName;
                    user.UserName = model.Email;
                    user.Email = model.Email;

                    user.IsActive = true;
                    var userResult = await _userManager.CreateAsync(user, model.Password);

                    if (userResult.Succeeded)
                    {
                        var comObj = new Company();
                        comObj.OrganisationName = model.OrganisationName;
                        comObj.UserId = user.Id;
                        comObj.IsActive = true;
                        comObj.PostCode = model.PostCode;
                        comObj.Telephone = model.Telephone;
                        comObj.Address = model.Address;
                        comObj.CompanyCode = GanerateCode();
                        var staffResult = await _company.InsertAsync(comObj, Accessor, User.GetUserId());




                        #region  CreatedynamicDB
                        DatabaseParamDto DBParam = new DatabaseParamDto();
                        string DatabaseName = GetConfigValue("DatabaseName");
                        DBParam.ServerName = GetConfigValue("ServerName");
                        DBParam.DatabaseName = DatabaseName + comObj.CompanyCode;
                        DBParam.DataFileName = DatabaseName + comObj.CompanyCode;
                        DBParam.DataPathName = GetConfigValue("DataPathName") + "\\" + DatabaseName + comObj.CompanyCode + ".mdf";
                        DBParam.DataFileGrowth = "1024KB";
                        DBParam.LogFileName = DatabaseName + comObj.CompanyCode + "_log";
                        DBParam.LogPathName = GetConfigValue("DataPathName") + "\\" + DatabaseName + comObj.CompanyCode + "_log.mdf";
                        DBParam.LogFileGrowth = "10%";
                        DBParam.UserID = GetConfigValue("USERID");
                        DBParam.Password = GetConfigValue("Password");


                        var databaseobj = new Database();

                        string ComanyDBUserID = DatabaseName;
                        string ComanyDBPAssword = DatabaseName;

                        databaseobj.ServerAddress = DBParam.ServerName;
                        databaseobj.DatabaseName = DatabaseName + comObj.CompanyCode;
                        databaseobj.ComanyCode = comObj.CompanyCode;
                        databaseobj.IsActive = true;
                        databaseobj.IsDelete = false;
                        databaseobj.Password = DBParam.Password;
                        databaseobj.UserName = DBParam.DatabaseName;
                        databaseobj.Dbconnectionstring = "SERVER = " + DBParam.ServerName +
                                 "; DATABASE = " + DBParam.DatabaseName + "; User ID=" + DBParam.DatabaseName + ";Password= Password123#";// + DBParam.Password;

                        var DbResult = await _database.InsertAsync(databaseobj, Accessor, User.GetUserId());
                        var allList = _database.CreateDBComapnyWise(DBParam, GetConfigValue("ConnectionStrings:Integr8edContext"));

                        #endregion


                        #region Added recored client admin DB

                        #region Branch_creation

                        BranchMaster branchObj = new BranchMaster();
                        branchObj.BranchName = "Main";
                        branchObj.IsActive = true;
                        var branchResult = await _branchService.InsertAsync(databaseobj.Dbconnectionstring, branchObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (branchResult != null)
                        {
                            Users ud = new Users();
                            ud.TelephoneNumber = model.Telephone;
                            ud.FirstName = model.FirstName;
                            ud.LastName = model.SurName;
                            ud.Email = model.Email;
                            ud.UserName = model.Email;
                            ud.Password = model.Password;
                            ud.MobileNumber = model.Telephone;
                            ud.AddressLine1 = model.Address;
                            ud.BranchId = branchResult.Id;
                            ud.IsAdmin = true;

                        var userResult1 = await _userService.InsertAsync(databaseobj.Dbconnectionstring, ud, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                            if (userResult1 != null)
                            {

                                #region Create and Assign roles
                                
                                _userService.AddRoles(databaseobj.Dbconnectionstring, userResult1.Id);

                                #endregion


                                #region sent mail
                                var resetUrl = _config["CommonProperty:PhysicalUrl"];
                                string emailTemplate = CommonMethod.ReadEmailTemplate(_errorLog, _webHostEnvironment.WebRootPath, "CreateCompany.html", resetUrl);
                                emailTemplate = emailTemplate.Replace("{UserName}", user.FirstName + "  " + user.LastName);
                                emailTemplate = emailTemplate.Replace("{LoginUserName}", user.Email);
                                emailTemplate = emailTemplate.Replace("{CompanyCode}", comObj.CompanyCode);
                                emailTemplate = emailTemplate.Replace("{Password}", model.Password);
                                await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                                {
                                    ToDisplayName = user.FirstName + "  " + user.LastName,
                                    ToAddress = user.Email,
                                    Subject = "Company Created",
                                    BodyText = emailTemplate
                                });

                                #endregion
                                 return JsonResponse.GenerateJsonResult(1, ResponseConstants.CreateCompany);
                            }
                            else {
                                 return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                            }
                        #endregion

                        }
                        else
                                return JsonResponse.GenerateJsonResult(0, ResponseConstants.CreateBranchFailed);
                        #endregion


                        #region Create StoreProcedures

                        //var Parameters = new List<SqlParameter>
                        //     {
                        //            new SqlParameter("@dbName", SqlDbType.VarChar) { Value = DatabaseName + comObj.CompanyCode }

                        //     };
                        //var spResult=await _company.CreateStoreProcedures(Parameters.ToArray());
                        #endregion    

                    }
                    else
                    {
                        //  txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "Error in Create clientadmin");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }

                    #endregion
                }

            }
            catch (Exception ex)
            {
                // txscope.Dispose();
                ErrorLog.AddErrorLog(ex, "AddorRemoveclientadmin");
                return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
            }
            // }
        }


        [HttpPost]
        public IActionResult RemoveClientAdmin(long[] id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    foreach (var item in id)
                    {
                        var user = _user.GetSingle(x => x.Id == item);
                        if (user.Id != 0)
                        {
                            user.IsActive = false;
                            _user.Delete(user);
                            //if (userResult != null)
                            //{
                            //    var clientAdmin = _company.GetSingle(x => x.UserId == userResult.Id);
                            //    clientAdmin.IsDelete = true;
                            //    var clientadminResult = await _company.UpdateAsync(clientAdmin, Accessor, User.GetUserId());
                            //    if (clientadminResult != null)
                            //    {

                            //    }
                            //    else
                            //    {
                            //        txscope.Dispose();
                            //        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.DeleteClientAdmin);
                            //}
                            //else
                            //{

                            //}

                        }
                        else
                        {
                            txscope.Dispose();
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.invalidClientAdmin);
                        }
                    }

                    txscope.Dispose();
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);

                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemovLesson");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        public IActionResult ViewCompanyLocation()
        {
            return View();
        }

        [HttpGet]
        public List<string> GetCompanyAddress()
        {
            var address = _company.GetAll().Select(x => x.Address).ToList();
            return address;
        }


        #endregion

        #region Common

        public string GanerateCode()
        {
            var code = DateTime.Now.Ticks.ToString();
            code = code.Substring(code.Length - 5);
            var existingCodeList = _company.GetAll().Select(x => x.CompanyCode).ToList();
            if (!existingCodeList.Contains(code))
                return code;
            else
                return GanerateCode();

        }


        public async Task<bool> CheckForEmail(string Email, long Id)
        {
            bool isExist;
            var result = await _userManager.FindByEmailAsync(Email);
            if (result != null)
            {

                isExist = result.Email.ToLower().Trim().Equals(Email.ToLower().Trim()) && result.IsActive ? true : false;
                return isExist ? false : true;

            }
            else
            {
                return result == null ? true : false;
            }
        }


        #endregion

    }
}