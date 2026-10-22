using FnacDarty.TechnicalTest.Library.Domain.Entities;

namespace FnacDarty.TechnicalTest.Library.Domain.Interfaces
{
    public interface IBookRepository
    {
        IReadOnlyCollection<Book> GetAll();

        void Add(Book book);
    }
}