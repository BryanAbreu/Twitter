using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Database.Models;
using Email;
using AutoMapper;
using Twitt_prof.Infraestructure.AutoMapper;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Repository.Repository;

namespace Twitt_prof
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDbContext<TwittContext>(Options =>
            Options.UseSqlServer(Configuration.GetConnectionString("Default")));

            services.AddSession(so =>
            {
                so.IdleTimeout = TimeSpan.FromHours(1);
            });
            var EmailConfig = Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
            services.AddSingleton(EmailConfig);

            services.AddScoped<IEmailSender , GmailSender>();
            services.AddAutoMapper(typeof(AutoMapperConfiguration).GetTypeInfo().Assembly);

            services.AddIdentity<IdentityUser, IdentityRole>(Option => {
                Option.Password = new PasswordOptions
                {
                    RequireNonAlphanumeric = false,
                    RequireLowercase = false,
                    RequireUppercase = false,
                    RequireDigit = true,
                    RequiredLength = 6,


                };
                
                

            })
                .AddEntityFrameworkStores<TwittContext>().AddDefaultTokenProviders();
            services.AddScoped<PostRepository>();
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=User}/{action=Login}/{id?}");
            });
        }
    }
}
