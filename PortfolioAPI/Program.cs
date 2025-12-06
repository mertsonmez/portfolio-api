using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// CORS politikası ekle (frontend'den erişim için)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger sadece development'ta
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// In-memory blog data
var blogs = new List<BlogPost>
{
    new BlogPost
    {
        Id = 1,
        Title = ".NET Core ile RESTful API Geliştirme",
        Summary = "Modern web uygulamaları için .NET Core kullanarak nasıl güçlü API'ler geliştirebileceğinizi öğrenin.",
        Content = "Bu yazıda .NET Core ile RESTful API geliştirme konusunu detaylı şekilde inceleyeceğiz. Minimal API'ler, dependency injection, middleware kullanımı ve daha fazlası...",
        Author = "Adınız Soyadınız",
        PublishedDate = new DateTime(2024, 11, 15),
        Category = "Backend",
        Tags = new[] { ".NET Core", "API", "C#" },
        ReadTimeMinutes = 8
    },
    new BlogPost
    {
        Id = 2,
        Title = "Entity Framework Core Best Practices",
        Summary = "EF Core kullanırken dikkat etmeniz gereken performans ve güvenlik ipuçları.",
        Content = "Entity Framework Core, .NET dünyasında en popüler ORM araçlarından biridir. Bu yazıda performans optimizasyonu, query optimization, migration stratejileri gibi konuları ele alacağız...",
        Author = "Adınız Soyadınız",
        PublishedDate = new DateTime(2024, 11, 20),
        Category = "Database",
        Tags = new[] { "EF Core", "Database", "Performance" },
        ReadTimeMinutes = 12
    },
    new BlogPost
    {
        Id = 3,
        Title = "Docker ile .NET Uygulamalarını Containerize Etme",
        Summary = "Docker kullanarak .NET uygulamalarınızı nasıl paketleyip deploy edeceğinizi öğrenin.",
        Content = "Containerization, modern uygulama geliştirme süreçlerinin vazgeçilmez bir parçası. Bu yazıda Docker ile .NET uygulamalarını nasıl containerize edeceğinizi, Docker Compose kullanımını ve production deployment stratejilerini göreceğiz...",
        Author = "Adınız Soyadınız",
        PublishedDate = new DateTime(2024, 12, 1),
        Category = "DevOps",
        Tags = new[] { "Docker", ".NET", "Deployment" },
        ReadTimeMinutes = 10
    }
};

// API Endpoints

// Health check endpoint
app.MapGet("/", () => new
{
    status = "OK",
    message = "Portfolio API is running",
    version = "1.0.0",
    endpoints = new[]
    {
        "GET /api/blogs - Tüm blog yazılarını getir",
        "GET /api/blogs/{id} - Belirli bir blog yazısını getir",
        "GET /api/blogs/category/{category} - Kategoriye göre filtrele"
    }
});

// Tüm blog yazılarını getir
app.MapGet("/api/blogs", () =>
{
    return Results.Ok(new
    {
        success = true,
        data = blogs.OrderByDescending(b => b.PublishedDate),
        count = blogs.Count
    });
})
.WithName("GetAllBlogs")
.WithOpenApi();

// ID'ye göre blog yazısı getir
app.MapGet("/api/blogs/{id}", (int id) =>
{
    var blog = blogs.FirstOrDefault(b => b.Id == id);
    
    if (blog == null)
    {
        return Results.NotFound(new
        {
            success = false,
            message = $"ID {id} ile blog yazısı bulunamadı"
        });
    }
    
    return Results.Ok(new
    {
        success = true,
        data = blog
    });
})
.WithName("GetBlogById")
.WithOpenApi();

// Kategoriye göre filtrele
app.MapGet("/api/blogs/category/{category}", (string category) =>
{
    var filteredBlogs = blogs
        .Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(b => b.PublishedDate);
    
    return Results.Ok(new
    {
        success = true,
        data = filteredBlogs,
        count = filteredBlogs.Count()
    });
})
.WithName("GetBlogsByCategory")
.WithOpenApi();

// Tüm kategorileri getir
app.MapGet("/api/categories", () =>
{
    var categories = blogs
        .Select(b => b.Category)
        .Distinct()
        .OrderBy(c => c);
    
    return Results.Ok(new
    {
        success = true,
        data = categories
    });
})
.WithName("GetCategories")
.WithOpenApi();

// Blog arama
app.MapGet("/api/blogs/search", ([FromQuery] string query) =>
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return Results.BadRequest(new
        {
            success = false,
            message = "Arama terimi boş olamaz"
        });
    }
    
    var results = blogs.Where(b =>
        b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        b.Summary.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        b.Content.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        b.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase))
    ).OrderByDescending(b => b.PublishedDate);
    
    return Results.Ok(new
    {
        success = true,
        data = results,
        count = results.Count(),
        query = query
    });
})
.WithName("SearchBlogs")
.WithOpenApi();

app.Run();

// Blog Post Model
public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public int ReadTimeMinutes { get; set; }
}