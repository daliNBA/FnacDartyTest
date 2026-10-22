using FnacDarty.TechnicalTest.Library.Domain.Entities;
using FnacDarty.TechnicalTest.Library.Domain.Interfaces;
using FnacDarty.TechnicalTest.Library.Domain.Services;
using Moq;
using Xunit;

namespace FnacDarty.TechnicalTest.Library.Test
{
    public class AddBookTests
    {
        [Fact]
        public void Ajouter_un_livre_appelle_Add_avec_le_bon_titre_et_auteur()
        {
            // Arrange : 5 livres existent déjà
            var books = new List<Book>
        {
            new Book(1, "Livre 1", "Author 1"), new Book(2, "Livre 2", "Author 2"),
            new Book(3, "Livre 3", "Author 3"), new Book(10, "Livre 10", "Author 10"),
            new Book(11, "Livre 11", "Author 11")
        };

            var customerRepo = new Mock<ICustomerRepository>();
            var bookRepo = new Mock<IBookRepository>();
            bookRepo.Setup(r => r.GetAll()).Returns(books);

            var service = new LibraryService(bookRepo.Object, customerRepo.Object);

            // Act
            service.AddBook("Livre 4", "Author 4");

            // Assert :
            bookRepo.Verify(r => r.Add(It.Is<Book>(b =>
                b.Title == "Livre 4" &&
                b.Author == "Author 4")), Times.Once);
        }
    }
}


