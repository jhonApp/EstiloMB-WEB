namespace EstiloMB.Site.Controllers
{
    public class PaymentRequest
    {
        public string PaymentMethodId { get; set; }
        public string IssuerId { get; set; }
        public string Email { get; set; }
        public double TransactionAmount { get; set; }
        public string Description { get; set; }
        public string Amount { get; set; }
        public string Token { get; set; }
        public double Installments { get; set; }
        public string IdentificationNumber { get; set; }
        public string IdentificationType { get; set; }
        
    }
}