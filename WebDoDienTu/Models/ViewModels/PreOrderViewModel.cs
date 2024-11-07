﻿namespace WebDoDienTu.Models.ViewModels
{
    public class PreOrderViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public decimal ProductPrice { get; set; }
        public int DepositAmount { get; set; }
    }
}
