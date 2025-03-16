using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels.HelperVM;
using PizzaShop.Repository.Implementaion;
using PizzaShop.Repository.Interface;
using PizzaShop.Service.Implementaion;
using PizzaShop.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("PostgreSqlConnectionString");

// Add services to the container.
builder.Services.AddDbContext<PizzaShopContext>(q => q.UseNpgsql(conn));
builder.Services.AddControllersWithViews();

var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option => {
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer= true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role
    };

    option.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if(context.Request.Cookies.ContainsKey("AuthToken"))
            {
                context.Token = context.Request.Cookies["AuthToken"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(option => {
    option.AddPolicy("AdminOnly",policy => policy.RequireRole("Admin"));
    option.AddPolicy("AccountManagerOnly",policy => policy.RequireRole("Account Manager"));
    option.AddPolicy("ChefOnly",policy => policy.RequireRole("Chef"));

});

builder.Services.Configure<EmailSettingVM>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IUserService, UserSerivce>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IEmailService,EmailService>();
builder.Services.AddScoped<IMenuService,MenuService>();
builder.Services.AddScoped<ISectionAndTableRepository,SectionAndTableRepository>();
builder.Services.AddScoped<ISectionAndTableService,SectionAndTableService>();
builder.Services.AddScoped<IMenuRepository,MenuRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
