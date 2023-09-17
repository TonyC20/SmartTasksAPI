using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartTasksAPI;
using SmartTasksAPI.DbContexts;
using SmartTasksAPI.Entities;
using SmartTasksAPI.Services;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddNewtonsoftJson()
    .AddXmlDataContractSerializerFormatters();

// Configure logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/smarttasks.txt")
    .CreateLogger();

builder.Host.UseSerilog();



// Configure documentation
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setupAction =>
{
    var xmlCommentsPath = Path.Combine(AppContext.BaseDirectory, "SmartTasksAPI.xml");

    // Add xml comments to the documentation
    setupAction.IncludeXmlComments(xmlCommentsPath);

    // Add title and description
    setupAction.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = Constants.AppTitle,
        Description = Constants.AppDescription,
    });

    // Setup token based authentication
    setupAction.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"." +
                      "\nObtain a token at `/api/v1/account/authenticate`",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer" // Must be lowercase
    });

    setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

// Configure database related services (using Sql Server)
var connectionString = builder.Configuration.GetConnectionString("SmartTasksConnectionString");
if (connectionString == null)
{
    throw new Exception("Connection string not found");
}
builder.Services.AddDbContext<SmartTasksContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<ISmartTasksRepository, SmartTasksRepository>();

// Configure user authentication
builder.Services.AddIdentityCore<UserAccount>(options =>
    {
        // Password options
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredLength = Constants.MinPasswordLength;
    })
    .AddEntityFrameworkStores<SmartTasksContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"]))
        };
    });

// Configure versions
builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new ApiVersion(1, 0);
    setupAction.ReportApiVersions = true;
});

// Build app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
