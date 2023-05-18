
using Integr8ed.Data.DbModel.ClientAdmin;

using Microsoft.EntityFrameworkCore;

namespace Integr8ed.Data.DbContext.ClientAdminContext
{
   public class ClientAdminDbContext: Microsoft.EntityFrameworkCore.DbContext
    {
      

        public ClientAdminDbContext()
        {
        }

        public ClientAdminDbContext(DbContextOptions<ClientAdminDbContext> options)
            : base(options)
        {

            
        }
   
        public DbSet<Users> Users { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<Room_Type> Room_Types{ get; set; }
        public DbSet<BookingStatus> BookingStatus { get; set; }
        public DbSet<RoomImageMaster> RoomImageMaster { get; set; }
        public DbSet<StandardEquipment> StandardEquipment  { get; set; }
        public DbSet<MeetingType> MeetingTypes { get; set; }
        public DbSet<CallLogs> CallLogs { get; set; }
        public DbSet<Catering_Details> Catering_Details { get; set; }
        public DbSet<Entry_Type> Entry_Types { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserAccess> UserAccess { get; set; }
        public DbSet<Visitors> Visitors { get; set; }
        public DbSet<Catering_Requirements> Catering_Requirements { get; set; }
        public DbSet<BookingDetails> BookingDetails { get; set; }
        public DbSet<BookingRequest> BookingRequest { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetail { get; set; }
        public DbSet<ItemsToInvoice> ItemsToInvoice { get; set; }
        public DbSet<Security> Security { get; set; }
        public DbSet<Cat_Req_Menu> Cat_Req_Menu { get; set; }
        public DbSet<Notes> Notes { get; set; }
        public DbSet<ContactDetails> ContactDetails { get; set; }
        public DbSet<CatererMenu> CatererMenu { get; set; }
        public DbSet<VisitorBooking> VisitorBooking { get; set; }
        public DbSet<EquipmentRequiredForBooking> EquipmentRequiredForBooking { get; set; }
        public DbSet<RecurringBookings> RecurringBookings { get; set; }
        public DbSet<BranchMaster> BranchMaster { get; set; }
        public DbSet<Roles> Roles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           // optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }
    }
}
