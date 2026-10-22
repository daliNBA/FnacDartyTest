using FnacDarty.TechnicalTest.Library.Domain.Entities;

namespace FnacDarty.TechnicalTest.Library.Domain.Interfaces
{
    public interface ILibraryService
    {
        void AddBook(string Title, string Author);

        IReadOnlyCollection<Book> GetAllBooks();

        ResultLoan BorrowBooks(int customerId, List<int> bookIds);
    }
}