using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Integr8ed.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Integr8ed.Service.Interface;
using Integr8ed.Service.Dto;
using Integr8ed.Utility.Common;
using Integr8ed.Service.Enums;

namespace Integr8ed.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManagerMaster;
        private readonly UserManager<ApplicationUser> _userManager;
        //  private readonly SignInManager<ApplicationUserClientAdmin> _signInManagerCA;
        private readonly ILogger<LoginModel> _logger;
        private readonly IDatabaseService _database;
        private readonly IClientAdminLogin _ClientAdminLogin;
        public LoginModel(SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger,
            IDatabaseService database,
            IClientAdminLogin clientAdminLogin

            )
        {
            _userManager = userManager;
            _signInManagerMaster = signInManager;
            //  _signInManagerCA = signInManagerCA;
            _logger = logger;
            _database = database;
            _ClientAdminLogin = clientAdminLogin;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage ="Please enter a User Name")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Please enter a Password")]
            [DataType(DataType.Password)]

            public string Password { get; set; }

            [Required(ErrorMessage = "Please enter a Company Code")]

            public string ComanyCode { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManagerMaster.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            try
            {
                returnUrl = returnUrl ?? Url.Content("~/");

                if (ModelState.IsValid)
                {
                    try
                    {
                        #region  DynamicDbLogin

                       
                        UserDto UD = new UserDto();
                        UD.UserName = Input.Email.TrimEnd().TrimStart();
                        UD.Password = Input.Password.TrimEnd().TrimStart();
                        UD.CompanyCode = Input.ComanyCode.TrimEnd().TrimStart();
                        if (Input.ComanyCode != "00000")
                        {
                            var Dbdata = _database.GetConnectionStringByCompanyCode(Input.ComanyCode.ToString());

                            if (Dbdata == "")
                            {
                                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                return Page();
                            }
                            HttpContext.Session.SetString("ConnectionString", Dbdata.ToString());


                            //  UserDto ud = new UserDto()

                            var Login = _ClientAdminLogin.Login(UD, HttpContext.Session.GetString("ConnectionString"));
                            if (Login != null)
                            {

                                if (Input.RememberMe)
                                {
                                    CookieOptions cookieOptions = new CookieOptions();
                                    cookieOptions.Expires = DateTime.Now.AddDays(30);
                                    Response.Cookies.Append("UserName", Input.Email, cookieOptions);
                                    Response.Cookies.Append("Password", Input.Password, cookieOptions);
                                    Response.Cookies.Append("CompanyCode", Input.ComanyCode, cookieOptions);
                                    Response.Cookies.Append("Remember_Me", Input.RememberMe.ToString(), cookieOptions);
                                }

                                _logger.LogInformation("User logged in.");
                                HttpContext.Session.SetString("UserID", Login[0].ID.ToString());
                                HttpContext.Session.SetString("CompanyCode", Input.ComanyCode);
                                HttpContext.Session.SetString("Role", Login[0].Role.ToString());
                                HttpContext.Session.SetString("RoleId", Login[0].RoleId.ToString());
                                HttpContext.Session.SetString("BranchId", Login[0].BranchId.ToString());
                                HttpContext.Session.SetString("AdminBranchId", Login[0].AdminBranchId??"");
                                HttpContext.Session.SetString("CompanyCode", Input.ComanyCode ?? "");
                                HttpContext.Session.SetString("UserName", Login[0].UserName==null?"":Login[0].UserName.ToString());
                                HttpContext.Session.SetString("FullName", Login[0].FullName??"");
                                HttpContext.Session.SetString("CompanyAddress", Login[0].AddressLine1 ?? "");
                                HttpContext.Session.SetString("CompanyMobile", Login[0].MobileNumber ?? "");
                                HttpContext.Session.SetString("ClientAdminProfileImage", Login[0].UserImage??"");
                                //var CheckAdmin = await _userManager.FindByNameAsync(Input.Email);
                               
                                HttpContext.Session.SetInt32("IsAdmin", Login[0].IsAdmin?1:0);
                                HttpContext.Session.SetInt32("IsBranchAdmin", Login[0].RoleId==2?1:0);


                                //redirect to  external user panel
                                if (Login[0].RoleId == 4)
                                    return LocalRedirect("/User/External/index");



                                //AssignUserAccess

                                if (Login[0].MenuId.Contains((int)UserMenu.Access.RoomType))
                                    HttpContext.Session.SetInt32("RoomType",(int)UserMenu.Access.RoomType);
                                else
                                    HttpContext.Session.SetInt32("RoomType", (int)UserMenu.Access.Denied);


                                if (Login[0].MenuId.Contains((int)UserMenu.Access.EquipmentRequirement))
                                    HttpContext.Session.SetInt32("EquipmentRequirement", (int)UserMenu.Access.EquipmentRequirement);
                                else
                                    HttpContext.Session.SetInt32("EquipmentRequirement", (int)UserMenu.Access.Denied);

                                if (Login[0].MenuId.Contains((int)UserMenu.Access.DelegetsCodes))
                                    HttpContext.Session.SetInt32("DelegetsCodes", (int)UserMenu.Access.DelegetsCodes);
                                else
                                    HttpContext.Session.SetInt32("DelegetsCodes", (int)UserMenu.Access.Denied);


                                if (Login[0].MenuId.Contains((int)UserMenu.Access.MeetingType))
                                    HttpContext.Session.SetInt32("MeetingType", (int)UserMenu.Access.MeetingType);
                                else
                                    HttpContext.Session.SetInt32("MeetingType", (int)UserMenu.Access.Denied);

                                if (Login[0].MenuId.Contains((int)UserMenu.Access.InvoiceItem))
                                    HttpContext.Session.SetInt32("InvoiceItem", (int)UserMenu.Access.InvoiceItem);
                                else
                                    HttpContext.Session.SetInt32("InvoiceItem", (int)UserMenu.Access.Denied);


                                if (Login[0].MenuId.Contains((int)UserMenu.Access.EntryType))
                                    HttpContext.Session.SetInt32("EntryType", (int)UserMenu.Access.EntryType);
                                else
                                    HttpContext.Session.SetInt32("EntryType", (int)UserMenu.Access.Denied);

                                if (Login[0].MenuId.Contains((int)UserMenu.Access.UserGroup))
                                    HttpContext.Session.SetInt32("UserGroup", (int)UserMenu.Access.UserGroup);
                                else
                                    HttpContext.Session.SetInt32("UserGroup", (int)UserMenu.Access.Denied);
                                if (Login[0].MenuId.Contains((int)UserMenu.Access.CateringDetail))
                                    HttpContext.Session.SetInt32("CateringDetail", (int)UserMenu.Access.CateringDetail);
                                else
                                    HttpContext.Session.SetInt32("CateringDetail", (int)UserMenu.Access.Denied);
                                if (Login[0].MenuId.Contains((int)UserMenu.Access.RoomAvailability))
                                    HttpContext.Session.SetInt32("RoomAvailability", (int)UserMenu.Access.RoomAvailability);
                                else
                                    HttpContext.Session.SetInt32("RoomAvailability", (int)UserMenu.Access.Denied);
                                if (Login[0].MenuId.Contains((int)UserMenu.Access.Internal_ExternalBooking))
                                    HttpContext.Session.SetInt32("Internal_ExternalBooking", (int)UserMenu.Access.Internal_ExternalBooking);
                                else
                                    HttpContext.Session.SetInt32("Internal_ExternalBooking", (int)UserMenu.Access.Denied);
                                if (Login[0].MenuId.Contains((int)UserMenu.Access.RecurringBooking))
                                    HttpContext.Session.SetInt32("RecurringBooking", (int)UserMenu.Access.RecurringBooking);
                                else
                                    HttpContext.Session.SetInt32("RecurringBooking", (int)UserMenu.Access.Denied);
                                if (Login[0].MenuId.Contains((int)UserMenu.Access.ManageBranch))
                                    HttpContext.Session.SetInt32("ManageBranch", (int)UserMenu.Access.ManageBranch);
                                else
                                    HttpContext.Session.SetInt32("ManageBranch", (int)UserMenu.Access.Denied);
                                if (Login[0].MenuId.Contains((int)UserMenu.Access.BookingDiary))
                                    HttpContext.Session.SetInt32("BookingDiary", (int)UserMenu.Access.BookingDiary);
                                else
                                    HttpContext.Session.SetInt32("BookingDiary", (int)UserMenu.Access.Denied);
                                if (Login[0].MenuId.Contains((int)UserMenu.Access.Reports))
                                    HttpContext.Session.SetInt32("Reports", (int)UserMenu.Access.Reports);
                                else
                                    HttpContext.Session.SetInt32("Reports", (int)UserMenu.Access.Denied);


                              
                                return LocalRedirect("/ClientAdmin/Dashboard/index");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                return Page();
                            }

                        }
                        else
                        {



                            var result = await _signInManagerMaster.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                            var User = await _userManager.FindByNameAsync(Input.Email);
                            bool isAdmin = await _userManager.IsInRoleAsync(User, "SuperAdmin");


                            if (result.Succeeded && isAdmin)
                            {
                                if (Input.RememberMe)
                                {
                                    CookieOptions cookieOptions = new CookieOptions();
                                    cookieOptions.Expires = DateTime.Now.AddDays(30);
                                    Response.Cookies.Append("UserName", Input.Email, cookieOptions);
                                    Response.Cookies.Append("Password", Input.Password, cookieOptions);
                                    Response.Cookies.Append("CompanyCode", Input.ComanyCode, cookieOptions);
                                    Response.Cookies.Append("Remember_Me", Input.RememberMe.ToString(), cookieOptions);
                                }
                                _logger.LogInformation("User logged in.");
                                //return LocalRedirect(returnUrl);

                                HttpContext.Session.SetString("UserID", User.Id.ToString());
                                HttpContext.Session.SetString("FullName", User.FullName);
                                return LocalRedirect("/SuperAdmin/Company/index");
                            }

                            if (result.RequiresTwoFactor)
                            {
                                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                            }
                            if (result.IsLockedOut)
                            {
                                _logger.LogWarning("User account locked out.");
                                //return RedirectToPage("./Lockout");
                                return Page();
                            }
                            else
                            {
                                await _signInManagerMaster.SignOutAsync();
                                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                return Page();
                            }

                        }


                        #endregion

                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }


                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
               // return Page();
            }
        }
    } 

