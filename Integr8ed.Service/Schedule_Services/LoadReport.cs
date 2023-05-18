using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Integr8ed.Data.DbContext;
using Integr8ed.Service.BackGroundService;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Enums;
using Integr8ed.Service.Implementation;
using Integr8ed.Service.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integr8ed.Service.Interface.ClientAdmin;
using Microsoft.Extensions.Configuration;

namespace Integr8ed.Service.Schedule_Services
{
    public class LoadReport
    {
        // private readonly UserManager<ApplicationUser> _userManager;




        private static StoredProcedureRepositoryBase _repositoryBase;
        private static SchedulerEmailService _schedulerEmail;
        private IConfiguration _config;

        public LoadReport(IConfiguration configuration)
        {
            _repositoryBase = new StoredProcedureRepositoryBase();

            //// Narola Email configuration
            _schedulerEmail = new SchedulerEmailService(new EmailSettingsGmail
            {
                SmtpUserName = "ams@narola.email",
                SmtpHost = "smtp.1and1.com",
                SmtpSenderEmail = "ams@narola.email",
                SmtpPassword = "yWRcXid2UYNyZoY",
                SmtpPort = 587,
                SenderName = "Integr8ed",
                SmtpEnableSsl = true
            });


            //Richard Email Configuration
            //_schedulerEmail = new SchedulerEmailService(new EmailSettingsGmail
            //{
            //    SmtpUserName = "roomconferencemanager@gmail.com",
            //    SmtpHost = "smtp.gmail.com",
            //    SmtpSenderEmail = "roomconferencemanager@gmail.com",
            //    SmtpPassword = "HU66$X1@",
            //    SmtpPort = 587,
            //    SenderName = "Integr8ed",
            //    SmtpEnableSsl = true
            //});
            _config = configuration;
        }


        public async Task<Task> DownloadReport()
        {
            try
            {
                string qry = StoredProcedureList.GetDBwiseBookingNotifyList;
                DataSet result = _repositoryBase.GetQueryDatatableAsync(qry, null, CommandType.StoredProcedure);
                var bookingList = _repositoryBase.CreateListFromTable<BookingNotifyDto>(result.Tables[0]).ToList();

               
                foreach (var booking in bookingList)
                {
                    var resetUrl = _config["CommonProperty:PhysicalUrl"];
                    var confirmationUrl = _config["CommonProperty:PhysicalUrl"] + "/ClientAdmin/BookingNotification/BookingStatusMessage?IsConfirmed=true&id=" + booking.Id + "&CurrentComCode="+booking.CurrentComCode;
                    var cancelationUrl = _config["CommonProperty:PhysicalUrl"] + "/ClientAdmin/BookingNotification/BookingStatusMessage?IsConfirmed=false&id=" + booking.Id + "&CurrentComCode=" + booking.CurrentComCode;
                    string EmailTemplate = _schedulerEmail.ReadEmailTemplate(@"wwwroot\EmailTemplate", "ConfirmBookingNotify.html", resetUrl);
                    EmailTemplate = EmailTemplate.Replace("{CancelationURL}", cancelationUrl);
                    EmailTemplate = EmailTemplate.Replace("{ConfirmURL}", confirmationUrl);
                    EmailTemplate = EmailTemplate.Replace("{UserName}", booking.FirstName + "  " + booking.LastName);
                    EmailTemplate = EmailTemplate.Replace("{RoomType}", booking.RoomTitle);
                    EmailTemplate = EmailTemplate.Replace("{AdminEmail}", booking.Email);
                    EmailTemplate = EmailTemplate.Replace("{BookingDate}", booking.BookingDate + " From : " + booking.StartTime + "  To: " + booking.FinishTime);


                    await Task.Run(() => _schedulerEmail.SendEmailAsyncByGmail(new BackGroundService.SendEmailModel()
                    {
                        BodyText = EmailTemplate,
                        Subject = "Integr8ed Room Booking Notify",
                        ToAddress = booking.Email,
                        ToDisplayName = "Dear ," + booking.FirstName
                    }));
                }
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.CompletedTask;
            }
        }
    }
}
