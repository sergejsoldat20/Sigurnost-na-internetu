using messages_backend.Data;
using messages_backend.Helpers;
using messages_backend.Middleware;
using messages_backend.RabbitMQ;
using messages_backend.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
var enableBackgroundService = Environment.GetEnvironmentVariable("ENABLE_BACKGORUND_SERVICE");
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbPassword = Environment.GetEnvironmentVariable("DB_SA_PASSWORD");
// Add services to the container.
 // var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var connectionString = $"Server={dbHost};Initial Catalog={dbName};User ID=sa;Password={dbPassword};TrustServerCertificate=True;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
    providerOptions => providerOptions.EnableRetryOnFailure()));

// builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.IgnoreNullValues = true);
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSingleton<AppSettings>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<IMessagesService, MessagesService>();
builder.Services.AddScoped<ISteganographyService, SteganographyService>();

// add RabbitMQ background service
// builder.Services.AddHostedService<RabbitMQConsumer>();

builder.Services.AddSingleton<IEnumerable<IConnectionFactory>>(sp =>
{
	var factories = new List<IConnectionFactory>
	{
		new ConnectionFactory()
		{  
			HostName = "rabbitmq-1", 
			Port = 5672, 
			UserName = "sergej", 
			Password = "NewPassword123" 
		},
		new ConnectionFactory()
		{
			HostName = "rabbitmq-2",
			Port = 5672,
			UserName = "sergej",
			Password = "NewPassword123"
		},
		new ConnectionFactory()
		{
			HostName = "rabbitmq-3",
			Port = 5672,
			UserName = "sergej",
			Password = "NewPassword123"
		},
	};
	return factories;
});
if (enableBackgroundService != null && enableBackgroundService.Equals("True")) 
{
	builder.Services.AddHostedService<RabbitMQConsumer>();
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	using (var serviceScope = app.Services
	.GetRequiredService<IServiceScopeFactory>()
	.CreateScope())
	{
		using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
		{
			context.Database.Migrate();
		}
	}
	app.UseSwagger();
    app.UseSwaggerUI();
}

// global cors policy
app.UseCors(x => x
	.SetIsOriginAllowed(origin => true)
	.AllowAnyMethod()
	.AllowAnyHeader()
	.AllowCredentials());

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.Run();
