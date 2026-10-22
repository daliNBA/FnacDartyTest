namespace FnacDarty.TechnicalTest.Library.Domain.Entities
{
    public record BorrowedBook(
        int BookId,
        DateOnly DueAt
    );


    public record RejectedBook(
        int BookId,
        string Reason,
        string ReasonLabel
    );
    public record ResultLoan(
        List<BorrowedBook> BorrowedBooks,
        List<RejectedBook> RejectedBooks
    );
    public class CustomerNotFoundException : Exception { }
}
