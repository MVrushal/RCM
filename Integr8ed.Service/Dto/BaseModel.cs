using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Integr8ed.Service
{
  public  class BaseModel
    {
        public long Id { get; set; }

        public long? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string DateCreated { get; set; }

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }

        // public bool EmailConfirmed { get; set; }

        public long TotalRecords { get; set; }
    }
    public class DropdownModel
    {
        public long Id { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class RoleDropdownModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class MultiSelectDropdownModel
    {
        public string label { get; set; }
        public string value { get; set; }
    }
}
