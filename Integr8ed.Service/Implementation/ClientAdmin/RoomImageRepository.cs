using Integr8ed.Data.DbContext.ClientAdminContext;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Service.Implementation.BaseService;
using Integr8ed.Service.Interface.ClientAdmin;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Implementation.ClientAdmin
{
   public class RoomImageRepository : ClientAdminGenericRepository<RoomImageMaster>, IRoomImageService
    {
        private readonly ClientAdminDbContext _context;
        public RoomImageRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

     
    }
}

