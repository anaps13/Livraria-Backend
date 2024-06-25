using NSwag.AspNetCore; // Pacote NSwag.AspNetCore para documentação da API
using Bookstore.Models; // Modelos de dados da aplicação
using Bookstore.Data; // Camada de acesso a dados da aplicação
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc; //Entity Framework Core para interagir com o banco de dados

public class Program
{
    public static void Main(string[] args)
    {
        // Configuração básica da aplicação ASP.NET Core
        var builder = WebApplication.CreateBuilder(args);

        // Adicionar Configuração de CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder => builder.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader());
        });

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

        // Configuração da documentação da API usando o Swagger UI apenas no ambiente de desenvolvimento
        var app = builder.Build();
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

        // Usar a Política de CORS
        app.UseCors("AllowAllOrigins");

        // Acessa todos os livros disponíveis na API
        app.MapGet("/api/books", (AppDbContext context) =>
        {
            var books = context.Books.ToList(); //recupera os livros do bd usando o contexto do bd
            return books is not null ? Results.Ok(books) : Results.NotFound();
        }).Produces<Book>();

        // Procura um livro no banco de dados com o ID fornecido
        app.MapGet("/api/books/{id}", (AppDbContext context, Guid id) =>
        {
            var book = context.Books.Find(id);
            return book != null ? Results.Ok(book) : Results.NotFound();
        }).Produces<Book>();

        // Adiciona um novo livro à API
        app.MapPost("/api/books", (AppDbContext context, [FromBody] Book newBook) =>
        {
            var book = new Book(Guid.NewGuid(), newBook.Title, newBook.Author);
            context.Books.Add(book);
            context.SaveChanges();
            return Results.Ok(book);
        }).Produces<Book>();

        // Atualiza um livro existente
app.MapPut("/api/books/{id}", (AppDbContext context, Guid id, [FromBody] Book inputBook) =>
{
    var book = context.Books.Find(id);
    if (book == null)
    {
        return Results.NotFound();
    }
    book.Title = inputBook.Title;
    book.Author = inputBook.Author;
    context.SaveChanges();
    return Results.Ok(book);
}).Produces<Book>();



        // Remove um livro existente pelo ID fornecido
        app.MapDelete("/api/books/{id}", (AppDbContext context, Guid id) =>
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

        // Retorna a contagem total de livros na API
        app.MapGet("/api/books/count", (AppDbContext context) =>
        {
            var count = context.Books.Count();
            return Results.Ok(count);
        }).Produces<int>();

        // Atualiza parcialmente um recurso existente
        app.MapPatch("/api/books/{id}", (AppDbContext context, Guid id, [FromBody] string inputBook) =>
        {
            var book = context.Books.Find(id);
            if (book == null)
            {
                return Results.NotFound();
            }

            context.Entry(book).State = EntityState.Detached;
            var updatebook = new Book(id, inputBook, book.Author);
            context.Books.Update(updatebook);
            context.SaveChanges();
            return Results.Ok(updatebook);
        }).Produces<Book>();

        // Pesquisar livros pelo título
        app.MapGet("/api/books/search/{title}", (AppDbContext context, string title) =>
        {
            var books = context.Books.Where(b => b.Title.Contains(title)).ToList();
            return books.Any() ? Results.Ok(books) : Results.NotFound();
        }).Produces<Book[]>();

        // Obter livros de um autor específico
        app.MapGet("/api/books/author/{author}", (AppDbContext context, string author) =>
        {
            var books = context.Books.Where(b => b.Author == author).ToList();
            return books.Any() ? Results.Ok(books) : Results.NotFound();
        }).Produces<Book[]>();

        app.Run();
    }
}