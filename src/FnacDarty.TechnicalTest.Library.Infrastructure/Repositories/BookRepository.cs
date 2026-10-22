using FnacDarty.TechnicalTest.Library.Domain.Entities;
using FnacDarty.TechnicalTest.Library.Domain.Interfaces;

namespace FnacDarty.TechnicalTest.Library.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        public List<Book> Books;

        public BookRepository()
        {
            Books = new List<Book>
            {
                new Book(1, "Da Vinci Code", "Dan Brown"),
                new Book(2, "Dune", "Frank Herbert"),
                new Book(3, "Le Petit Prince", "Antoine de Saint-Exupéry"),
                new Book(4, "Les Misérables", "Victor Hugo"),
                new Book(5, "Le Seigneur des Anneaux", "J.R.R. Tolkien"),
                new Book(6, "L'Etranger", "Albert Camus"),
                new Book(7, "1984", "George Orwell"),
                new Book(8, "Gatsby le Magnifique", "F. Scott Fitzgerald"),
                new Book(9, "Harry Potter à l'école des sorciers", "J.K. Rowling")
            };
        }

        public IReadOnlyCollection<Book> GetAll()
        {
            return Books.AsReadOnly();
        }

        public void Add(Book book)
        {
            Books.Add(book);
        }
    }
}