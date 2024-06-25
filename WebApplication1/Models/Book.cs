namespace Bookstore.Models
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }

        public Book(Guid id, string title, string author)
        {
            Id = id;
            Title = title;
            Author = author;
        }
    }
}
