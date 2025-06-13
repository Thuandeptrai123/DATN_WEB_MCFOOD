using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DUANTOTNGHIEP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            //{
            //    options.SignIn.RequireConfirmedAccount = false;
            //})
            //  .AddEntityFrameworkStores<ASM_C_5Context>()
            //  .AddDefaultTokenProviders();
            //// Add services to the container.
            ////builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
             .AddEntityFrameworkStores<ApplicationDbContext>()
             .AddDefaultTokenProviders();


            builder.Services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts =>
            {
                opts.RequireHttpsMetadata = false;
                opts.SaveToken = true;
                opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? "")),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true
                };
            });


            builder.Services.AddControllers().AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ContractResolver = null;
            });


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy.SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .AllowAnyMethod();
                    });
            });


            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStaticFiles(); // Phục vụ ảnh từ wwwroot
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
        // ✅ Tạo Role mặc định trong Scope hợp lệ
        private static async Task CreateRolesAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                string[] roleNames = { "ADMIN", "STAFF", "Customer" };

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }
            }
        }
    }
}
