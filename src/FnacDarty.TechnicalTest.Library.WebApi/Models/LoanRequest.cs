namespace FnacDarty.TechnicalTest.Library.WebApi.Models
{
    public record LoanRequest(int CustomerId, List<int> BookIds);
}
