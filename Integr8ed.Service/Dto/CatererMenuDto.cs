using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class CatererMenuDto : BaseModel
    {
        public long CatererId { get; set; }

        public bool IsView { get; set; }

        public List<CatererMenuItemList> catererMenuItemLists { get; set; }

    }

    public class CatererMenuItemList
    {
        public long MenuId { get; set; }
        public string MenuName { get; set; }
        public decimal Cost { get; set; }

    }


    public class CatMenuDto
    {
        public List<CatererList> CatererList { get; set; }
        public List<MenuList> Menulist { get; set; }
        public List<MenuList> AllMenuList { get; set; }
    }

    public class CatererList
    {
        public long CatererId { get; set; }
        public string CatererName { get; set; }
    }

    public class MenuList
    {
        public decimal Cost { get; set; }
        public long MenuId { get; set; }
        public string MenuName { get; set; }
    }



    public class CatererModelList
    {
        public long CatererId { get; set; }
        public string CatererName { get; set; }
        public List<catererData> CatMenuList { get; set; }
    }

    public class catererData
    {
        public long MenuId { get; set; }

        public string MenuName { get; set; }
        public decimal Cost { get; set; }

    }

    public class GetCatMenuListDto:BaseModel 
      {
        public long MenuId { get; set; }
        public long CatererId { get; set; }
        public decimal  Cost { get; set; }
        public string   DescriptionOFFood { get; set; }
        public string   CatererName { get; set; }

    }

}
