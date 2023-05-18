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
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Integr8ed.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize]
    public class ProfileController : BaseController<ProfileController>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> IndexAsync()
        {
            var userData = new UserDto();
            var user = await _userManager.FindByIdAsync(HttpContext.Session.GetString("UserID"));
            userData.ID = user.Id;
            userData.UserName = user.UserName;
            userData.Password = user.PasswordHash;
            userData.Email = user.Email;
            userData.FirstName = user.FirstName;
            userData.MiddleName = user.MiddleName;
            userData.LastName = user.LastName;
            userData.MobileNumber = user.MobileNumber;
            userData.TelephoneNumber = user.TelephoneNumber;
            userData.Notes = user.Notes;
            userData.AddressLine1 = user.AddressLine1;
            userData.AddressLine2 = user.AddressLine2;

            return View(userData);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserDto model)
        {
            try
            {
                if (model.ID != 0)
                {
                    var objResult = _userManager.FindByIdAsync(model.ID.ToString()).Result;

                    //objResult.UserName = model.UserName;
                    //objResult.PasswordHash = EncodePasswordToBase64(model.Password);
                    //objResult.Email = model.Email;
                    objResult.FirstName = model.FirstName;
                    objResult.MiddleName = model.MiddleName;
                    objResult.LastName = model.LastName;
                    objResult.MobileNumber = model.MobileNumber;
                    objResult.TelephoneNumber = model.TelephoneNumber;
                    objResult.Notes = model.Notes;
                    objResult.AddressLine1 = model.AddressLine1;
                    objResult.AddressLine2 = model.AddressLine2;
                    objResult.IsActive = true;

                    var user = await _userManager.UpdateAsync(objResult);
                    //await _userManager.AddPasswordAsync(objResult, objResult.PasswordHash);
                    if (user != null)
                    {

                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.AdminUpdated);
                    }
                    else
                    {
                        // txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "Error in Update User");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }

                }
                return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
            }
            catch (Exception e)
            {
                throw e;

            }
        }

        //this function Convert to Encord your Password 
        public static string EncodePasswordToBase64(string password)
        {
            try
            {
                byte[] encData_byte = new byte[password.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(password);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }
        
        //this function Convert to Decord your Password
        public string DecodeFrom64(string encodedData)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encodedData);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new String(decoded_char);
            return result;
        }

    }
}