using FnacDarty.TechnicalTest.Library.Domain.Entities;

namespace FnacDarty.TechnicalTest.Library.Domain.Interfaces
{
    public interface ICustomerService
    {
        ResultLoan BorrowBooks(int customerId, List<int> bookIds);
    }
}
