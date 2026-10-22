namespace FnacDarty.TechnicalTest.Library.Models
{
    public class AddBookRequest
    {
        public string Title { get; }

        public string Author { get; }
        
        public AddBookRequest(string title, string author)
        {
            Title = title;
            Author = author;
        }
    }
}