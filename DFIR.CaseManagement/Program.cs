using System.Text;
using DFIR.CaseManagement.Auth;
using DFIR.CaseManagement.Data;
using DFIR.CaseManagement.Interfaces;
using DFIR.CaseManagement.Middleware;
using DFIR.CaseManagement.Repositories;
using DFIR.CaseManagement.Services;
using DFIR.CaseManagement.Services.Observers;
using DFIR.CaseManagement.Services.Strategies;
using DFIR.CaseManagement.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Entity Framework Core - SQL Server provider.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository Pattern + Unit of Work.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Service Layer.
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<IEvidenceService, EvidenceService>();
builder.Services.AddScoped<ICustodyService, CustodyService>();
builder.Services.AddScoped<IMalwareService, MalwareAnalysisService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Strategy Pattern - report generators selected at runtime by format key.
builder.Services.AddScoped<IReportGenerator, PdfReportGenerator>();
builder.Services.AddScoped<IReportGenerator, HtmlReportGenerator>();
builder.Services.AddScoped<IReportGenerator, ExcelReportGenerator>();

// Observer Pattern - one shared subject (publisher) + the concrete observers.
builder.Services.AddSingleton<CaseEventPublisher>();
builder.Services.AddSingleton<ICaseSubject>(sp => sp.GetRequiredService<CaseEventPublisher>());
builder.Services.AddSingleton<AuditLogObserver>();
builder.Services.AddSingleton<NotificationObserver>();

// JWT authentication services.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IRefreshTokenStore, RefreshTokenStore>();

// FluentValidation - register every validator in this assembly.
builder.Services.AddValidatorsFromAssemblyContaining<CaseCreateDtoValidator>();

// Authentication - validate the bearer JWT on every secured request.
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
builder.Services
    .AddAuthentication(options =>
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
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// Policy-based authorization - one policy per fine-grained permission claim.
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.All)
        options.AddPolicy(permission, policy =>
            policy.RequireClaim(AppClaimTypes.Permission, permission));
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT bearer support so endpoints can be tested after logging in.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DFIR Case Management API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT access token here (without the 'Bearer ' prefix).",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();

// Wire the Observer Pattern: attach observers to the case event publisher at startup.
var caseEventPublisher = app.Services.GetRequiredService<CaseEventPublisher>();
caseEventPublisher.Attach(app.Services.GetRequiredService<AuditLogObserver>());
caseEventPublisher.Attach(app.Services.GetRequiredService<NotificationObserver>());

// Apply migrations and seed the default users.
await DbSeeder.SeedAsync(app.Services);

// Seed synthetic demonstration data (cases, evidence, custody, malware).
await SyntheticDataSeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DFIR Case Management API v1"));

// Custom middleware: catch-all JSON errors first, then request logging.
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
