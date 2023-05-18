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
    public class RoomAvailabilityController : BaseController<RoomAvailabilityController>
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly IBookingDetailServices _bdService;
        private readonly IRoomTypesService _roomTypes;
        private readonly IUserService _user;
        private readonly IUserGroupServices _userGroup;
        private readonly IBookingStatusService _bookingStatus;

        #endregion

        #region ctor
        public RoomAvailabilityController(IRoomTypesService roomTypesService, IUserService user, IUserGroupServices userGroupServices, IConfiguration config, IBookingDetailServices bookingDetailServices , IBookingStatusService bookingStatus)
        {
            _roomTypes = roomTypesService;
            _userGroup = userGroupServices;
            _config = config;
            _bdService = bookingDetailServices;
            _user = user;
            _bookingStatus = bookingStatus;
        }
        #endregion

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

            var BookingStatusList = _bookingStatus.GetBookingStatusListForDropDown(HttpContext.Session.GetString("ConnectionString"));
            if (BookingStatusList.Count() != 0)
            {
                ViewBag.IsBookingStatusEmpty = false;
                ViewBag.BookingStatus = BookingStatusList.Select(x => new SelectListItem()
                {
                    Text = x.Status,
                    Value = x.Id.ToString()

                }).OrderBy(x => x.Text);
            }
            else
            {
                ViewBag.IsBookingStatusEmpty = true;
            }
            //var BookingStatusList = System.Enum.GetValues(typeof(GlobalEnums.BookingStatus)).Cast<GlobalEnums.BookingStatus>()
            //        .Select(d => (d, (int)d))
            //        .ToList();
            //ViewBag.BookingStatus = BookingStatusList.Select(x => new SelectListItem()
            //{
            //    Text = x.d.ToString(),
            //    Value = x.Item2.ToString()

            //}).OrderBy(x => x.Text);

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetRoomAvailiblity(JQueryDataTableParamModel param, RmAvParamDto model)
        {
            try
            {
                //var searchRecords = new SqlParameter { ParameterName = "@SearchRecords", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output };
                var Parameters = new List<SqlParameter>
                {
                new SqlParameter("@roomTypeId", SqlDbType.BigInt) { Value = model.roomTypeId },
                new SqlParameter("@startDate", SqlDbType.VarChar) { Value = Convert.ToDateTime(model.startDate).ToString("yyyy/MM/dd") },
                new SqlParameter("@endDate", SqlDbType.VarChar) { Value = Convert.ToDateTime(model.endDate).ToString("yyyy/MM/dd") },
                new SqlParameter("@maxPerson", SqlDbType.Int) { Value = model.maxPerson },
                new SqlParameter("@userGroup", SqlDbType.BigInt) { Value = model.userGroup },
                new SqlParameter("@BookingStatus", SqlDbType.Int) { Value = model.BookingStatus },
                new SqlParameter("@NumberOfAttending", SqlDbType.Int) { Value = model.NumberOfAttending },
                new SqlParameter("@ClientId", SqlDbType.BigInt) { Value = model.ClientId },
                new SqlParameter("@SUNDAY", SqlDbType.Bit) { Value = model.ExcludeSunday },
                new SqlParameter("@MONDAY", SqlDbType.Bit) { Value = model.ExcludeMonday },
                 new SqlParameter("@TUESDAY", SqlDbType.Bit) { Value = model.ExcludeTuesday },
                 new SqlParameter("@WEDNESDAY", SqlDbType.Bit) { Value = model.ExcludeWednesday },
                 new SqlParameter("@THURSDAY", SqlDbType.Bit) { Value = model.ExcludeThursday },
                 new SqlParameter("@FRIDAY", SqlDbType.Bit) { Value = model.ExcludeFriday },
                 new SqlParameter("@SATURDAY", SqlDbType.Bit) { Value = model.ExcludeSaturday },
                 new SqlParameter("@startTime", SqlDbType.VarChar) { Value = model.TimeStart == "" ? null : model.TimeStart },
                 new SqlParameter("@endTime", SqlDbType.VarChar) { Value = model.TimeEnd == "" ? null : model.TimeEnd },
                 new SqlParameter("@BranchId", SqlDbType.VarChar) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId")).ToString() }

                //searchRecords
            };
                
                var allList = await _bdService.GetRoomAvailiblity(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetRoomAvailiblity");
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
        public async Task<IActionResult> GetRoomAvailiblityDownloadExcel(long roomTypeId , string startDate , string endDate
           ,int maxPerson , int userGroup , int BookingStatus , int ClientId ,
            int NumberOfAttending , string TimeStart,  string TimeEnd , 
            bool ExcludeSunday ,
        bool ExcludeMonday ,
        bool ExcludeTuesday ,
        bool ExcludeWednesday,
        bool ExcludeThursday,
        bool ExcludeFriday ,
        bool ExcludeSaturday 
             
            
            )
        {
            try
            {
                var searchRecords = new SqlParameter { ParameterName = "@SearchRecords", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output };
                var Parameters = new List<SqlParameter>
                {
                     
               new SqlParameter("@roomTypeId", SqlDbType.BigInt) { Value = roomTypeId },
               new SqlParameter("@startDate", SqlDbType.VarChar) { Value = startDate },
               new SqlParameter("@endDate", SqlDbType.VarChar) { Value = endDate },
               new SqlParameter("@maxPerson", SqlDbType.Int) { Value = maxPerson },
               new SqlParameter("@userGroup", SqlDbType.BigInt) { Value = userGroup },
               new SqlParameter("@BookingStatus", SqlDbType.Int) { Value = BookingStatus },
               new SqlParameter("@ClientId", SqlDbType.BigInt) { Value = ClientId },
               new SqlParameter("@SUNDAY", SqlDbType.Bit) { Value = ExcludeSunday },
               new SqlParameter("@MONDAY", SqlDbType.Bit) { Value = ExcludeMonday },
               new SqlParameter("@TUESDAY", SqlDbType.Bit) { Value = ExcludeTuesday },
               new SqlParameter("@NumberOfAttending", SqlDbType.Int) { Value = NumberOfAttending },
               new SqlParameter("@WEDNESDAY", SqlDbType.Bit) { Value = ExcludeWednesday },
               new SqlParameter("@THURSDAY", SqlDbType.Bit) { Value = ExcludeThursday },
               new SqlParameter("@FRIDAY", SqlDbType.Bit) { Value = ExcludeFriday },
               new SqlParameter("@SATURDAY", SqlDbType.Bit) { Value = ExcludeSaturday },
               new SqlParameter("@startTime", SqlDbType.VarChar) { Value = TimeStart == "" ? null : TimeStart },
               new SqlParameter("@endTime", SqlDbType.VarChar) { Value = TimeEnd == "" ? null : TimeEnd },
               new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId")).ToString()  },
               searchRecords
            };
                var allList = await _bdService.GetRoomAvailiblityforDownloadExcel(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
                var total = allList.FirstOrDefault()?.TotalRecords ?? 0;

                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = "Room_Availability_List.xlsx";

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
                    for (int index =0 ; index < allList.Count; index++)
                    {
                        worksheet.Cell(ExcelRowID, 1).Value = allList[index].Title;
                        worksheet.Cell(ExcelRowID, 2).Value = allList[index].BookingDate; 
                        worksheet.Cell(ExcelRowID, 3).Value = allList[index].Day;
                        worksheet.Cell(ExcelRowID, 4).Value = allList[index].StartTime;
                        worksheet.Cell(ExcelRowID, 5).Value = allList[index].EndTime;
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
                return Json(new
                {
                   
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = ""
                });
            }
        }

        [HttpGet]
        public IActionResult DownloadExcelDocument(List<RoomAvailiblityDto> roomAvailiblities = null)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "RoomAvailability.xlsx";
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Add("RoomAvailability");
                    worksheet.Cell(1, 1).Value = "Id";
                    worksheet.Cell(1, 2).Value = "Title";
                    worksheet.Cell(1, 3).Value = "AvailableDate";
                    worksheet.Cell(1, 4).Value = "StartTime";
                    worksheet.Cell(1, 5).Value = "FinishTime";

                    for (int index = 1; index <= roomAvailiblities.Count; index++)
                    {
                        worksheet.Cell(index + 1, 1).Value = roomAvailiblities[index - 1].Id;
                        worksheet.Cell(index + 1, 2).Value = roomAvailiblities[index - 1].Title;
                        worksheet.Cell(index + 1, 3).Value = roomAvailiblities[index - 1].BookingDate;
                        worksheet.Cell(index + 1, 4).Value = roomAvailiblities[index - 1].StartTime;
                        worksheet.Cell(index + 1, 5).Value = roomAvailiblities[index - 1].EndTime;
                    }
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, contentType, fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content("Test"); 
            }
        }


        [HttpPost]
        public async Task<IActionResult> GetbookingTableList(RmAvParamDto model)
        {
            var Parameters = new List<SqlParameter>{ 
                new SqlParameter("@BookingDate", SqlDbType.VarChar) { Value = model.BookingDate },
                new SqlParameter("@roomTypeId", SqlDbType.BigInt) { Value = model.roomTypeId },
                new SqlParameter("@clientId", SqlDbType.BigInt) { Value = model.ClientId },
                new SqlParameter("@numberOfAttending", SqlDbType.Int) { Value = model.NumberOfAttending },
                new SqlParameter("@bookingStatus", SqlDbType.Int) { Value = model.BookingStatus },
                new SqlParameter("@userGroupId", SqlDbType.BigInt) { Value = model.userGroup },
                new SqlParameter("@MaxPerson", SqlDbType.Int) { Value = model.maxPerson }
            };
            
            var allList = await _bdService.GetbookingTableList(HttpContext.Session.GetString("ConnectionString"), Parameters.ToArray());
            return Json(allList);
        }

    }
}