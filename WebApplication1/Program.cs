using NSwag.AspNetCore; // Importa o pacote NSwag.AspNetCore para documentação da API
using Bookstore.Models; // Modelos de dados da aplicação
using Bookstore.Data; // Camada de acesso a dados da aplicação 
using Microsoft.EntityFrameworkCore; //Entity Framework Core para interagir com o banco de dados

class HelloWeb
{
    static void Main(string[] args)
    {

        // Configuração básica da aplicação ASP.NET Core
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
 
        // Configuração do banco de dados SQLite e da documentação da API
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("DataSource=books.sqlite;Cache=Shared"));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "Books";
            config.Title = "Books v1";
            config.Version = "v1";
        });


        // Configura a documentação da API usando o Swagger UI apenas no ambiente de desenvolvimento
        WebApplication app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi(config =>
            {
                config.DocumentTitle = "Books API";
                config.Path = "/swagger";
                config.DocumentPath = "/swagger/{documentName}/swagger.json";
                config.DocExpansion = "list";
            });
        }


        //Acessa todos os livros disponíveis na API
        app.MapGet("/api/books", (AppDbContext context) =>
        {
            var books = context.Books;
            return books is not null ? Results.Ok(books) : Results.NotFound();
        }).Produces<Book>();

        //Procura um livro no banco de dados com o ID fornecido
        app.MapGet("/api/books/{id}", (AppDbContext context, Guid id) =>
        {
            var book = context.Books.Find(id);
            return book != null ? Results.Ok(book) : Results.NotFound();
        }).Produces<Book>();

        //Adiciona um novo livro à API
        app.MapPost("/api/books", (AppDbContext context, string title, string author) =>
        {
            var book = new Book(Guid.NewGuid(), title, author);
            context.Books.Add(book);
            context.SaveChanges();
            return Results.Ok(book);
        }).Produces<Book>();

        //Atualiza um livro existente
        app.MapPut("/api/books", (AppDbContext context, Book inputBook) =>
        {
            var book = context.Books.Find(inputBook.Id);
            if (book == null)
            {
                return Results.NotFound();
            }
            var entry = context.Entry(book).CurrentValues;
            entry.SetValues(inputBook);
            context.SaveChanges();
            return Results.Ok(book);
        }).Produces<Book>();

        //Atualiza parcialmente um recurso existente
        app.MapPatch("/api/books/{id}", (AppDbContext context, Guid id, Book inputBook) =>
        {
            var book = context.Books.Find(id);
            if (book == null)
            {
                return Results.NotFound();
            }
            context.Entry(book).CurrentValues.SetValues(inputBook);
            context.SaveChanges();
            return Results.Ok(book);
        }).Produces<Book>();

        //Remove um livro existente pelo ID fornecido
        app.MapDelete("/api/books", (AppDbContext context, Guid id) =>
        {
            var book = context.Books.Find(id);
            if (book == null)
            {
                return Results.NotFound();
            }
            context.Books.Remove(book);
            context.SaveChanges();
            return Results.Ok(book);
        }).Produces<Book>();

        //Retorna a contagem total de livros na API
        app.MapGet("/api/books/count", (AppDbContext context) =>
        {
            var count = context.Books.Count();
            return Results.Ok(count);
        }).Produces<int>();

        app.Run();
    }
}



