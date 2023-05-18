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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IO;

namespace Integr8ed.Areas.ClientAdmin.Controllers
{
    [Area("ClientAdmin")]
    public class RoomTypesController : BaseController<RoomTypesController>
    {
        #region Fields
        private readonly IStandardEquipmentServices _standardEquipment;
        private readonly IRoomTypesService _room;
        private readonly IRoomImageService _roomImageService;
        private readonly IConfiguration _config;
        private readonly IBookingDetailServices _bookingDetail;
        private string cnnstr;
        private long userId;
        private readonly IEquipServices _EquipRequirement;
        #endregion

        #region ctor
        public RoomTypesController(IBookingDetailServices bookingDetails , IRoomImageService roomImageService,IEquipServices equipServices, IStandardEquipmentServices standardEquipment, IRoomTypesService room, IConfiguration config)
        {
            _roomImageService = roomImageService;
            _EquipRequirement = equipServices;
            _standardEquipment = standardEquipment;
            _room = room;
            _config = config;
            _bookingDetail = bookingDetails;
        }
        #endregion

        #region Method
        public IActionResult Index()
        {
            bool status = CheckISSessionExpired();
            if (status)
                return Redirect(_config["CommonProperty:PhysicalUrl"]);

            if (IsAccess("RoomType"))
                return View(new EquipmentDto() { Id = 0 });
            else
                return RedirectToAction("Index", "Dashboard");

        }

        [HttpGet]
        public IActionResult _AddEditRoomImages(long id, bool isView)
        {
            if (id == 0) return View(@"Components/_AddEditRoomImages", new RoomTypeImageDto { Id = id });
            var tempView = new RoomTypeImageDto();
            tempView = _room.GetImages(HttpContext.Session.GetString("ConnectionString"), id);

          //  tempView.path = HostingEnvironment.WebRootPath + FilePathListConstant.RoomImages + @"/";
            tempView.path = Directory.GetCurrentDirectory()+ @"/wwwroot/" + FilePathListConstant.RoomImages + @"/";


            return View(@"Components/_AddEditRoomImages", tempView);
        }

        [HttpPost]
        public async Task<IActionResult> AddEditRoomImages(long Id)
        {

            var roomimages = Request.Form.Files;
            try
            {
                if (Request.Form.Files.Count() != 0)
                {

                    var roomlist = new List<RoomTypeImageDto>();
                    foreach (var rimg in roomimages)
                    {

                        var newImage = $@"Room-{CommonMethod.GetFileName(rimg.FileName)}";
                       // await CommonMethod.UploadFileAsync(HostingEnvironment.WebRootPath, FilePathListConstant.RoomImages, newImage, rimg);
                        await CommonMethod.UploadFileAsync(Directory.GetCurrentDirectory() + @"/wwwroot/", FilePathListConstant.RoomImages, newImage, rimg);

                        var temp = new RoomTypeImageDto();
                        temp.RoomId = Id;
                        temp.ImageName = newImage;
                        roomlist.Add(temp);
                    }
                    _room.InsertImage(HttpContext.Session.GetString("ConnectionString"), roomlist);
                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.uploadImage);

                }
                else
                {
                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.EmptyImage);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.AddErrorLog(null, "Room Image Upload excetions  --- "+ex.Message);
                return JsonResponse.GenerateJsonResult(0, ex.Message);
            }


        }

        [HttpGet]
        public IActionResult _AddEditStandardEquipment(long id, bool isView)
        {
            if (id == 0) return View(@"Components/_AddEditStandardEquipment", new EquipmentDto { Id = id, IsView = isView });
            var tempView = new EquipmentDto();
            var objResult = _standardEquipment.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.Title = objResult.Title;
            tempView.Description = objResult.Description;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            tempView.IsActive = objResult.IsActive;
            return View(@"Components/_AddEditStandardEquipment", tempView);
        }

        [HttpPost]
        public async Task<IActionResult> AddEditEquipment(EquipmentDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        StandardEquipment se = new StandardEquipment();
                        se.code = model.Title;
                        se.Title = model.Title;
                        se.Description = model.Description;
                        se.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());



                        var standardEquipment = await _standardEquipment.InsertAsync(HttpContext.Session.GetString("ConnectionString"), se, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (standardEquipment != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.StandardEquipmentCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Standard Equipment");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }

                    else
                    {

                        var objResult = _standardEquipment.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.code = model.Title;
                        objResult.Title = model.Title;
                        objResult.Description = model.Description;

                        var standardEquipment = await _standardEquipment.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (standardEquipment != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.StandardEquipmentUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Standard Equipment");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Standard Equipment");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEquipmentList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _standardEquipment.GetStandardEquipmentList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "Get Standard EquipmentList");
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
        public async Task<IActionResult> RemoveEquipment(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var questionObj = _standardEquipment.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _standardEquipment.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.StandardEquipmentDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveEquipment");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public IActionResult DeleteImage(long id, string rimg)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    CommonMethod.DeleteFile(Path.Combine(HostingEnvironment.WebRootPath, FilePathListConstant.RoomImages, rimg));
                    var deleteResult = _room.DeleteImage(HttpContext.Session.GetString("ConnectionString"), id, rimg);
                    if (deleteResult)
                    {
                        txscope.Complete();
                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.deleteImage);
                    }
                    else
                    {
                        txscope.Dispose();
                        ErrorLog.AddErrorLog(null, "GET-DeleteImage");
                        return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                    }
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "GET-DeleteImage");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditRoomTypes(Room_TypeDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        //var roomObj = Mapper.Map<Room_Type>(model);
                        var roomObj = new Room_Type();
                        roomObj.Title = model.RoomTitle;
                        roomObj.code = model.code;
                        roomObj.Description = model.Description;
                        roomObj.Notes = model.Notes;
                        roomObj.Floor = model.Floor;
                        roomObj.HourlyRate = model.HourlyRate;
                        roomObj.SundayRate = model.SundayRate;
                        roomObj.SaturdayRate = model.SaturdayRate;
                        roomObj.Maxperson = model.Maxperson;
                        roomObj.IsActive = model.IsActive;
                        roomObj.BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
                        roomObj.RoomOrder = model.RoomOrder;

                        var standardEquipment = await _room.InsertAsync(HttpContext.Session.GetString("ConnectionString"), roomObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (standardEquipment != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.RoomTypeCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Post - AddEditRoomTypes / Else  Create part");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }

                    else
                    {
                        var objResult = _room.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        // objResult = Mapper.Map<Room_Type>(model);
                        objResult.Title = model.RoomTitle;
                        objResult.code = model.code;
                        objResult.Description = model.Description;
                        objResult.Notes = model.Notes;
                        objResult.Floor = model.Floor;
                        objResult.HourlyRate = model.HourlyRate;
                        objResult.SundayRate = model.SundayRate;
                        objResult.SaturdayRate = model.SaturdayRate;
                        objResult.Maxperson = model.Maxperson;
                        objResult.IsActive = model.IsActive;
                        objResult.RoomOrder = model.RoomOrder;
                        var roomUpdated = await _room.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (roomUpdated != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.RoomTypeUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Post - AddEditRoomTypes / Else  Update part");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }

                    }
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-AddEditRoomTypes");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public IActionResult _AddEditRoomTypes(long id, bool isView)
        {
            if (id == 0) return View(@"Components/_AddEditRoomTypes", new Room_TypeDto { Id = id, IsView = isView, IsActive = true });
            var tempView = new Room_TypeDto();
            var objResult = _room.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView = Mapper.Map<Room_TypeDto>(objResult);
            tempView.IsView = isView;
            return View(@"Components/_AddEditRoomTypes", tempView);
        }

        [HttpGet]
        public async Task<IActionResult> GetRoomTypesList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _room.GetRoomTypesList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetStandardEquipmentList");
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
        public async Task<IActionResult> RemoveRoomType(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var BookingExist = _bookingDetail.GetBookingDetailForDelete(HttpContext.Session.GetString("ConnectionString")).Where(x=>x.RoomTypeId == id).ToList().Count();
                    if (BookingExist != 0)
                    {
                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.CheckBookingAvailableUsingRoomType);
                    }
                    else
                    {
                        var Obj = _room.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                        Obj.IsDelete = true;
                        await _room.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), Obj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        txscope.Complete();

                        return JsonResponse.GenerateJsonResult(1, ResponseConstants.RoomTypeDeleted);
                    }
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveRoomType");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }
        #endregion

        #region Equipment Req

        [HttpGet]
        public IActionResult _AddEditEquipRequirement(long id, bool isView)
        {
            var BranchId = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString());
            var roolist = _room.GET_EuipmentList(HttpContext.Session.GetString("ConnectionString"), BranchId);
            var tempView = new EquiptDto();
            var equipmentList = _EquipRequirement.GET_EuipmentReqList(HttpContext.Session.GetString("ConnectionString")).Result;
            var EquipObj = _EquipRequirement.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id && x.IsDelete == false);
            var NewRoolList = new List<StandardEquipment>();
            if (roolist.Count() != 0)
            {
                ViewBag.IsRoomEmpty = false;
                if (equipmentList.Count() != 0)
                {
                    var temp = equipmentList.Where(x => !x.IsDelete).Select(y => y.EquipId);
                    if (roolist.Where(x => !temp.Contains(x.Id)).Count() != 0)
                        NewRoolList.AddRange(roolist.Where(x => !temp.Contains(x.Id)));
                    if (EquipObj != null)
                        NewRoolList.Add(roolist.FirstOrDefault(x => x.Id == EquipObj.EquipId));

                    ViewBag.IsRoomEmpty = NewRoolList.Count() == 0 ? true : false;
                    ViewBag.EquipList = NewRoolList.Select(x => new SelectListItem()
                    {
                        Text = x.Title,
                        Value = x.Id.ToString()
                    }).OrderBy(x => x.Text);
                }
                else
                {
                    ViewBag.EquipList = roolist.Select(x => new SelectListItem()
                    {
                        Text = x.Title,
                        Value = x.Id.ToString()
                    }).OrderBy(x => x.Text);
                }
            }
            else
            {
                ViewBag.IsRoomEmpty = true;
                tempView.IsView = true;
            }

            if (id == 0) return View(@"Components/_AddEditEquipRequirement", new EquiptDto { Id = id, IsView = isView });

            var objResult = _EquipRequirement.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
            tempView.Title = objResult.Title;
            tempView.Description = objResult.Description;
            tempView.EquipId = objResult.EquipId;
            tempView.Id = objResult.Id;
            tempView.IsView = isView;
            return View(@"Components/_AddEditEquipRequirement", tempView);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditEquipType(EquiptDto model)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.Id == 0)
                    {
                        Equipment MT = new Equipment();
                        MT.code = model.Title;
                        MT.Title = model.Title;
                        MT.Description = model.Description;
                        MT.EquipId = model.EquipId;

                        var standardEquipment = await _EquipRequirement.InsertAsync(HttpContext.Session.GetString("ConnectionString"), MT, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (standardEquipment != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.EquipmentCreated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in create Equipment");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                    else
                    {
                        var objResult = _EquipRequirement.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == model.Id);
                        objResult.code = model.Title;
                        objResult.Title = model.Title;
                        objResult.Description = model.Description;
                        objResult.EquipId = model.EquipId;

                        var Equipment = await _EquipRequirement.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), objResult, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                        if (Equipment != null)
                        {
                            txscope.Complete();
                            return JsonResponse.GenerateJsonResult(1, ResponseConstants.EquipmentUpdated);
                        }
                        else
                        {
                            txscope.Dispose();
                            ErrorLog.AddErrorLog(null, "Error in Update Equipment");
                            return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                        }
                    }
                }
                catch (Exception e)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(null, "Error in Create or Update Equipment");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEquipmentReqList(JQueryDataTableParamModel param)
        {
            try
            {
                var parameters = CommonMethod.GetJQueryDatatableParamList(param, GetSortingColumnName(param.iSortCol_0)).Parameters;
                parameters.Insert(0, new SqlParameter("@BranchId", SqlDbType.BigInt) { Value = Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()) });
                var allList = await _EquipRequirement.GetEquipmentList(HttpContext.Session.GetString("ConnectionString"), parameters.ToArray());
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
                ErrorLog.AddErrorLog(ex, "GetEquipmentReqList");
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
        public async Task<IActionResult> RemoveEquipType(long id)
        {
            using (var txscope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var questionObj = _EquipRequirement.GetSingle(HttpContext.Session.GetString("ConnectionString"), x => x.Id == id);
                    questionObj.IsDelete = true;
                    await _EquipRequirement.UpdateAsync(HttpContext.Session.GetString("ConnectionString"), questionObj, Accessor, Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                    txscope.Complete();

                    return JsonResponse.GenerateJsonResult(1, ResponseConstants.EquipmentRequirementDeleted);
                }
                catch (Exception ex)
                {
                    txscope.Dispose();
                    ErrorLog.AddErrorLog(ex, "Post-RemoveEquipment");
                    return JsonResponse.GenerateJsonResult(0, ResponseConstants.SomethingWrong);
                }
            }
        }

        #endregion

        #region Common
        public bool CheckTitle(string RoomTitle, long Id)
        {
            bool isExist;
            var result = _room.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                x => x.Title.ToLower().Equals(RoomTitle.ToLower()) && x.IsDelete == false && x.BranchId == Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            if (result != null)
            {
                isExist = result.Title.ToLower().Trim().Equals(RoomTitle.ToLower().Trim()) ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = _room.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                        x => x.Title.ToLower().Equals(RoomTitle.ToLower()) && x.Id == Id && x.IsDelete == false);
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

        public bool CheckEquipmentTitle(string Title, long Id)
        {
            bool isExist;
            var result = _standardEquipment.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                x => x.Title.ToLower().Equals(Title.ToLower()) && x.IsDelete == false && x.BranchId == Convert.ToInt64(HttpContext.Session.GetString("BranchId").ToString()));
            if (result != null)
            {
                isExist = result.Title.ToLower().Trim().Equals(Title.ToLower().Trim()) ? true : false;
                if (isExist && Id != 0)
                {
                    var resultExist = _standardEquipment.GetSingle(HttpContext.Session.GetString("ConnectionString"),
                        x => x.Title.ToLower().Equals(Title.ToLower()) && x.Id == Id && x.IsDelete == false);
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