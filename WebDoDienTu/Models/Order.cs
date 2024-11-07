using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDoDienTu.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [DisplayName("Ngày đặt")]
        public DateTime OrderDate { get; set; }

        [DisplayName("Tổng tiền")]
        public decimal TotalPrice { get; set; }

        [DisplayName("Số tiền giảm giá")]
        public decimal DiscountAmount { get; set; } = 0;

        [DisplayName("Trạng thái")]
        public OrderStatus Status { get; set; }

        [DisplayName("Họ")]
        public string FirstName { get; set; }

        [DisplayName("Tên")]
        public string LastName { get; set; }

        [DisplayName("Số điện thoại")]
        public string Phone { get; set; }

        public string Email { get; set; }

        [DisplayName("Địa chỉ")]
        public string Address { get; set; }

        [DisplayName("Ghi chú")]
        public string? Notes { get; set; }
        public int? PromotionId { get; set; }
        public decimal DepositAmount { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]

        public ApplicationUser? ApplicationUser { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }     
        public Promotion? Promotion { get; set; }
        public ICollection<OrderComplaint>? Complaints { get; set; }
    }
}
