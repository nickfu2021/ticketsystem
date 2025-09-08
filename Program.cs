using TicketSystemApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation.AspNetCore;
using FluentValidation;
using TicketSystemApi.Repositories;
using TicketSystemApi.Services;
using TicketSystemApi.Validators;
using TicketSystemApi.Services.Auth;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using TicketSystemApi.Configurations;
using TicketSystemApi.Common;
using Microsoft.AspNetCore.Mvc;
using TicketSystemApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ----- 設定 & DI -----
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<FrontendSettings>(builder.Configuration.GetSection("Frontend"));
builder.Services.Configure<MailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPostalRepository, PostalRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ----- Auth -----
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var settings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                       ?? throw new InvalidOperationException("JwtSettings is missing");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// ----- MVC + FluentValidation + ModelState 統一輸出 -----
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(opt =>
    {
        opt.InvalidModelStateResponseFactory = ctx =>
        {
            var fieldErrors = ctx.ModelState
                .Where(kv => kv.Value?.Errors?.Any() == true)
                .SelectMany(kv => kv.Value!.Errors.Select(err => new FieldError
                {
                    Field = kv.Key,
                    Message = err.ErrorMessage
                }))
                .ToList();

            var payload = ServiceResult<object?>.Fail("欄位驗證錯誤", fieldErrors);
            return new BadRequestObjectResult(payload);
        };
    });

builder.Services.AddFluentValidationAutoValidation();
// 只要挑一個你專案內的 Validator 類別來讓掃描器定位組件即可
builder.Services.AddValidatorsFromAssemblyContaining<OrderUpdateDtoValidator>();

// ----- Swagger -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var jwtScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "在下方輸入：Bearer {token}"
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// ----- 例外處理（建議：開發用 Dev Page；正式用自訂 Middleware）-----
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // 讓自訂的 ApiExceptionMiddleware 接手所有未處理例外
    app.UseMiddleware<ApiExceptionMiddleware>();
}

// ※ 想在開發環境也用自訂例外頁，可把上面 else 改成無條件使用 ApiExceptionMiddleware，並移除 DeveloperExceptionPage。

// ----- Auth -----
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// app.UseHttpsRedirection(); // 需 HTTPS 再開

app.Run();
