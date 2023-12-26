
using API.SignalrHub;
using DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .Build();
builder.Services.AddHttpClient();

builder.Services.AddTransient<ChatHub>();
builder.Services.AddDbContext<ManageAppDbContext>(options =>
           options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
var secret = builder.Configuration["JWT:Secret"];
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddAuthentication()
  .AddJwtBearer( options =>
  {
      options.Authority = "";
      options.Audience = "CoffeeAPI";
      options.Authority = "https://localhost:5443";
      options.Audience = "https://localhost:5443/resources";
 //     options.Authority = "chatSignalr";

      options.RequireHttpsMetadata = false;
      options.Events = new JwtBearerEvents
      {
          OnMessageReceived = context => {
              var accessToken = context.Request.Query["access_token"];
              var path = context.HttpContext.Request.Path;
              if (!string.IsNullOrEmpty(accessToken)
                  && path.StartsWithSegments("/SignalrHub"))
              {
                  context.Token = accessToken;
              }
              return Task.CompletedTask;
          }
      };

  });
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()  // Đặt cụm từ khóa allowCredentials
            .WithOrigins("https://localhost:7037")
            ); // Thay đổi đúng theo origin của bạn
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseHttpsRedirection();app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/SignalrHub");
});

app.Run();
