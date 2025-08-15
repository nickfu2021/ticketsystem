using TicketSystemApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicketSystemApi.Common;
using FluentValidation.AspNetCore;
using FluentValidation;
using TicketSystemApi.Repositories;
using TicketSystemApi.Services;
using TicketSystemApi.Validators;
using TicketSystemApi.Middleware;
using TicketSystemApi.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings!.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthentication();

builder.Services.AddControllers();

/*
AddFluentValidationAutoValidation
啟用 伺服器端自動驗證
ASP.NET Core 在模型綁定（ModelState）時，會自動執行對應的 Validator。
你不需要在 Controller 中手動呼叫 validator.Validate(...)
這是 FluentValidation 最基本的功能

.AddFluentValidationClientsideAdapters()
啟用 前端 JavaScript 客戶端驗證支援（例如 Razor Pages / Blazor Server）
會把你設定的規則自動轉換成 HTML data-val-* 屬性
僅適用於 Razor Page、Blazor Server，搭配 jQuery Validation 使用
如果你用的是 API + Vue/React/Next.js，就不需要這個
*/
builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddValidatorsFromAssemblyContaining<Program>(); 避免耦合 Program
builder.Services.AddValidatorsFromAssemblyContaining<OrderUpdateDtoValidator>();    //只要註冊其中「任一個」即可掃描整個專案的 Validators

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

app.UseMiddleware<ValidationExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Swagger 僅在開發時啟用
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
//app.UseHttpsRedirection();
app.Run();

