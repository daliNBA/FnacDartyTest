namespace FnacDarty.TechnicalTest.Library.Domain.Entities
{
    public class Customer
    {
        public int Id { get; }

        public string Name { get; }

        public IReadOnlyCollection<Loan> BorrowedBooks { get; } = new List<Loan>();

        public Customer(int id, string name, IReadOnlyCollection<Loan> borrowedBooks)
        {
            Id = id;
            Name = name;
            BorrowedBooks = borrowedBooks;
        }
    }
}