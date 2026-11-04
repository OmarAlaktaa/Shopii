namespace Shopii.Application.Features.Orders.DTOs
{
    public class PaymentDTO
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }

        public int PaymentMethodId { get; set; }
        public string PaymentMethodTitle { get; set; } = null!;

        public int PaymentStatusId { get; set; }
        public string PaymentStatusTitle { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
