using FnacDarty.TechnicalTest.Library.Domain.Entities;

namespace FnacDarty.TechnicalTest.Library.Domain.Interfaces
{
    public interface ICustomerRepository
    {
        IReadOnlyCollection<Customer> Get();
        List<Loan> GetAllLoans();
        void AddBorrowedBooks(int customerId, List<Loan> loans);
    }
}