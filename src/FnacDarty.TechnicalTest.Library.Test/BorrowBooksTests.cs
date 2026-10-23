using FnacDarty.TechnicalTest.Library.Application.Services;
using FnacDarty.TechnicalTest.Library.Domain.Entities;
using FnacDarty.TechnicalTest.Library.Domain.Interfaces;
using Moq;
using Xunit;

namespace FnacDarty.TechnicalTest.Library.Test
{
    public class BorrowBooksTests
    {
        private static readonly DateTime Today = DateTime.Today;
        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly CustomerService _service;
        private readonly List<Customer> _customers = new();
        private readonly List<Loan> _loans = new();
        public BorrowBooksTests()
        {
            // Books 1 to 5 exist in the library
            _bookRepositoryMock.Setup(r => r.GetAll()).Returns(new List<Book>
            {
                new Book(1, "Book 1", "Author 1"), new Book(2, "Book 2", "Author 2"),
                new Book(3, "Book 3", "Author 3"), new Book(4, "Book 4", "Author 4"),
                new Book(5, "Book 5", "Author 5")
            });

            _customerRepositoryMock.Setup(r => r.Get()).Returns(_customers.AsReadOnly());
            _customerRepositoryMock.Setup(r => r.GetAllLoans()).Returns(_loans);

            _service = new CustomerService(_customerRepositoryMock.Object, _bookRepositoryMock.Object);
        }

        private void AddCustomer(int id, params Loan[] loans)
        {
            _loans.AddRange(loans);
            _customers.Add(new Customer(id, $"Customer {id}", loans.ToList().AsReadOnly()));
        }

        private static Loan ActiveLoan(int id, int customerId, int bookId)
            => new Loan(id, customerId, bookId, Today.AddDays(-5), Today.AddDays(16), null);

        private static Loan OverdueLoan(int id, int customerId, int bookId)
            => new Loan(id, customerId, bookId, Today.AddDays(-30), Today.AddDays(-9), null);

        private static Loan ReturnedLoan(int id, int customerId, int bookId)
            => new Loan(id, customerId, bookId, Today.AddDays(-30), Today.AddDays(-9), Today.AddDays(-2));


        // ---------- Error cases ----------

        [Fact]
        public void BorrowBooks_UnknownCustomer_ThrowsCustomerNotFoundException()
        {
            Assert.Throws<CustomerNotFoundException>(() => _service.BorrowBooks(99, new List<int> { 1 }));
        }

        [Fact]
        public void BorrowBooks_UnknownBook_IsRejectedWithNotFound()
        {
            AddCustomer(1);

            var result = _service.BorrowBooks(1, new List<int> { 99 });

            Assert.Empty(result.BorrowedBooks);
            var rejected = Assert.Single(result.RejectedBooks);
            Assert.Equal(99, rejected.BookId);
            Assert.Equal("NOT_FOUND", rejected.Reason);
        }

        [Fact]
        public void BorrowBooks_BookBorrowedByAnotherCustomer_IsRejectedWithNotAvailable()
        {
            AddCustomer(1);
            AddCustomer(2, ActiveLoan(1, customerId: 2, bookId: 1));

            var result = _service.BorrowBooks(1, new List<int> { 1 });

            Assert.Empty(result.BorrowedBooks);
            Assert.Equal("NOT_AVAILABLE", Assert.Single(result.RejectedBooks).Reason);
        }

        [Fact]
        public void BorrowBooks_SameBookRequestedTwice_SecondIsRejectedWithNotAvailable()
        {
            AddCustomer(1);

            var result = _service.BorrowBooks(1, new List<int> { 1, 1 });

            Assert.Equal(1, Assert.Single(result.BorrowedBooks).BookId);
            var rejected = Assert.Single(result.RejectedBooks);
            Assert.Equal(1, rejected.BookId);
            Assert.Equal("NOT_AVAILABLE", rejected.Reason);
        }

        [Fact]
        public void BorrowBooks_CustomerWithOverdueLoan_IsRejectedWithLoanOverdue()
        {
            AddCustomer(1, OverdueLoan(1, customerId: 1, bookId: 5));

            var result = _service.BorrowBooks(1, new List<int> { 1 });

            Assert.Empty(result.BorrowedBooks);
            Assert.Equal("LOAN_OVERDUE", Assert.Single(result.RejectedBooks).Reason);
        }

        [Fact]
        public void BorrowBooks_CustomerWithThreeActiveLoans_IsRejectedWithLimitReached()
        {
            AddCustomer(1,
                ActiveLoan(1, customerId: 1, bookId: 3),
                ActiveLoan(2, customerId: 1, bookId: 4),
                ActiveLoan(3, customerId: 1, bookId: 5));

            var result = _service.BorrowBooks(1, new List<int> { 1 });

            Assert.Empty(result.BorrowedBooks);
            Assert.Equal("LIMIT_REACHED", Assert.Single(result.RejectedBooks).Reason);
        }

        // ---------- Success cases ----------

        [Fact]
        public void BorrowBooks_CustomerWithoutLoans_AllBooksAreBorrowedForTwentyOneDays()
        {
            AddCustomer(1);

            var result = _service.BorrowBooks(1, new List<int> { 1, 2, 3 });

            Assert.Empty(result.RejectedBooks);
            Assert.Equal(new[] { 1, 2, 3 }, result.BorrowedBooks.Select(b => b.BookId));
            Assert.All(result.BorrowedBooks, b =>
                Assert.Equal(DateOnly.FromDateTime(Today.AddDays(21)), b.DueAt));
        }

        [Fact]
        public void BorrowBooks_CustomerWithTwoActiveLoans_OnlyOneBookIsBorrowed()
        {
            AddCustomer(1,
                ActiveLoan(1, customerId: 1, bookId: 4),
                ActiveLoan(2, customerId: 1, bookId: 5));

            var result = _service.BorrowBooks(1, new List<int> { 1, 2, 3 });

            Assert.Equal(1, Assert.Single(result.BorrowedBooks).BookId);
            Assert.Equal(new[] { 2, 3 }, result.RejectedBooks.Select(r => r.BookId));
            Assert.All(result.RejectedBooks, r => Assert.Equal("LIMIT_REACHED", r.Reason));
        }

        [Fact]
        public void BorrowBooks_ReturnedLoans_DoNotCountTowardsLimitOrOverdue()
        {
            // 3 old loans, one of them was late, but all are returned
            AddCustomer(1,
                ReturnedLoan(1, customerId: 1, bookId: 3),
                ReturnedLoan(2, customerId: 1, bookId: 4),
                ReturnedLoan(3, customerId: 1, bookId: 5));

            var result = _service.BorrowBooks(1, new List<int> { 1 });

            Assert.Empty(result.RejectedBooks);
            Assert.Equal(1, Assert.Single(result.BorrowedBooks).BookId);
        }

        [Fact]
        public void BorrowBooks_ReturnedBook_CanBeBorrowedAgain()
        {
            AddCustomer(1);
            AddCustomer(2, ReturnedLoan(1, customerId: 2, bookId: 1));

            var result = _service.BorrowBooks(1, new List<int> { 1 });

            Assert.Equal(1, Assert.Single(result.BorrowedBooks).BookId);
        }

        [Fact]
        public void BorrowBooks_MixedRequest_ReturnsBorrowedAndRejectedBooks()
        {
            AddCustomer(1);
            AddCustomer(2, ActiveLoan(1, customerId: 2, bookId: 2));

            var result = _service.BorrowBooks(1, new List<int> { 1, 2, 99 });

            Assert.Equal(1, Assert.Single(result.BorrowedBooks).BookId);
            Assert.Equal(2, result.RejectedBooks.Count);
            Assert.Contains(result.RejectedBooks, r => r.BookId == 2 && r.Reason == "NOT_AVAILABLE");
            Assert.Contains(result.RejectedBooks, r => r.BookId == 99 && r.Reason == "NOT_FOUND");
        }

        [Fact]
        public void BorrowBooks_Success_SavesNewLoansOnce()
        {
            AddCustomer(1);

            _service.BorrowBooks(1, new List<int> { 1, 2 });

            _customerRepositoryMock.Verify(r => r.AddBorrowedBooks(1, It.Is<List<Loan>>(l => l.Count == 2)), Times.Once);
        }
    }
}
