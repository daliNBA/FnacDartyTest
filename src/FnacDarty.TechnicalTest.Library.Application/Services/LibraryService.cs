using FnacDarty.TechnicalTest.Library.Domain.Entities;
using FnacDarty.TechnicalTest.Library.Domain.Interfaces;

namespace FnacDarty.TechnicalTest.Library.Domain.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly IBookRepository _bookRepository;

        public LibraryService(IBookRepository bookRepository)
        {

            _bookRepository = bookRepository;
        }

        public IReadOnlyCollection<Book> GetAllBooks()
        {
            return _bookRepository.GetAll();
        }

        public void AddBook(string title, string author)
        {
            var allBooks = _bookRepository.GetAll();
            var id = allBooks.Count == 0 ? 1 : allBooks.Max(b => b.Id) + 1;
            var book = new Book(id, title, author);

            _bookRepository.Add(book);
        }
    }
}
