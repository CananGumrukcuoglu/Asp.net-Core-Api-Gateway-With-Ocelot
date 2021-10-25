using ApiGateway.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Services.Middleware.Loggings;
using System.Text;
using System.Threading.Tasks;
using Ocelot.Provider.Polly;

namespace ApiGateway
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            StartupExtensions.Configuration = configuration;
        }

        readonly string myOrigins = "allowCors";

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var corsConfig = Configuration.GetSection("CorsConfig").Get<CorsConfig>();
            services.AddCors(options =>
            {
                options.AddPolicy(myOrigins,
                    builder => { builder.WithOrigins(corsConfig.Urls.ToArray()).AllowAnyHeader().AllowAnyMethod(); });
            });
            services.AddSettings();
            services.AddJwtToken();
            services.AddOcelot(Configuration)
                .AddPolly();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(myOrigins);
            app.UseMiddleware<SerilogMiddleware>();
            await app.UseOcelot();
        }
    }

    public static class StartupExtensions
    {
        public static IConfiguration Configuration { get; set; }

        public static void AddSettings(this IServiceCollection services)
        {
            services.Configure<TokenConfig>(Configuration.GetSection("TokenConfig"));
            services.Configure<CorsConfig>(Configuration.GetSection("CorsConfig"));
        }

        public static void AddJwtToken(this IServiceCollection services)
        {
            var token = Configuration.GetSection("TokenConfig").Get<TokenConfig>();

            var key = Encoding.ASCII.GetBytes(token.Secret);
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = ctx => { return Task.CompletedTask; },
                        OnAuthenticationFailed = ctx =>
                        {
                            if (ctx.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                ctx.Response.Headers.Add("Token-Expired", "true");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
}