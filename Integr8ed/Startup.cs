using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Integr8ed.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Integr8ed.Service.Interface;
using Integr8ed.Service.Implementation;
using Integr8ed.Data.DbContext.ClientAdminContext;
using Integr8ed.Service.Interface.ClientAdmin;
using Integr8ed.Service.Implementation.ClientAdmin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Integr8ed.Service.Utility;
using Rotativa.AspNetCore;
using Quartz.Impl;
using Quartz.Spi;
using Integr8ed.Service.Schedule_Services;
using Quartz;

namespace Integr8ed
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddControllersWithViews();
            //services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton(Configuration);
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Integr8edContext")));
            services.AddDbContext<ClientAdminDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ClientAdminContext")));
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //services.ini
            services.AddIdentity<ApplicationUser, Role>()
                .AddRoles<Role>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<RoleManager<Role>>();

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ClaimsPrincipalFactory>();

            #region Identity Configuration
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = false;
            });

            //Seting the Account Login page
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Identity/Account/Login"; // If the LoginPath is not set here, ASP.NET Core will default to /Identity/Account/Login
                options.LogoutPath = "/Identity/Account/Logout"; // If the LogoutPath is not set here, ASP.NET Core will default to /Account/Logout
                options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // If the AccessDeniedPath is not set here, ASP.NET Core will default to /Account/AccessDenied
                options.SlidingExpiration = true;
            });
            //Seting the Post Configure
            services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
            {
                //configure your other properties
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Identity/Account/Login"; // If the LoginPath is not set here, ASP.NET Core will default to /Identity/Account/Login
                options.LogoutPath = "/Identity/Account/Logout"; // If the LogoutPath is not set here, ASP.NET Core will default to /Account/Logout
                options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // If the AccessDeniedPath is not set here, ASP.NET Core will default to /Account/AccessDenied
                options.SlidingExpiration = true;
            });
            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
            });
            #endregion

            #region Scheduler Configuration
            var scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();
            services.AddSingleton(scheduler);

            services.AddSingleton<IJobFactory, Integr8ed_JOBFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<NotificationJob>();

            services.AddSingleton(new JobMetadata(Guid.NewGuid(), typeof(NotificationJob), "Notification Job", "0 0 0/1 1/1 * ? *")); // For Every hour
          //  services.AddSingleton(new JobMetadata(Guid.NewGuid(), typeof(NotificationJob), "Notification Job", "0 0 12 * * ? *"));//For 12:00 AM daily. 
            services.AddHostedService<Integr8ed_HostedService>();

            #endregion


            services.AddAuthentication
            (CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = Configuration["Jwt:Issuer"],
                       ValidAudience = Configuration["Jwt:Issuer"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                   };
               });

            #region Dependency Injection

            #region B

            services.AddScoped<IBookingDetailServices, BookingDetailRepositroy>();
            services.AddScoped<IbookingRequestService, BookingRequestRepository>();
            services.AddScoped<IBranchService, BranchRepository>();
            services.AddScoped<IBookingNotificationServices, BookingNotificationRepository>();
            services.AddScoped<IBookingStatusService, BookingStatusRepository>();

            #endregion

            #region C      
            
            services.AddScoped<ICompnayService, CompanyRepository>();
            services.AddScoped<IClientAdminLogin, ClientAdminLoginRepository>();
            services.AddScoped<ICompanyUserService, CompanyUserRepository>();
            services.AddScoped<ICateringDetailsServices, CateringDetailsRepository>();
            services.AddScoped<ICallLogsServices, CallLogsRepository>();
            services.AddScoped<ICatering_RequirementsServices, Catering_RequirementsRepository>();
            services.AddScoped<ICat_Req_MenuServices, Cat_Req_MenuRepository>();
            services.AddScoped<IContactDetailsServices, ContactDetailsRepository>();
            services.AddScoped<ICatererMenuServices, CatererMenuRepository>();
            services.AddScoped<IClientAdminDashboardServices, ClientAdminDashboardRepository>();

            #endregion

            #region D

            services.AddScoped<IDatabaseService, DatabaseRepository>();
            services.AddScoped<IVisitorsService, VisitorsRepository>();

            #endregion

            #region E

            services.AddScoped<IEquipServices, EquipmentRepository>();
            services.AddScoped<IEntryTypeServices, EntryTypeRepository>();
            services.AddScoped<EmailService, EmailService>();
            services.AddScoped<IEquipmentRequiredForBookingServices, EquipmentRequiredForBookingRepository>();
            services.AddScoped<IErrorLogService, ErrorLogRepository>();

            #endregion

            #region I

            services.AddScoped<IInvoiceServices, InvoiceRepository>();
            services.AddScoped<IInvoiceDetailServices, InvoiceDetailRepository>();
            services.AddScoped<IItemsToInvoiceServices, ItemsToInvoiceRepository>();

            #endregion

            #region M

            services.AddScoped<IMeetingTypeServices, MeetingTypeRepository>();
            services.AddScoped<IMenuServices, MenuRepository>();

            #endregion

            #region N

            services.AddScoped<INotesServices, NotesRepository>();

            #endregion

            #region P

            services.AddScoped<IProfileServices, ProfileRepository>();

            #endregion

            #region R

            services.AddScoped<IRoomTypesService, RoomTypesRepository>();
            services.AddScoped<IRecurringBookingServices, RecurringBookingRepository>();
            services.AddScoped<IReportsService, ReportsRepository>();
            services.AddScoped<IRoomImageService, RoomImageRepository>();

            #endregion

            #region S

            services.AddScoped<ISecurityServices, SecurityRepository>();
            services.AddScoped<ISuparAdminUserService, SuparAdminRepository>();
            services.AddScoped<IStandardEquipmentServices, StandardEquipmentRepository>();

            #endregion

            #region U

            services.AddScoped<IUserService, UserRepository>();
            services.AddScoped<IUserGroupServices, UserGroupRepository>();
            services.AddScoped<IUserAccessService, UserAccessRepository>();

            #endregion

            #region V

            services.AddScoped<IVisitorBookingServices, VisitorBookingRepository>();

            #endregion

            #endregion


            services.AddMvc();
            services.AddSession();
            services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
            /**Add Automapper**/
            services.AddAutoMapper(typeof(Startup));
            services.AddSession(opts =>
            {
                opts.Cookie.IsEssential = true; // make the session cookie Essential
            });

            //set login as default page
            services.AddMvc()
             .AddRazorPagesOptions(options =>
             {
                 options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "");
             })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddSessionStateTempDataProvider();
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            services.AddSession();

            #region Configure App Settings

            /**Email Settings**/
            services.Configure<EmailSettingsGmail>(Configuration.GetSection("EmailSettingsGmail"));

            /**Settings**/
            #endregion


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, UserManager<ApplicationUser> userManager, RoleManager<Role> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapAreaControllerRoute(
                   name: "ClientAdmin",
                   areaName: "ClientAdmin",
                   pattern: "ClientAdmin/{controller=Dashboard}/{action=Index}/{id?}");

                endpoints.MapAreaControllerRoute(
                   name: "User",
                   areaName: "User",
                   pattern: "User/{controller=External}/{action=Index}/{id?}");
                endpoints.MapAreaControllerRoute(
                   name: "SuperAdmin",
                   areaName: "SuperAdmin",
                   pattern: "SuperAdmin/{controller=Company}/{action=Index}/{id?}");
                endpoints.MapAreaControllerRoute(
                   name: "Default",
                   areaName: "Identity",
                   pattern: "{controller=External}/{action=Index}/{id?}");
                endpoints.MapRazorPages();

                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Home}/{action=Index}/{id?}");
                //endpoints.MapRazorPages();
            });
            Integr8edIdentityDataInitializer.SeedData(userManager, roleManager);
            RotativaConfiguration.Setup(env);
        }
    }
}
