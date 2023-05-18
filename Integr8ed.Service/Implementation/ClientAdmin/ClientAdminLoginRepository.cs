using Integr8ed.Data.DbContext.ClientAdminContext;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Service.BaseService;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Integr8ed.Service.Implementation
{
    public class ClientAdminLoginRepository : IClientAdminLogin
    {
        private readonly ClientAdminDbContext _context;
        public ClientAdminLoginRepository(ClientAdminDbContext context)
        {
            _context = context;
        }

        public List<Users> ForgotPassword(UserDto userDto, string connectionstring)
        {
            try
            {
                var dbConnection = new SqlConnection(connectionstring);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var data = db.Users.Where(u => u.Email == userDto.Email && !u.IsDelete).ToList();

                    return data;
                }

            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<UserDto> Login(UserDto userDto, string connectionstring)
        {
            try
            {
                var dbConnection = new SqlConnection(connectionstring);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    // var data = db.Users.Where(u => u.Email == userDto.UserName  && u.Password == userDto.Password && !u.IsDelete ).ToList();
                    var userResult = (from user in db.Users
                                      join branch in db.BranchMaster on user.BranchId equals branch.Id
                                      join ur in db.UserRoles on user.Id equals ur.UserId
                                      join role in db.Roles on ur.RoleId equals role.Id
                                      where user.Email == userDto.UserName && user.Password == userDto.Password && !user.IsDelete && branch.IsActive && !branch.IsDelete

                                      select new UserDto
                                      {
                                          UserName = user.UserName,
                                          Email = user.Email,
                                          ID = user.Id,
                                          FullName = user.FirstName + " " + user.LastName,
                                          MenuId = db.UserAccess.Where(x => x.UserId == user.Id).Select(x => Convert.ToInt32(x.MenuId)).ToList(),
                                          UserImage = user.UserImage,
                                          AddressLine1 = user.AddressLine1,
                                          AddressLine2 = user.AddressLine2,
                                          MobileNumber = user.MobileNumber,
                                          BranchId = user.BranchId ?? 0,
                                          AdminBranchId = user.AdminBranchId,
                                          Role = role.RoleName,
                                          RoleId = ur.RoleId,
                                          IsAdmin = ur.RoleId == 1 ? true : false

                                      }).ToList();

                    var ExternalLogin = (from user in db.Users
                                         join ur in db.UserRoles on user.Id equals ur.UserId
                                         join role in db.Roles on ur.RoleId equals role.Id
                                         where user.Email == userDto.UserName && user.Password == userDto.Password && !user.IsDelete

                                         select new UserDto
                                         {
                                             UserName = user.UserName,
                                             Email = user.Email,
                                             ID = user.Id,
                                             FullName = user.FirstName + " " + user.LastName,
                                             MenuId = db.UserAccess.Where(x => x.UserId == user.Id).Select(x => Convert.ToInt32(x.MenuId)).ToList(),
                                             UserImage = user.UserImage,
                                             AddressLine1 = user.AddressLine1,
                                             AddressLine2 = user.AddressLine2,
                                             MobileNumber = user.MobileNumber,
                                             Role = role.RoleName,
                                             RoleId = ur.RoleId,
                                             IsAdmin = ur.RoleId == 1 ? true : false

                                         }).ToList();

                    if (userResult.Count() != 0)
                    {


                        return userResult;
                        //var result = data.Select(m => new UserDto
                        //{
                        //    UserName = m.UserName,
                        //    Email = m.Email,
                        //    ID = m.Id,
                        //    FullName = m.FirstName + " " + m.LastName,
                        //    MenuId= db.UserAccess.Where(x => x.UserId == m.Id).Select(x =>Convert.ToInt32(x.MenuId)).ToList(),
                        //    UserImage = m.UserImage,
                        //    AddressLine1 = m.AddressLine1,
                        //    AddressLine2 = m.AddressLine2,
                        //    MobileNumber = m.MobileNumber,
                        //    BranchId=m.BranchId??0,
                        //    IsAdmin=m.IsAdmin??false

                        //}).ToList();


                    }
                    else if (ExternalLogin.Count() != 0)
                        return ExternalLogin;
                    else
                        return null;

                }

            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Users UpdatedUserDetail(UserDto userDto, string connectionstring)
        {
            try
            {
                var dbConnection = new SqlConnection(connectionstring);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var data = db.Users.Where(u => u.Id == userDto.ID).SingleOrDefault();

                    if (data != null)
                    {

                        data.Frgt_Code = userDto.Frgt_Code;
                        //data.Email = m.Email,
                        //data.ID = m.Id,
                        //data.FullName = m.FirstName + " " + m.LastName,
                        //data.MenuId = db.UserAccess.Where(x => x.UserId == m.Id).Select(x => Convert.ToInt32(x.MenuId)).ToList(),
                        //data.UserImage = m.UserImage,
                        //data.AddressLine1 = m.AddressLine1,
                        //data.AddressLine2 = m.AddressLine2,
                        //data.MobileNumber = m.MobileNumber

                        db.SaveChanges();

                    }

                    return data;
                }

            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}
