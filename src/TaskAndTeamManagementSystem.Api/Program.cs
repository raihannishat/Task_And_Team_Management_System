using System.Reflection;
using System.Text;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.Authorization;
using TaskAndTeamManagementSystem.Api.Common.Behaviors;
using TaskAndTeamManagementSystem.Api.Common.Middleware;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Infrastructure.Data;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using TaskAndTeamManagementSystem.Api.Infrastructure.Services;
using TaskAndTeamManagementSystem.Api.Infrastructure.Services.Contracts;

try
{
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    builder.Host.UseSerilog();

    Log.Information("Starting up the application");

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Task and Team Management System API",
            Version = "v1",
            Description = "A comprehensive Task and Team Management System API built with .NET 8, CQRS, and Vertical Slice Architecture"
        });

        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        });
        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = builder.Configuration.GetValue<bool>("Identity:Password:RequireDigit", true);
        options.Password.RequireLowercase = builder.Configuration.GetValue<bool>("Identity:Password:RequireLowercase", true);
        options.Password.RequireUppercase = builder.Configuration.GetValue<bool>("Identity:Password:RequireUppercase", true);
        options.Password.RequireNonAlphanumeric = builder.Configuration.GetValue<bool>("Identity:Password:RequireNonAlphanumeric", false);
        options.Password.RequiredLength = builder.Configuration.GetValue<int>("Identity:Password:RequiredLength", 6);

        options.User.RequireUniqueEmail = builder.Configuration.GetValue<bool>("Identity:User:RequireUniqueEmail", true);
        options.SignIn.RequireConfirmedEmail = builder.Configuration.GetValue<bool>("Identity:User:RequireConfirmedEmail", false);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

    var config = TypeAdapterConfig.GlobalSettings;
    config.Scan(Assembly.GetExecutingAssembly());
    builder.Services.AddSingleton(config);
    builder.Services.AddScoped<IMapper, ServiceMapper>();

    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    var endpointTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && 
                   t is { IsInterface: false, IsAbstract: false });

    foreach (var endpointType in endpointTypes)
    {
        builder.Services.AddSingleton(endpointType);
        builder.Services.AddSingleton(typeof(IEndpoint), endpointType);
    }

    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        AuthorizationPolicies.ConfigurePolicies(options);
    });

    var app = builder.Build();

    app.UseGlobalExceptionMiddleware();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapSwagger("/openapi/{documentName}.json").AllowAnonymous();

        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Task and Team Management System API")
                .WithTheme(ScalarTheme.BluePlanet)
                .WithDefaultHttpClient(
                    ScalarTarget.CSharp,
                    ScalarClient.HttpClient)
                .WithOpenApiRoutePattern("/openapi/v1.json");
        }).AllowAnonymous();
    }

    app.MapEndpoints();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        
        try
        {
            if (app.Environment.IsDevelopment())
            {
                Log.Information("Development environment detected. Ensuring database is recreated...");
                var deleted = await dbContext.Database.EnsureDeletedAsync();
                if (deleted)
                {
                    Log.Information("Existing database deleted.");
                }
            }

            Log.Information("Creating database with all tables...");
            var created = await dbContext.Database.EnsureCreatedAsync();
            
            if (created)
            {
                Log.Information("Database created successfully with all tables.");
            }
            else
            {
                Log.Information("Database already exists with all tables.");
            }

            var canConnect = await dbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                throw new InvalidOperationException("Cannot connect to the database after creation.");
            }

            Log.Information("Starting database seeding...");
            await DbSeeder.SeedAsync(dbContext, userManager, roleManager);
            Log.Information("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred while creating/seeding the database.");
            throw;
        }
    }

    Log.Information("Application started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
