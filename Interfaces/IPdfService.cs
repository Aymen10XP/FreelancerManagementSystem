using FreelancerManagementSystem.Models;

namespace FreelancerManagementSystem.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateInvoicePdf(Invoice invoice);
        byte[] GenerateContractPdf(Contract contract);
    }
}
