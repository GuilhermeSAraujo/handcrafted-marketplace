var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddNpgsqlDataSource("Host=hmdatabase.postgres.database.azure.com;Port=5432;Username=hm_admin;Password=fKS701B3%:\\|;Database=hmdb;Trust Server Certificate=true;");

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
