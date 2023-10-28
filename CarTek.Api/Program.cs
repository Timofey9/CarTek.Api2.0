using CarTek.Api.Const;
using CarTek.Api.DBContext;
using CarTek.Api.Mapper;
using CarTek.Api.Services;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseNpgsql(builder.Configuration.GetConnectionString("ApplicationDbContext")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter(); 

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(ModelProfile));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITrailerService, TrailerService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IQuestionaryService, QuestionaryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportGeneratorService, ReportGeneratorService>();
builder.Services.AddScoped<IDriverTaskService, DriverTaskService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddScoped<IAWSS3ClientFactory, AWSS3ClientFactory>();
builder.Services.AddScoped<IAWSS3Service, AWSS3Service>();

builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.ADMIN_ONLY, policy => policy.RequireClaim(AuthConstants.ClaimTypeIsAdmin));
    options.AddPolicy(AuthPolicies.DRIVER_ONLY, policy => policy.RequireClaim(AuthConstants.ClaimTypeIsDriver));
});

builder.Services.AddControllers()
                .AddNewtonsoftJson();


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins(new string[] { "http://151.248.113.138:3000", "http://cartek-app.ru", "http://cartek-app.ru:3000", "http://localhost:3000", "https://localhost:3000", 
                "https://cartek-app.online" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
        });
});

var app = builder.Build();

var loggerFactory = app.Services.GetService<ILoggerFactory>();

loggerFactory.AddFile("/data/Logs/log-{Date}.txt");

app.UseStaticFiles();

app.UseCors("_AllowSpecificOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseCors("_AllowSpecificOrigins");
    app.UseSwaggerUI();
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    //context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    DbInitializer.Initialize(context);
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
