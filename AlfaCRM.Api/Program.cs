using System.Security.Claims;
using System.Text;
using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Interfaces.Services.Security;
using AlfaCRM.Domain.Models.Settings;
using AlfaCRM.Infrastructure;
using AlfaCRM.Infrastructure.Repositories;
using AlfaCRM.Services.Entity;
using AlfaCRM.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
var configuration = builder.Configuration;

#region Service configuration

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(_ => true);
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecurityKey"] ??
                                                                           throw new ApplicationException(
                                                                               "SecurityKey is missing.")))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token)) context.Token = token;
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            try
            {
                var claims = context.Principal?.Claims;
                var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdClaim?.Value, out var userId))
                {
                    var identity = context.Principal.Identity as ClaimsIdentity;
                    if (identity != null)
                    {
                        if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Failed to parse UserId from token");
                    context.Fail("Invalid UserId in token");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation error: {ex.Message}");
                context.Fail("Invalid token");
            }

            return Task.CompletedTask;
        }
    };
});

#endregion


#region Dependency injection

builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(configuration.GetConnectionString("Database"))
        .UseLazyLoadingProxies();
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostReactionRepository, PostReactionRepository>();
builder.Services.AddScoped<IPostCommentRepository, PostCommentRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IPostReactionService, PostReactionService>();
builder.Services.AddScoped<IPostCommentService, PostCommentService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IHashService, SHA256Hasher>();
builder.Services.AddScoped<IJwtService>(provider => new JwtService(new JwtSettings(
    Audience: configuration["JWT:Audience"] ?? throw new ApplicationException("Missing JWT:Audience"),
    Issuer: configuration["JWT:Issuer"] ?? throw new ApplicationException("Missing JWT:Issuer"),
    SecurityKey: configuration["JWT:SecurityKey"] ?? throw new ApplicationException("Missing JWT:SecurityKey"),
    ExpireHours: int.Parse(configuration["JWT:ExpireHours"] ?? throw new ApplicationException("Missing JWT:ExpireHours"))
)));

#endregion

builder.Services.AddControllers();

var app = builder.Build();


app.UseCors("ReactFrontend");
app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();