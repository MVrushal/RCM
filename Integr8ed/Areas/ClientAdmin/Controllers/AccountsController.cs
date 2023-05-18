using Integr8ed.Service.Dto;
using Integr8ed.Utility.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Collections.Generic;
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
using System.ComponentModel.DataAnnotations;
using Integr8ed.Service.Utility;
using Microsoft.AspNetCore.Hosting;
using System.Security.Principal;
using Microsoft.AspNetCore.Identity;
using Integr8ed.Data;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{

    [Area("ClientAdmin")]
    public class AccountsController : BaseController<AccountsController>
    {
        private readonly IDatabaseService database;
        private IConfiguration _config;
        private readonly IProfileServices _profile;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IErrorLogService _errorLog;
        // private readonly IConfigurationRoot _configurationRoot;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompnayService _compnay;
        private readonly IUserService _user;
        private readonly IClientAdminLogin _ClientAdminLogin;
        public AccountsController(IDatabaseService _database, IConfiguration config,
           IProfileServices profile
            , EmailService emailService, IWebHostEnvironment webHostEnvironment,
            IErrorLogService errorLog, UserManager<ApplicationUser> userManager, ICompnayService compnay, IUserService user,
                  IClientAdminLogin clientAdminLogin
            )//, IConfigurationRoot configurationRoot)
        {
            database = _database;
            _config = config;
            _profile = profile;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _errorLog = errorLog;
            _userManager = userManager;
            _compnay = compnay;
            _user = user;
            _ClientAdminLogin = clientAdminLogin;
            // _configurationRoot = configurationRoot;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody]UserDto login)
        {
            IActionResult response = Unauthorized();
            var user = AuthenticateUser(login);

            if (user != null)
            {
                var tokenString = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private UserDto AuthenticateUser(UserDto login)
        {
            UserDto user = null;

            //Validate the User Credentials    
            //Demo Purpose, I have Passed HardCoded User Information    
            if (login.UserName == "Jignesh")
            {
                user = new UserDto { UserName = "Jignesh Trivedi", Email = "test.btest@gmail.com" };
            }
            return user;
        }

        private string GenerateJSONWebToken(UserDto userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                //new Claim(JwtRegisteredClaimNames.valid, "1"),
        new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static bool ValidateToken(string authToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetValidationParameters();

                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = "Test.com",
                ValidAudience = "Test.com",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisismySecretKey")),
                RequireSignedTokens = true,
                RequireExpirationTime = true
            };
        }

        [HttpGet]
        public IActionResult _ForgotPassword()
        {
            return View(@"Components/_ForgotPassword");
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(UserDto model)
        {
            try
            {
                if (model.CompanyCode == "00000")
                {
                    var resetUrl = _config["CommonProperty:PhysicalUrl"] + "/ClientAdmin/Accounts/ResetPassword?companyCode=" + model.CompanyCode + "&resetPassURL=" + "sds";
                    string emailTemplate = CommonMethod.ReadEmailTemplate(_errorLog, _webHostEnvironment.WebRootPath, "ResetPassword.html", resetUrl);
                    emailTemplate = emailTemplate.Replace("{UserName}", "sdfdf" + "  " + "dsf");
                    bool data = await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                    {
                        ToDisplayName = "Admin",
                        ToAddress = model.Email,
                        Subject = "Reset Password",
                        BodyText = emailTemplate
                    });
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
                else
                {
                    var connectionstring = database.GetConnectionStringByCompanyCode(model.CompanyCode);
                    var token = GenerateJSONWebToken(model);
                    UserDto UD = new UserDto();
                    UD.Email = Input.Email;
                    // UD.Password = "K@bmef_11";
                    //var objResult1 = _ClientAdminLogin.Login(UD, connectionstring);

                    var objResult = _ClientAdminLogin.ForgotPassword(UD, connectionstring);
                    if (objResult != null)
                    {
                        UD.Frgt_Code = token;
                        UD.ID = objResult[0].Id;

                        var user = _ClientAdminLogin.UpdatedUserDetail(UD, connectionstring);
                        var resetUrl = _config["CommonProperty:PhysicalUrl"] + "/ClientAdmin/Accounts/ResetPassword?companyCode=" + model.CompanyCode + "&resetPassURL=" + token;
                        string emailTemplate = CommonMethod.ReadEmailTemplate(_errorLog, _webHostEnvironment.WebRootPath, "ResetPassword.html", resetUrl);
                        emailTemplate = emailTemplate.Replace("{UserName}", objResult[0].FirstName + "  " + objResult[0].LastName);
                        await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
                        {
                            ToDisplayName = objResult[0].FirstName + "  " + objResult[0].LastName,
                            ToAddress = objResult[0].Email,
                            Subject = "Reset Password",
                            BodyText = emailTemplate
                        });
                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.CheckMailForPasswordReset);
                    }
                    else
                    {
                        ErrorLog.AddErrorLog(null, "ForgotPassword/Post");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.AddErrorLog(null, "ForgotPassword/Post");
                return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
            }
        }








        //[HttpPost, ValidateAntiForgeryToken]
        //public async Task<IActionResult> ForgotPassword(UserDto model)
        //{
        //    using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //    {
        //        try
        //        {
        //            if (HttpContext.Session.GetString("ConnectionString") == null)
        //            {
        //                var connectionstring = database.GetConnectionStringByCompanyCode(model.CompanyCode);
        //                var token = GenerateJSONWebToken(model);
        //                UserDto UD = new UserDto();
        //                UD.UserName = Input.Email;
        //                UD.Password = "K@bmef_11";


        //                var Login = _ClientAdminLogin.Login(UD, connectionstring);

        //                var objResult = _profile.GetSingle(connectionstring, x => x.Email == model.Email);
        //                objResult.Frgt_Code = token;


        //                if (user != null)
        //                {
        //                    var resetUrl = _config["CommonProperty:PhysicalUrl"] + "/ClientAdmin/Accounts/ResetPassword?companyCode=" + model.CompanyCode + "&resetPassURL=" + token;
        //                    string emailTemplate = CommonMethod.ReadEmailTemplate(_errorLog, _webHostEnvironment.WebRootPath, "ResetPassword.html", resetUrl);
        //                    emailTemplate = emailTemplate.Replace("{UserName}", user.FirstName + "  " + user.LastName);
        //                    await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
        //                    {
        //                        ToDisplayName = user.FirstName + "  " + user.LastName,
        //                        ToAddress = user.Email,
        //                        Subject = "Reset Password",
        //                        BodyText = emailTemplate
        //                    });
        //                    txscope.Complete();
        //                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.CheckMailForPasswordReset);
        //                }
        //                else
        //                {
        //                    txscope.Dispose();
        //                    ErrorLog.AddErrorLog(null, "ForgotPassword/Post");
        //                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.UserNotExist);
        //                }
        //            }
        //            else
        //            {
        //                var token = GenerateJSONWebToken(model);
        //                var objResult = _profile.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Email == model.Email);
        //                objResult.Frgt_Code = token;
        //                var user = await _profile.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
        //                if (user != null)
        //                {
        //                    var resetUrl = _config["CommonProperty:PhysicalUrl"] + "/ClientAdmin/Accounts/ResetPassword?companyCode=" + model.CompanyCode + "&resetPassURL=" + token;
        //                    string emailTemplate = CommonMethod.ReadEmailTemplate(_errorLog, _webHostEnvironment.WebRootPath, "ResetPassword.html", resetUrl);
        //                    emailTemplate = emailTemplate.Replace("{UserName}", user.FirstName + "  " + user.LastName);
        //                    await _emailService.SendEmailAsyncByGmail(new SendEmailModel()
        //                    {
        //                        ToDisplayName = user.FirstName + "  " + user.LastName,
        //                        ToAddress = user.Email,
        //                        Subject = "Reset Password",
        //                        BodyText = emailTemplate
        //                    });
        //                    txscope.Complete();
        //                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.CheckMailForPasswordReset);
        //                }
        //                else
        //                {
        //                    txscope.Dispose();
        //                    ErrorLog.AddErrorLog(null, "ForgotPassword/Post");
        //                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            txscope.Dispose();
        //            ErrorLog.AddErrorLog(null, "ForgotPassword/Post");
        //            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
        //        }
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string companyCode, string resetPassURL)
        {
            var resetUrl = _config["CommonProperty:PhysicalUrl"] + "/ClientAdmin/Accounts/ResetPassword";
            resetUrl = resetPassURL.Replace(resetUrl, "");
            var isValidToken = ValidateToken(resetUrl);
            if (isValidToken)
            {
                if (HttpContext.Session.GetString("ConnectionString") == null || HttpContext.Session.GetString("ConnectionString") == "")
                {
                    var connectionstring = database.GetConnectionStringByCompanyCode(companyCode);
                    if (connectionstring != null && connectionstring != "")
                    {
                        var objResult = _profile.GetSingle(connectionstring, x => x.Frgt_Code == resetUrl);
                        if(objResult != null) {
                            objResult.Frgt_Code = null;
                            var profile = await _profile.UpdateAsync(connectionstring, objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                            return View(new UserDto() { Email = objResult.Email, CompanyCode = companyCode });
                        }
                        return JsonResponse.GenerateJsonResult(0, "Invalid Reset Password link");

                    }
                    return JsonResponse.GenerateJsonResult(0, "Invalid Reset Password link");
                }
                else
                {
                    var objResult = _profile.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Frgt_Code == resetUrl);
                    return View(new UserDto() { Email = objResult.Email, CompanyCode = companyCode });

                }
            }
            return JsonResponse.GenerateJsonResult(0, "Invalid Reset Password link");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(UserDto model)
        {
            //using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //{
            try
            {
                if (HttpContext.Session.GetString("ConnectionString") == null || HttpContext.Session.GetString("ConnectionString") == "")
                {
                    var connectionstring = database.GetConnectionStringByCompanyCode(model.CompanyCode);
                    if (connectionstring != null && connectionstring != "")
                    {
                        var objResult = _profile.GetSingle(connectionstring, x => x.Email == model.Email && !x.IsDelete);
                        objResult.Password = model.Password;
                        var user = await _profile.UpdateAsync(connectionstring, objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (user != null)
                        {
                            return JsonResponse.GenerateJsonResult(1, "Your password changed successfully...!! Please click here for login " + _config["CommonProperty:PhysicalUrl"]);
                        }
                    }
                    //  txscope.Complete();
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
                else
                {
                    //  txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Update User");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
            catch (Exception e)
            {
                //  txscope.Dispose();
                ErrorLog.AddErrorLog(null, "Error in Update User");
                return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
            }
            // }
        }


        [HttpGet]
        public IActionResult _ResetOldPassToNewPass()
        {
            return View(@"Components/_ResetOldPassToNewPass");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetOldPassToNewPass(UserDto model)
        {

            try
            {
                var objResult = _profile.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Password == model.Password &&
                x.Id == Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                if (objResult == null)
                {
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.InvalidPassword);
                }
                objResult.Password = model.NewPassword;
                var user = await _profile.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                if (user != null)
                {

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.PasswordChanged);
                }
                else
                {

                    ErrorLog.AddErrorLog(null, "Error in Update User");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
            catch (Exception e)
            {

                ErrorLog.AddErrorLog(null, "Error in Update User");
                return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
            }

        }

        [HttpGet]
        public IActionResult _ForgotPasswordConfirmation()
        {
            return View(@"Components/_ForgotPasswordConfirmation");
        }

        #region  Common
        [HttpGet]
        public bool CheckUserPassword(string Password)
        {
            try
            {

                var result = _profile.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Password.ToLower().Equals(Password.ToLower()) &&
                x.Id == Convert.ToInt32(HttpContext.Session.GetString("UserID")));

                return result == null ? false : true;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        [HttpGet]
        public async Task<bool> CheckUserEmail(string Email)
        {
            try
            {
                bool isExist;
                var result = await _userManager.FindByEmailAsync(Email);
                if (result != null)
                {
                    isExist = result.Email.ToLower().Trim().Equals(Email.ToLower().Trim()) && result.IsActive ? true : false;
                    return isExist;
                }
                else
                {
                    return result == null ? false : true;
                }
            }
            catch (Exception e)
            {

                return false;
            }
        }


        [HttpGet]
        public bool CheckUserCompanyCode(string CompanyCode)
        {
            if (CompanyCode == "00000")
            {
                return true;
            }
            else
            {
                bool isExist;
                var result = _compnay.GetSingle(x => x.CompanyCode.ToLower().Equals(CompanyCode.ToLower()) && x.IsDelete == false);
                if (result != null)
                {

                    isExist = result.CompanyCode.ToLower().Trim().Equals(CompanyCode.ToLower().Trim()) ? true : false;
                    return isExist;
                }
                else
                {
                    return result == null ? false : true;
                }
            }
        }

        [HttpGet]
        public async Task<bool> CheckUserEmailAndCompanyCode(string Email, string CompanyCode)
        {
            try
            {
                bool isExist;
                if (CompanyCode == "00000")
                {


                    var result = await _userManager.FindByEmailAsync(Email);
                    if (result != null)
                    {

                        return result == null ? false : true;
                    }
                    else
                    {
                        return result == null ? false : true;
                    }
                }
                else
                {
                    var companyResult = _compnay.GetSingle(x => x.CompanyCode.ToLower().Equals(CompanyCode.ToLower()) && x.IsDelete == false);
                    if (companyResult != null)
                    {
                        var connectionstring = database.GetConnectionStringByCompanyCode(CompanyCode);
                        UserDto UD = new UserDto();
                        UD.Email = Email;
                        var objResult = _ClientAdminLogin.ForgotPassword(UD, connectionstring);
                        if (objResult.Count > 0)
                        {
                            isExist = true;
                        }

                        return objResult.Count <= 0 ? false : true;
                    }
                    else
                    {
                        return false;

                    }
                }
            }
            catch (Exception e)
            {

                return false;
            }
        }

        #endregion
    }
}
