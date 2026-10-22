using FnacDarty.TechnicalTest.Library.Domain.Entities;
using FnacDarty.TechnicalTest.Library.Domain.Interfaces;

namespace FnacDarty.TechnicalTest.Library.Domain.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICustomerRepository _customerRepository;
        private const int MaxActiveLoans = 3;
        private const int LoanDurationDays = 21;


        public LibraryService(IBookRepository bookRepository, ICustomerRepository customerRepository)
        {

            _bookRepository = bookRepository;
            _customerRepository = customerRepository;
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


        public ResultLoan BorrowBooks(int customerId, List<int> bookIds)
        {
            if (_customerRepository.Get() is null)
                throw new CustomerNotFoundException();
            var today = DateTime.UtcNow.Date;

            
            var existingBookIds = _bookRepository.GetAll().Select(b => b.Id).ToHashSet();
            var allLoans = _customerRepository.GetAllLoans();
            var activeLoans = allLoans.Where(l => l.ReturnedAt == null).ToList();

            var unavailableBookIds = activeLoans.Select(l => l.BookId).ToHashSet();
            var customerActive = activeLoans.Where(l => l.CustomerId == customerId).ToList();
            var hasOverdueLoan = customerActive.Any(l => l.DueAt < today);
            var remainingSlots = MaxActiveLoans - customerActive.Count;
            var nextLoanId = allLoans.Count == 0 ? 1 : allLoans.Max(l => l.Id) + 1;

            var newLoans = new List<Loan>();
            var borrowed = new List<BorrowedBook>();
            var rejected = new List<RejectedBook>();

            // chaque livre passe les 4 contrôles dans l'ordre
            foreach (var bookId in bookIds)
            {
                var reason = GetRejectionReason(bookId, existingBookIds, unavailableBookIds,
                                                hasOverdueLoan, remainingSlots);
                if (reason is not null)
                {
                    rejected.Add(new RejectedBook(bookId, reason.Value.ToString(), GetLabel(reason.Value)));
                    continue;
                }

                var dueAt = today.AddDays(LoanDurationDays);
                newLoans.Add(new Loan(nextLoanId++, customerId, bookId, today, dueAt, null));
                borrowed.Add(new BorrowedBook(bookId, DateOnly.FromDateTime(dueAt)));

                unavailableBookIds.Add(bookId);   // le même livre demandé 2 fois → NOT_AVAILABLE
                remainingSlots--;                 // la limite tient compte des prêts de la requête
            }

            //Sauvegarder
            if (newLoans.Count > 0)
                _customerRepository.AddBorrowedBooks(customerId, newLoans);

            return new ResultLoan(borrowed, rejected);
        }

        public static StatutCommande? GetRejectionReason(int bookId, HashSet<int> existingBookIds,
            HashSet<int> unavailableBookIds, bool hasOverdueLoan, int remainingSlots)
        {
            if (!existingBookIds.Contains(bookId)) return StatutCommande.NOT_FOUND;
            if (unavailableBookIds.Contains(bookId)) return StatutCommande.NOT_AVAILABLE;
            if (hasOverdueLoan) return StatutCommande.LOAN_OVERDUE;
            if (remainingSlots <= 0) return StatutCommande.LIMIT_REACHED;
            return null;
        }

        public static string GetLabel(StatutCommande reason) => reason switch
        {
            StatutCommande.NOT_FOUND => "Le livre est introuvable",
            StatutCommande.NOT_AVAILABLE => "Le livre n'est pas disponible actuellement",
            StatutCommande.LOAN_OVERDUE => "La date d'échéance d'un prêt est dépassée",
            StatutCommande.LIMIT_REACHED => "Le client a atteint la limite de 3 emprunts simultanés",
            _ => throw new ArgumentOutOfRangeException(nameof(reason))
        };
    }
}
