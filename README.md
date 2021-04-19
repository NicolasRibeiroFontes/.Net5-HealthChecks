# .Net5-HealthChecks

1. Create an WebAPI project using .Net 5
2. Add the line `services.AddHealthChecks();` in ConfigureServices and `app.UseHealthChecks("/status",` in Configure
3. Install the packages `AspNetCore.HealthChecks.SqlServer`, `AspNetCore.HealthChecks.UI`, `AspNetCore.HealthChecks.UI.Client`, `AspNetCore.HealthChecks.UI.InMemory.Storage`

 
3.1 Add the lines in Configure Services:

    services.AddHealthChecks()
				.AddSqlServer(Configuration.GetConnectionString("SqlServer"),
					name: "sqlserver", tags: new string[] { "db", "data" }); 
    services.AddHealthChecksUI()
				.AddInMemoryStorage();
      
3.2 Add the lines in Configure:

    app.UseHealthChecks("/healthchecks-data-ui", new HealthCheckOptions()
			{
				Predicate = _ => true,
				ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
			});
			app.UseHealthChecksUI(options =>
			{
				options.UIPath = "/monitor";
			});
    
