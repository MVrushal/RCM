using System.Collections.Generic;

namespace Integr8ed.Service.Dto
{
    public class Catering_RequirementsDto : BaseModel
    {
        public long CatererId { get; set; }
        public long CatererMenuId { get; set; }
        public string Notes { get; set; }
        public string TimeFor { get; set; }
        public string TimeCollected { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal Cost { get; set; }
        public bool IsView { get; set; }
        public string CatererName { get; set; }
        public long? BookingDetailId { get; set; }
        public decimal TotalCateringCost { get; set; }
        public string MenuId { get; set; }
        public List<Cat_Req_MenuDto> Cat_Req_Menu { get; set; }
        public string MenuItem { get; set; }
        public List<MenuItemAndCost> MenuItemAndCosts { get; set; }
    }

    public class MenuItemAndCost
    {
        public string Menu { get; set; }
    }
}
