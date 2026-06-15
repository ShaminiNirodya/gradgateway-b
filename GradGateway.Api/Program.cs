using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using GradGateway.Api.Json;
using GradGateway.Api.Authorization;
using GradGateway.Api.Middleware;
using GradGateway.Business.Interfaces;
using GradGateway.Business.Options;
using GradGateway.Business.Services;
using GradGateway.Data.Context;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddResponseCaching();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new UtcNullableDateTimeConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GradGateway API",
        Version = "v1",
        Description = "API for GradGateway - Platform for Sri Lankan Undergraduates",
        Contact = new OpenApiContact
        {
            Name = "GradGateway Team",
            Email = "admin@gradgateway.com"
        }
    });

    // Configure Firebase JWT Bearer authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your Firebase JWT token in the format: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configure Database
builder.Services.AddDbContext<GradGatewayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Firebase Authentication
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true,
            NameClaimType = "user_id",
        };

        // SignalR WebSockets send the Firebase JWT as access_token on the query string.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var path = context.HttpContext.Request.Path;
                if (!path.StartsWithSegments("/hubs"))
                {
                    return Task.CompletedTask;
                }

                var token = context.Request.Query["access_token"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(token))
                {
                    var authHeader = context.Request.Headers.Authorization.ToString();
                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        token = authHeader["Bearer ".Length..].Trim();
                    }
                }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.Requirements.Add(new AdminRoleRequirement()));
});
builder.Services.AddScoped<IAuthorizationHandler, AdminRoleHandler>();

// Configure CORS for Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add SignalR (same UTC JSON handling as REST API)
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.PayloadSerializerOptions.Converters.Add(new UtcDateTimeConverter());
        options.PayloadSerializerOptions.Converters.Add(new UtcNullableDateTimeConverter());
    });

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.Configure<FirebaseAdminOptions>(builder.Configuration.GetSection(FirebaseAdminOptions.SectionName));

if (builder.Environment.IsDevelopment())
{
    var emailSection = builder.Configuration.GetSection(EmailOptions.SectionName);
    var smtpUser = emailSection["SmtpUser"];
    var smtpPassword = emailSection["SmtpPassword"];
    Console.WriteLine(
        $"[Email config] Enabled={emailSection["Enabled"]}, User={smtpUser}, PasswordLength={smtpPassword?.Length ?? 0}");
}

// Register Services
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<IFirebaseAdminService, FirebaseAdminAuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IInterviewPlanService, InterviewPlanService>();
builder.Services.AddScoped<IOpportunityService, OpportunityService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDeadlineNotificationProcessor, DeadlineNotificationProcessor>();
builder.Services.AddHostedService<GradGateway.Api.Background.DeadlineNotificationBackgroundService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IEmailLogService, EmailLogService>();
builder.Services.AddScoped<IPlatformStatsService, PlatformStatsService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISupportInquiryService, SupportInquiryService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddSingleton<IRealtimeNotificationService>(sp =>
{
    var hubContext = sp.GetRequiredService<IHubContext<GradGateway.Api.Hubs.ChatHub>>();
    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

    async Task SendToUserGroupAsync(Guid userId, string method, object data)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GradGatewayDbContext>();
        var firebaseUid = await db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.FirebaseUid)
            .FirstOrDefaultAsync();

        if (!string.IsNullOrWhiteSpace(firebaseUid))
        {
            await hubContext.Clients.Group(firebaseUid).SendAsync(method, data);
        }
    }

    return new RealtimeNotificationService
    {
        SendMessageFunc = (userId, data) => SendToUserGroupAsync(userId, "ReceiveMessage", data),
        SendConversationUpdateFunc = (userId, data) => SendToUserGroupAsync(userId, "ConversationUpdated", data),
        SendNotificationFunc = async (firebaseUid, data) =>
            await hubContext.Clients.Group(firebaseUid).SendAsync("ReceiveNotification", data),
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GradGatewayDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GradGateway API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root (https://localhost:7059/)
        options.DocumentTitle = "GradGateway API Documentation";
        options.DisplayRequestDuration();
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendPolicy");
app.UseResponseCaching();

app.UseAuthentication(); // Verify who they are (Firebase JWT)
app.UseMiddleware<PlatformAccessMiddleware>();
app.UseAuthorization();  // Verify what they can do (Roles)

app.MapControllers();
app.MapHub<GradGateway.Api.Hubs.ChatHub>("/hubs/chat");

app.Run();
