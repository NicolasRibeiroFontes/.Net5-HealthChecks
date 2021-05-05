using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNet5_HealthChecks
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

			services.AddHealthChecks();
			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "DotNet5_HealthChecks", Version = "v1" });
			});

			services.AddHealthChecks()
				.AddSqlServer(Configuration.GetConnectionString("SqlServer"),
					name: "sqlserver", tags: new string[] { "db", "data" });

			services.AddHealthChecksUI()
				.AddInMemoryStorage();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				
			}
			app.UseSwagger();
			app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DotNet5_HealthChecks v1"));

			app.UseHealthChecks("/status",
				new HealthCheckOptions()
				{
					ResponseWriter = async (context, report) =>
					{
						var result = JsonSerializer.Serialize(
							new
							{
								currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
								statusApplication = report.Status.ToString(),
                                healthChecks = report.Entries.Select(e => new
                                {
                                    check = e.Key,
                                    status = Enum.GetName(typeof(HealthStatus), e.Value.Status)
                                })
                            });

						context.Response.ContentType = MediaTypeNames.Application.Json;
						await context.Response.WriteAsync(result);
					}
				});

			// Generated the endpoint which will return the needed data
			app.UseHealthChecks("/healthchecks-data-ui", new HealthCheckOptions()
			{
				Predicate = _ => true,
				ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
			});

			// Activate the dashboard for UI
			app.UseHealthChecksUI(options =>
			{
				options.UIPath = "/monitor";
			});

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
