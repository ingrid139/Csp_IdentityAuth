﻿using System.Collections.Generic;
using IdentityAuth.ConfigStartup;
using IdentityAuth.Filters;
using IdentityAuth.Models;
using IdentityAuth.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IdentityAuth
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                    .AddJsonFormatters()
                    .AddApiExplorer()
                    .AddVersionedApiExplorer(p =>
                    {
                        p.GroupNameFormat = "'v'VVV";
                        p.SubstituteApiVersionInUrl = true;
                    });

            services.AddMvc(opt =>
            {
                opt.Filters.Add(typeof(ErrorResponseFilter));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // config Identity por um método de extensão de IServiceCollection
            services.AddIdentityConfiguration(Configuration);

            // config versionamento
            services.AddApiVersioning(p =>
            {
                p.DefaultApiVersion = new ApiVersion(1, 0);
                p.ReportApiVersions = true;
                p.AssumeDefaultVersionWhenUnspecified = true;
            });

            // config desab validação de Model Sate automatica
            services.Configure<ApiBehaviorOptions>(opt =>
            {
                opt.SuppressModelStateInvalidFilter = true;
            });


            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityRequirement(
                    new Dictionary<string, IEnumerable<string>> {
                    { "Bearer", new string[] { } }
                    });

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey",
                    Description = "Insira o token JWT dessa maneira: Bearer {seu token}"
                });
            });


            services.AddTransient<IEmailServices, EmailServices>();

            // add config sendGrid
            services.Configure<SendGridOptions>(Configuration.GetSection("SendGridOptions"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(opt =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    opt.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
                opt.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            });

            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
