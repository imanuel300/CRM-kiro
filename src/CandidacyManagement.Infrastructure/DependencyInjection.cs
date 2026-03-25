using CandidacyManagement.Application.Notifications;
using CandidacyManagement.Application.Security;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Infrastructure.Email;
using CandidacyManagement.Infrastructure.Persistence;
using CandidacyManagement.Infrastructure.Security;
using CandidacyManagement.Infrastructure.Sms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CandidacyManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Notification delivery services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddHttpClient("SmsProvider");

        // Encryption service for column-level encryption (Requirement 18.1)
        var encryptionKey = configuration["Security:EncryptionKey"]
            ?? Convert.ToBase64String(new byte[32]); // fallback for dev
        services.AddSingleton<IEncryptionService>(new EncryptionService(encryptionKey));

        return services;
    }
}
