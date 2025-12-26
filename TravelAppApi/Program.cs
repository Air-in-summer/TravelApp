using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TravelAppApi.Data;
using TravelAppApi.Services;
using TravelAppApi.Repository;
namespace TravelAppApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Cấu hình Database Context với PostgreSQL
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Đăng ký các Services và Repositories vào Dependency Injection Container
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<TripService>();
            builder.Services.AddScoped<DestinationRepository>();
            builder.Services.AddScoped<TripRepository>();
            builder.Services.AddScoped<UserPreferencesRepository>();
            builder.Services.AddScoped<StopService>();
            builder.Services.AddScoped<StopRepository>();
            builder.Services.AddScoped<PlaceRepository>();
            builder.Services.AddScoped<PdfService>();
            builder.Services.AddScoped<ReviewService>();
            builder.Services.AddScoped<RecommendationService>();
            builder.Services.AddScoped<ReviewRepository>();

            // Cấu hình Controllers với JSON serialization để xử lý circular references
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            // Cấu hình CORS để cho phép frontend truy cập API
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            // Thêm health checks cho database
            builder.Services.AddHealthChecks()
                .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Cấu hình HTTP request pipeline (middleware)
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            // Sử dụng CORS policy
            app.UseCors("AllowAll");

            app.UseAuthorization();

            // Map health check endpoint
            app.MapHealthChecks("/health");

            app.MapControllers();

            app.Run();
        }
    }
}
