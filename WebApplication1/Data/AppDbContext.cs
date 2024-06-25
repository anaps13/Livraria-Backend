//classe usada para configurar e acessar o banco de dados em uma aplicação ASP.NET Core

using Bookstore.Models;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }

        //Adiciona um construtor que aceita DbContextOptions<AppDbContext>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}




