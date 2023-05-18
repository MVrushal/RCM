using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Data.DbModel.SuperAdmin
{
   public class Database : EntityWithAudit
    {
     
      
        public string Dbconnectionstring { get; set; }


        public string DatabaseName { get; set; }



        public string UserName { get; set; }


        public string Password { get; set; }

        public string ServerAddress { get; set; }


        public string ComanyCode { get; set; }


    }
}
