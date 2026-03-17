using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ShopAPI.Services;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IAuthService,    ShopAPI.Services.Impl.AuthService>();
builder.Services.AddScoped<IProductService, ShopAPI.Services.Impl.ProductService>();
builder.Services.AddScoped<ICartService,    ShopAPI.Services.Impl.CartService>();
builder.Services.AddScoped<IOrderService,   ShopAPI.Services.Impl.OrderService>();
builder.Services.AddScoped<IStripeWebhookService, ShopAPI.Services.Impl.StripeWebhookService>();
builder.Services.Configure<PaymentSettings>(builder.Configuration.GetSection("Payment"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200", "https://27b6-175-176-185-189.ngrok-free.app")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();


app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});


app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}



app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
