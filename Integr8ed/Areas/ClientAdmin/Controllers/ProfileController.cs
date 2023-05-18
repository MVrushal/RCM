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
using Microsoft.AspNetCore.Mvc.Rendering;
using Integr8ed.Data.DbModel.ClientAdmin;
using Microsoft.Extensions.Configuration;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class ProfileController : BaseController<ProfileController>
    {
        #region Fields
        private readonly IProfileServices _profile;
        private readonly IConfiguration _config;
        #endregion

        #region ctor
        public ProfileController(IProfileServices profile, IConfiguration config)
        {
            _profile = profile;
            _config = config;
        }
        #endregion

        #region Methods
        public IActionResult Index()
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);

            var userData = new UserDto();
            var user = _profile.GetSingle(HttpContext.Session.GetString("ConnectionString"),
            x => x.Id == Convert.ToInt32(HttpContext.Session.GetString("UserID")));
            userData.ID = user.Id;
            userData.UserName = user.UserName;
            userData.Password = user.Password;
            userData.Email = user.Email;
            userData.FirstName = user.FirstName;
            userData.MiddleName = user.MiddleName;
            userData.LastName = user.LastName;
            userData.MobileNumber = user.MobileNumber;
            userData.TelephoneNumber = user.TelephoneNumber;
            userData.Notes = user.Notes;
            userData.AddressLine1 = user.AddressLine1;
            userData.AddressLine2 = user.AddressLine2;
            userData.UserImage = user.UserImage;
            userData.CompanyCode = HttpContext.Session.GetString("CompanyCode");
            HttpContext.Session.SetString("ClientAdminProfileImage", userData.UserImage ?? "");
            return View(userData);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.ID != 0)
                    {
                        var objResult = _profile.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.ID);

                        #region UserImage
                        string newImage = string.Empty, newLicenseFile = string.Empty;
                        if (model.UserImageFile != null)
                        {
                            newImage = $@"User-{CommonMethod.GetFileName(model.UserImageFile.FileName)}";
                            await CommonMethod.UploadFileAsync(HostingEnvironment.WebRootPath, FilePathListConstant.UploadedFiles, newImage, model.UserImageFile);

                            objResult.UserImage = newImage;
                            HttpContext.Session.SetString("ClientAdminProfileImage", objResult.UserImage);
                        }
                        #endregion

                        //  objResult.UserName = model.UserName;
                        //objResult.Password = model.Password;
                        // objResult.Email = model.Email;
                        objResult.FirstName = model.FirstName;
                        objResult.MiddleName = model.MiddleName;
                        objResult.LastName = model.LastName;
                        objResult.MobileNumber = model.MobileNumber;
                        objResult.TelephoneNumber = model.TelephoneNumber;
                        objResult.Notes = model.Notes;
                        objResult.AddressLine1 = model.AddressLine1;
                        objResult.AddressLine2 = model.AddressLine2;
                        objResult.IsActive = true;

                        var user = await _profile.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (user != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.ProfileUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update User");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Update User");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion
    }
}