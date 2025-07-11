﻿namespace DUANTOTNGHIEP.Models
{
    public class Combo : BaseModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public ICollection<ComboDetail> ComboDetails { get; set; }
    }

}
