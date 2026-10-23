using FnacDarty.TechnicalTest.Library.Application.Services;
using FnacDarty.TechnicalTest.Library.Domain.Entities;
using Xunit;
public class GetRejectionReasonTests
{
    private static readonly HashSet<int> Existants = new() { 1, 2, 3 };
    private static readonly HashSet<int> Indispos = new() { 2 };

    [Fact]
    public void Available_Book_Returns_Null_Rejection_Reason()
    {
        var result = CustomerService.GetRejectionReason(1, Existants, Indispos, false, 3);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(99, false, 3, RejectionReason.NOT_FOUND)]      // inexistant
    [InlineData(2, false, 3, RejectionReason.NOT_AVAILABLE)]  // déjà emprunté
    [InlineData(1, true, 3, RejectionReason.LOAN_OVERDUE)]   // client en retard
    [InlineData(1, false, 0, RejectionReason.LIMIT_REACHED)]  // plus de place
    public void Returns_Correct_Rejection_Reason(int bookId, bool retard, int places, RejectionReason attendu)
    {
        var result = CustomerService.GetRejectionReason(bookId, Existants, Indispos, retard, places);
        Assert.Equal(attendu, result);
    }
}