using FnacDarty.TechnicalTest.Library.Domain.Entities;
using FnacDarty.TechnicalTest.Library.Domain.Services;
using Xunit;
public class GetRejectionReasonTests
{
    private static readonly HashSet<int> Existants = new() { 1, 2, 3 };
    private static readonly HashSet<int> Indispos = new() { 2 };

    [Fact]
    public void Livre_libre_et_client_en_regle_est_accorde()
    {
        var result = LibraryService.GetRejectionReason(1, Existants, Indispos, false, 3);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(99, false, 3, StatutCommande.NOT_FOUND)]      // inexistant
    [InlineData(2, false, 3, StatutCommande.NOT_AVAILABLE)]  // déjà emprunté
    [InlineData(1, true, 3, StatutCommande.LOAN_OVERDUE)]   // client en retard
    [InlineData(1, false, 0, StatutCommande.LIMIT_REACHED)]  // plus de place
    public void Renvoie_le_bon_motif(int bookId, bool retard, int places, StatutCommande attendu)
    {
        var result = LibraryService.GetRejectionReason(bookId, Existants, Indispos, retard, places);
        Assert.Equal(attendu, result);
    }
}