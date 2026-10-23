using FnacDarty.TechnicalTest.Library.Domain.Entities;
using FnacDarty.TechnicalTest.Library.Domain.Interfaces;

namespace FnacDarty.TechnicalTest.Library.Application.Services
{
    public class CustomerService : ICustomerService
    {
        const int MaxActiveLoans = 3;
        const int LoanDurationDays = 21;
        private readonly     ICustomerRepository _customerRepository;
        private readonly IBookRepository _bookRepository;
        public CustomerService(ICustomerRepository customerRepository, IBookRepository bookRepository)
        {
            _customerRepository = customerRepository;
            _bookRepository = bookRepository;
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

        public static RejectionReason? GetRejectionReason(int bookId, HashSet<int> existingBookIds,
            HashSet<int> unavailableBookIds, bool hasOverdueLoan, int remainingSlots)
        {
            if (!existingBookIds.Contains(bookId)) return RejectionReason.NOT_FOUND;
            if (unavailableBookIds.Contains(bookId)) return RejectionReason.NOT_AVAILABLE;
            if (hasOverdueLoan) return RejectionReason.LOAN_OVERDUE;
            if (remainingSlots <= 0) return RejectionReason.LIMIT_REACHED;
            return null;
        }

        public static string GetLabel(RejectionReason reason) => reason switch
        {
            RejectionReason.NOT_FOUND => "Le livre est introuvable",
            RejectionReason.NOT_AVAILABLE => "Le livre n'est pas disponible actuellement",
            RejectionReason.LOAN_OVERDUE => "La date d'échéance d'un prêt est dépassée",
            RejectionReason.LIMIT_REACHED => "Le client a atteint la limite de 3 emprunts simultanés",
            _ => throw new ArgumentOutOfRangeException(nameof(reason))
        };
    }
}
