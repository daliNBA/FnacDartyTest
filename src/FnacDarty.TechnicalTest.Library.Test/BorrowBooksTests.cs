using FnacDarty.TechnicalTest.Library.Domain.Entities;
using FnacDarty.TechnicalTest.Library.Domain.Interfaces;
using FnacDarty.TechnicalTest.Library.Domain.Services;
using Moq;
using Xunit;

namespace FnacDarty.TechnicalTest.Library.Test
{
    public class BorrowBooksTests
    {
        [Fact]
        public void Deux_emprunts_actifs_et_trois_livres_demandes_un_seul_accorde()
        {
            // Arrange : le client 4 a déjà 2 emprunts actifs, sur les livres 10 et 11
            var today = DateTime.UtcNow.Date;
            var loans = new List<Loan>
        {
            new Loan(1, 4, 10, today, today.AddDays(21), null),
            new Loan(2, 4, 11, today, today.AddDays(21), null)
        };

            var customers = new List<Customer>{
                new Customer(4, "Alice", loans.AsReadOnly())
            };

            var customerRepo = new Mock<ICustomerRepository>();
            customerRepo.Setup(r => r.Get()).Returns(customers.AsReadOnly());
            customerRepo.Setup(r => r.GetAllLoans()).Returns(loans);

            var bookRepo = new Mock<IBookRepository>();
            bookRepo.Setup(r => r.GetAll()).Returns(new List<Book>
        {
            new Book(1, "Livre 1", "Author 1"), new Book(2, "Livre 2", "Author 2"), new Book(3, "Livre 3", "Author 3"),
            new Book(10, "Livre 10", "Author 10"), new Book(11, "Livre 11", "Author 11")
        });

            var service = new LibraryService(bookRepo.Object, customerRepo.Object);

            // Act
            var result = service.BorrowBooks(4, new List<int> { 1, 2, 3 });

            // Assert
            Assert.Single(result.BorrowedBooks);                       // 1 seul livre accordé
            Assert.Equal(1, result.BorrowedBooks[0].BookId);
            Assert.Equal(2, result.RejectedBooks.Count);
            Assert.All(result.RejectedBooks, r => Assert.Equal("LIMIT_REACHED", r.Reason));
        }
    }
}
