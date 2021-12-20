using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;
using System.Text;
using allinfo22.Helpers;
using allinfo22.Services;
using allinfo22.Requirements;
using allinfo22.Handlers;
using allinfo22.Interfaces;
using allinfo22.dataAccessLayer;
using Newtonsoft.Json;
using allinfo22.Models;
using Microsoft.AspNetCore.Http.Features;

namespace allinfo22
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

            var emailConfig = Configuration
               .GetSection("EmailConfiguration")
               .Get<EmailConfiguration>();
            services.AddSingleton(emailConfig);
            services.AddScoped<IEmailSender, EmailSender>();

            services.Configure<FormOptions>(o => {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
            services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "https://admin.usaimportdata.com",
                    ValidAudience = "https://admin.usaimportdata.com",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"))
                };
            });

            services.AddControllersWithViews();

            services.AddCors();

            services.AddMvc().AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
           

            services.AddSingleton<IAuthorizationHandler, CustomerBlockedStatusHandler>();
           // services.AddSingleton(emailConfig);
            //services.AddScoped<IEmailSender, EmailSender>();
            // services.AddScoped<ICustomerService, UserService>();
            services.AddScoped<Iuserservice, UserService>();
          
            services.AddScoped<IDAL, DAL>();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                        builder =>
                        {
                            builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                        });

            });
            services.AddControllers();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHsts();
            }
            //app.UseMiddleware<JwtMiddleware>();
            app.UseHttpsRedirection();
            app.UseRouting();
           
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowSpecificOrigin");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
