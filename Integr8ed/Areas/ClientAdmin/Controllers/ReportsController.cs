using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Integr8ed.Models;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Enums;
using Integr8ed.Service.Interface;
using Integr8ed.Service.Interface.ClientAdmin;
using Integr8ed.Utility.Common;
using Integr8ed.Utility.JqueryDataTable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class ReportsController : BaseController<ReportsController>
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly IReportsService _bdService;
        private readonly IRoomTypesService _roomTypes;
        private readonly IUserService _user;
        private readonly IUserGroupServices _userGroup;
        private readonly IBookingStatusService _bookingStatus;

        #endregion

        #region ctor
        public ReportsController(IRoomTypesService roomTypesService, IUserService user, IUserGroupServices userGroupServices, IConfiguration config, IReportsService bookingDetailServices,IBookingStatusService bookingStatus)
        {
            _roomTypes = roomTypesService;
            _userGroup = userGroupServices;
            _config = config;
            _bdService = bookingDetailServices;
            _user = user;
            _bookingStatus = bookingStatus;
        }


        #endregion

        #region Method
        public async Task<IActionResult> Index()
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);
            var roomTypeList = _roomTypes.GetRoomTypeListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            ViewBag.RoomType = roomTypeList.Select(x => new SelectListItem()
            {
                Text = x.Title,
                Value = x.Id.ToString()
            }).OrderBy(x => x.Text);

            var userList = await _user.GetUsersList(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            ViewBag.UserList = userList.Select(x => new SelectListItem()
            {
                Text = x.FirstName + " " + x.LastName,
                Value = x.Id.ToString()
            }).OrderBy(x => x.Text);

            var userGroupList = _userGroup.GetUserGroupListForDropDown(HttpContext.Session.GetString("ConnectionString"), Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            ViewBag.UserGroup = userGroupList.Select(x => new SelectListItem()
            {
                Text = x.Title,
                Value = x.Id.ToString()
            }).OrderBy(x => x.Text);

            //var BookingStatusList = System.Enum.GetValues(typeof(GlobalEnums.BookingStatus)).Cast<GlobalEnums.BookingStatus>()
            //        .Select(d => (d, (int)d))
            //        .ToList();
            //ViewBag.BookingStatus = BookingStatusList.Select(x => new SelectListItem()
            //{
            //    Text = x.d.ToString(),
            //    Value = x.Item2.ToString()

            //}).OrderBy(x => x.Text);

            var BookingStatusList = _bookingStatus.GetBookingStatusListForDropDown(HttpContext.Session.GetString("ConnectionString"));
            if (BookingStatusList.Count() != 0)
            {
                ViewBag.IsBookingStatusEmpty = false;
                var data = BookingStatusList.Select(x => new SelectListItem()
                {
                    Text = x.Status.ToString(),
                    Value = x.Id.ToString()

                }).OrderBy(x => x.Text);
                ViewBag.BookingStatus = JsonConvert.SerializeObject(data);
            }
            else
            {
                ViewBag.IsBookingStatusEmpty = true;
            }
            return View();
           
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingsReportList(JQueryDataTableParamModel param, ReportsDTO model)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@roomTypeId", SqlDbType.BigInt) { Value = model.roomTypeId });
                parameters.Insert(1, new SqlParameter("@BookingStatus", SqlDbType.BigInt) { Value = model.BookingStatus });
                parameters.Insert(2, new SqlParameter("@StartDate", SqlDbType.VarChar) { Value = Convert.ToDateTime(model.startDate).ToString("yyyy/MM/dd") });
                parameters.Insert(3, new SqlParameter("@EndDate", SqlDbType.VarChar) { Value = Convert.ToDateTime(model.endDate).ToString("yyyy/MM/dd") });
                parameters.Insert(4, new SqlParameter("@StartTime", SqlDbType.VarChar) { Value = model.TimeStart??""});
                parameters.Insert(5, new SqlParameter("@EndTime", SqlDbType.VarChar) { Value = model.TimeEnd??""});
                parameters.Insert(6, new SqlParameter("@UserGroupId", SqlDbType.BigInt) { Value = model.userGroupID });
                parameters.Insert(7, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId"))});
                //parameters.Insert(4, new SqlParameter("@userGroup", SqlDbType.BigInt) { Value = model.userGroup });
                //parameters.Insert(6, new SqlParameter("@NumberOfAttending", SqlDbType.Int) { Value = model.NumberOfAttending });
                //parameters.Insert(7, new SqlParameter("@ClientId", SqlDbType.BigInt) { Value = model.ClientId });
                var allList = await _bdService.GetReportList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
                var total = allList.Count;
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
                ErrorLog.AddErrorLog(ex, "GetBookingsReportList");
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
        public async Task<FileResult> DownloadExcel(string jsonString)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<ReportsDTO>(jsonString);
              
               var Parameters = new List<SqlParameter>
                {

                        new SqlParameter("@roomTypeId", SqlDbType.BigInt) { Value = model.roomTypeId },
                        new SqlParameter("@BookingStatusId", SqlDbType.Int) { Value = model.BookingStatus },
                        new SqlParameter("@UserGroupId", SqlDbType.BigInt) { Value = model.userGroupID },
                        new SqlParameter("@StartDate", SqlDbType.VarChar) { Value = Convert.ToDateTime(model.startDate).ToString("yyyy/MM/dd") },
                        new SqlParameter("@EndDate", SqlDbType.VarChar) { Value = Convert.ToDateTime(model.endDate).ToString("yyyy/MM/dd") },
                        new SqlParameter("@StartTime", SqlDbType.VarChar) { Value = model.TimeStart == "" ? null : model.TimeStart },
                        new SqlParameter("@EndTime", SqlDbType.VarChar) { Value = model.TimeEnd == "" ? null : model.TimeEnd },
                        new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString())}
                };
                var allList = await _bdService.GetReportExcel(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
              

                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = "Boking Report.xlsx";

                using (var workbook = new XLWorkbook())
                {
                    IXLWorksheet worksheet =
                    workbook.Worksheets.Add("RoomAvailability");
                    worksheet.Cell(1, 1).Value = "Room";
                    worksheet.Cell(1, 2).Value = "Booking Date";
                    worksheet.Cell(1, 3).Value = "Day";
                    worksheet.Cell(1, 4).Value = "Time Start";
                    worksheet.Cell(1, 5).Value = "Time End";
                    int ExcelRowID = 2;



                    for (int index = 0; index < allList.Count; index++)
                    {
                        worksheet.Cell(ExcelRowID, 1).Value = allList[index].RoomType;
                        worksheet.Cell(ExcelRowID, 2).Value = allList[index].BookingDate;
                        worksheet.Cell(ExcelRowID, 3).Value = Convert.ToDateTime(allList[index].BookingDate).Day;
                        worksheet.Cell(ExcelRowID, 4).Value = allList[index].StartTime;
                        worksheet.Cell(ExcelRowID, 5).Value = allList[index].FinishTime;
                        ExcelRowID = ExcelRowID + 1;

                    }


                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, contentType, fileName);
                        // return Content("TEst");
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorLog.AddErrorLog(ex, "GetRoomAvailiblity");
                return null;
            }
        }

        #endregion
    }
}
