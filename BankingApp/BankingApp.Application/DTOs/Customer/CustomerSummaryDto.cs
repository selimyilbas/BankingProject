namespace BankingApp.Application.DTOs.Customer
{
    public class CustomerSummaryDto
    {
        public int CustomerId { get; set; }
        public string CustomerNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
