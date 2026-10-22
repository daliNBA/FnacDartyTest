namespace FnacDarty.TechnicalTest.Library.Domain.Entities
{
    public class Loan
    {
        public int Id { get; }

        public int CustomerId { get; }

        public int BookId { get; }

        public DateTime BorrowedAt { get; }

        public DateTime DueAt { get; }

        public DateTime? ReturnedAt { get; }

        public Loan(int id, int customerId, int bookId, DateTime borrowedAt, DateTime dueAt, DateTime? returnedAt)
        {
            Id = id;
            CustomerId = customerId;
            BookId = bookId;
            BorrowedAt = borrowedAt;
            DueAt = dueAt;
            ReturnedAt = returnedAt;
        }
    }
}