using FnacDarty.TechnicalTest.Library.Domain.Entities;
using FnacDarty.TechnicalTest.Library.Domain.Interfaces;

namespace FnacDarty.TechnicalTest.Library.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        public List<Customer> Customers;

        public CustomerRepository()
        {
            var today = DateTime.UtcNow.Date;

            Customers = new List<Customer>
            {
                new Customer(1, "Alice Martin", new List<Loan>
                {
                    new Loan(
                        id: 1,
                        customerId: 1,
                        bookId: 1,
                        borrowedAt: today.AddDays(-30),
                        dueAt: today.AddDays(-9),
                        returnedAt: null),
                    new Loan(
                        id: 5,
                        customerId: 1,
                        bookId: 9,
                        borrowedAt: today.AddDays(-15),
                        dueAt: today.AddDays(6),
                        returnedAt: today.AddDays(10)),
                    new Loan(
                        id: 6,
                        customerId: 1,
                        bookId: 2,
                        borrowedAt: today.AddDays(-15),
                        dueAt: today.AddDays(6),
                        returnedAt: null),
                    new Loan(
                        id: 7,
                        customerId: 1,
                        bookId: 4,
                        borrowedAt: today.AddDays(-10),
                        dueAt: today.AddDays(11),
                        returnedAt: null)
                }.ToList()),

                new Customer(2, "Bob Durand", new List<Loan>
                {
                    new Loan(
                        id: 3,
                        customerId: 2,
                        bookId: 3,
                        borrowedAt: today.AddDays(-18),
                        dueAt: today.AddDays(3),
                        returnedAt: today.AddDays(-10))
                }.ToList()),

                new Customer(3, "Chloé Bernard", new List<Loan>
                {
                    new Loan(
                        id: 2,
                        customerId: 3,
                        bookId: 8,
                        borrowedAt: today.AddDays(-30),
                        dueAt: today.AddDays(-9),
                        returnedAt: null)
                }.ToList()),

                new Customer(4, "David Lefèvre", new List<Loan>
                {
                    new Loan(
                        id: 4,
                        customerId: 4,
                        bookId: 7,
                        borrowedAt: today.AddDays(-15),
                        dueAt: today.AddDays(6),
                        returnedAt: null)
                }.ToList())
            };
        }

        public IReadOnlyCollection<Customer> Get()
        {
            return Customers.AsReadOnly();
        }

        //ajouter la liste des emprunts pour un client donné
        public void AddBorrowedBooks(int customerId, List<Loan> loans)
        {
            var customer = Customers.FirstOrDefault(c => c.Id == customerId);
            customer = customer ?? throw new CustomerNotFoundException();
            var updatedLoans = customer.BorrowedBooks.Concat(loans).ToList().AsReadOnly();
            var updatedCustomer = new Customer(customer.Id, customer.Name, updatedLoans);
            //pour ne pas modifier la liste originale, on remplace l'objet Customer par un nouveau avec la liste mise à jour
            var index = Customers.IndexOf(customer);
            Customers[index] = updatedCustomer;
        }

        //obtenir la liste des emprunts pour tous les clients
        public List<Loan> GetAllLoans()
        {
            return Customers.SelectMany(c => c.BorrowedBooks).ToList();
        }
    }
}