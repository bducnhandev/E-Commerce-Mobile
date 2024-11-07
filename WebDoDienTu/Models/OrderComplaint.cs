namespace WebDoDienTu.Models
{
    public class OrderComplaint
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string ComplaintDescription { get; set; } = string.Empty;
        public DateTime ComplaintDate { get; set; } = DateTime.Now;
        public OrderComplaintStatus Status { get; set; } = OrderComplaintStatus.Pending;
    }
}
