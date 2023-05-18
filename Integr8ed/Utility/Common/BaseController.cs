using AutoMapper;
using AutoMapper.Configuration;
using Integr8ed.Service.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using Integr8ed.Service.Interface;
using Integr8ed.Service.Enums;

namespace Integr8ed.Utility.Common
{
    public class BaseController<T> : Controller where T : BaseController<T>
    {
        #region Fields
        protected JsonResponse JsonResponse => new JsonResponse();

      

        private IErrorLogService _errorLog;
        protected IErrorLogService ErrorLog => _errorLog ?? (_errorLog = HttpContext.RequestServices.GetService<IErrorLogService>());

        private IHttpContextAccessor _accessor;
        protected IHttpContextAccessor Accessor => _accessor ?? (_accessor = HttpContext.RequestServices.GetService<IHttpContextAccessor>());

        [Obsolete]
        private IHostingEnvironment _hostingEnvironment;

        [Obsolete]
        protected IHostingEnvironment HostingEnvironment => _hostingEnvironment ?? (_hostingEnvironment = HttpContext.RequestServices.GetService<IHostingEnvironment>());

        private IMapper _mapper;
        protected IMapper Mapper => _mapper ?? (_mapper = HttpContext.RequestServices.GetService<IMapper>());

        private IConfiguration _config;
        protected IConfiguration Config => _config ?? (_config = HttpContext.RequestServices.GetService<IConfiguration>());
        
        #endregion

        public string GetSortingColumnName(int sortColumnNo)
        {
            return Accessor.HttpContext.Request.Query["mDataProp_" + sortColumnNo][0];
        }


        public  bool IsAccess(string ModuleName)
        {
            bool IsAuthorized;
            try
            {

                int currPermission=0;
                switch (ModuleName)
                {
                    case "RoomType": currPermission = (int)UserMenu.Access.RoomType;break;
                    case "EquipmentRequirement": currPermission = (int)UserMenu.Access.EquipmentRequirement;break;
                    case "DelegetsCodes": currPermission = (int)UserMenu.Access.DelegetsCodes;break;
                    case "MeetingType": currPermission = (int)UserMenu.Access.MeetingType;break;
                    case "InvoiceItem": currPermission = (int)UserMenu.Access.InvoiceItem;break;
                    case "EntryType": currPermission = (int)UserMenu.Access.EntryType;break;
                    case "UserGroup": currPermission = (int)UserMenu.Access.UserGroup;break;
                    case "CateringDetail": currPermission = (int)UserMenu.Access.CateringDetail; break;
                } 

                if (HttpContext.Session.GetInt32("IsAdmin") != 1 && HttpContext.Session.GetInt32("IsBranchAdmin") != 1)
                {
                    IsAuthorized = HttpContext.Session.GetInt32(ModuleName) == currPermission ? true : false;

                } else { 

                 IsAuthorized = (HttpContext.Session.GetInt32("IsAdmin") == 1 || HttpContext.Session.GetInt32("IsBranchAdmin") == 1 )? true:false;
                }

            }
            catch (Exception e)
            {

                IsAuthorized = false;
            }


            return IsAuthorized;
        }

        public string GetConfigValue(string key)
        {
            return Config.GetValue<string>(key);
        }

        public bool  CheckISSessionExpired()
        {
            if (Accessor.HttpContext.Session.GetString("ConnectionString") != null && Accessor.HttpContext.Session.GetString("ConnectionString") != "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
    public class JsonResponse
    {
        public IActionResult GenerateJsonResult(int status, string message = null, object data = null)
        {
            return new JsonResult(new JsonResponseHelper
            {
                Status = status,
                Data = data,
                Message = message
            });
        }
    }

    public class DatatableResponseHelper
    {
        public int TotalElements { get; set; }
        public object Data { get; set; }
    }

    public class JsonResponseHelper
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
