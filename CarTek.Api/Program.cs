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

builder.Services.AddAutoMapper(typeof(ModelProfile));

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ICarService, CarService>();
builder.Services.AddTransient<IDriverService, DriverService>();
builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.AddTransient<IQuestionaryService, QuestionaryService>();

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
});

builder.Services.AddControllers()
                .AddNewtonsoftJson();


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins(new string[] { "https://localhost:3000" })
            .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        });
});

var app = builder.Build();

app.UseStaticFiles();



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
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
