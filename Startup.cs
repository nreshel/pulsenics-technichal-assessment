// Startup.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace PulsenicsApp
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // ConfigureServices is used to configure the services used by the application.
    public void ConfigureServices(IServiceCollection services)
    {
      // Add Razor Pages service to the application
      services.AddRazorPages();

      // Configure SQLite database with a file path
      services.AddDbContext<ApplicationDbContext>(options =>
      {
        // Get the connection string named "DefaultConnection" from configuration
        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        // Use SQLite database with the specified file path
        options.UseSqlite($"Data Source={Path.Combine(Directory.GetCurrentDirectory(), connectionString)}");
      });
    }

    // Configure is used to set up the middleware pipeline and HTTP request processing.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      // Check if the application is running in development mode
      if (env.IsDevelopment())
      {
        // Enable the developer exception page for better error details
        app.UseDeveloperExceptionPage();
      }
      else
      {
        // Use a generic error page and enable HTTP Strict Transport Security (HSTS) in production
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
      }

      // Redirect HTTP requests to HTTPS
      app.UseHttpsRedirection();

      // Serve static files (e.g., CSS, images) from the wwwroot folder
      app.UseStaticFiles();

      // Enable routing for the application
      app.UseRouting();

      // Enable authorization for the application
      app.UseAuthorization();

      // Configure the endpoints for Razor Pages
      app.UseEndpoints(endpoints =>
      {
        endpoints.MapRazorPages();
      });
    }
  }
}
