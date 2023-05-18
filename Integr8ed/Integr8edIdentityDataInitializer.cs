using Integr8ed.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integr8ed
{
    public class Integr8edIdentityDataInitializer
    {
        public static void SeedData(UserManager<ApplicationUser> userManager, RoleManager<Role> roleManager)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager);
        }

        private static void SeedUsers(UserManager<ApplicationUser> userManager)
        {
            var superAdmin = new ApplicationUser
            {
                FirstName = "Admin",
                LastName = "",
                MiddleName = "",
                UserName = "admin@system.com",
                MobileNumber = "+1 999-999-9999",
                NormalizedUserName = "Admin",
                Email = "admin@system.com",
                NormalizedEmail = "admin@system.com",
                EmailConfirmed = true,
                IsActive = true,
                AddressLine1 = "Integr8ed systems ltd,Victoria House Woodwared Road L33 7UY",

            };
            if (userManager.FindByEmailAsync(superAdmin.UserName).Result != null) return;

            var result = userManager.CreateAsync(superAdmin, "Admin@123").Result;

            if (result.Succeeded)
            {
                userManager.AddToRoleAsync(superAdmin, "SuperAdmin").Wait();
            }


            var SystemSuperAdmin = new ApplicationUser
            {
                FirstName = "Admin",
                LastName = "",
                MiddleName = "",
                UserName = "admin@system.com",
                MobileNumber = "+1 999-999-9999",
                NormalizedUserName = "Admin",
                Email = "admin@system.com",
                NormalizedEmail = "admin@system.com",
                EmailConfirmed = true,
                IsActive = true,
                AddressLine1 = "Integr8ed systems ltd,Victoria House Woodwared Road L33 7UY",

            };
            if (userManager.FindByEmailAsync(superAdmin.UserName).Result != null) return;

            var AdminResult = userManager.CreateAsync(superAdmin, "Admin@123").Result;

            if (AdminResult.Succeeded)
            {
                userManager.AddToRoleAsync(SystemSuperAdmin, "SuperAdmin").Wait();
            }





            var clientAdmin = new ApplicationUser
            {
                FirstName = "Karishma",
                LastName = "Narola",
                MiddleName = "",
                UserName = "kdk@narola.email",
                MobileNumber = "+1 999-999-9999",
                NormalizedUserName = "user",
                Email = "kdk@narola.email",
                NormalizedEmail = "kdk@narola.email",
                EmailConfirmed = true,
                IsActive = true,
                AddressLine1 = "Surat",
            };
            if (userManager.FindByEmailAsync(clientAdmin.UserName).Result != null) return;
            var developeResult = userManager.CreateAsync(clientAdmin, "Password123#").Result;

            if (developeResult.Succeeded)
            {
                userManager.AddToRoleAsync(clientAdmin, "ClientAdmin").Wait();
            }

            var staff = new ApplicationUser
            {
                FirstName = "Parita",
                LastName = "Narola",
                MiddleName = "",
                UserName = "pk@narola.email",
                MobileNumber = "+1 999-999-9999",
                NormalizedUserName = "user",
                Email = "pk@narola.email",
                NormalizedEmail = "pk@narola.email",
                EmailConfirmed = true,
                IsActive = true,
                AddressLine1 = "Surat",
            };
            if (userManager.FindByEmailAsync(staff.UserName).Result != null) return;
            var staffResult = userManager.CreateAsync(staff, "Password123#").Result;

            if (staffResult.Succeeded)
            {
                userManager.AddToRoleAsync(staff, "Staff").Wait();
            }


        }

        private static void SeedRoles(RoleManager<Role> roleManager)
        {
            #region User Roles
            Dictionary<string, string> normalizedName = new Dictionary<string, string>
            {
                { "SuperAdmin", "SuperAdmin"},
                { "ClientAdmin", "ClientAdmin"},
                { "Staff", "Staff"},
            };

            var existrolesList = roleManager.Roles.Select(x => x.Name).ToList();
            if (existrolesList.Any())
            {
                var notExirst = normalizedName.Keys.Except(existrolesList);
                foreach (var notRole in notExirst)
                {
                    string normalized = normalizedName.FirstOrDefault(x => x.Key == notRole).Value;
                    var roleResult = roleManager.CreateAsync(new Role { Name = notRole, NormalizedName = normalized, DisplayRoleName = normalized }).Result;
                }
            }
            else
            {
                foreach (var objRole in normalizedName.Keys)
                {
                    string normalized = normalizedName.FirstOrDefault(x => x.Key == objRole).Value;
                    IdentityResult roleResult = roleManager.CreateAsync(new Role { Name = objRole, NormalizedName = normalized, DisplayRoleName = normalized }).Result;
                }
            }
            #endregion
        }
    }
}
