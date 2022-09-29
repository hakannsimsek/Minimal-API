using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Data;
using MinimalAPI.Dtos;
using MinimalAPI.Models;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var sqlConnectionBuilder = new SqlConnectionStringBuilder();
sqlConnectionBuilder.ConnectionString = builder.Configuration.GetConnectionString("SQLDbConnection");
sqlConnectionBuilder.UserID = "sa";
sqlConnectionBuilder.Password = "as";

builder.Services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(sqlConnectionBuilder.ConnectionString));
builder.Services.AddScoped<ICommandRepo, CommandRepo>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("api/v1/commands", async (ICommandRepo repo, IMapper mapper) =>
{
    var commands = await repo.GetAllCommands();
    return Results.Ok(mapper.Map<IEnumerable<CommandReadDto>>(commands));
});

app.MapGet("api/v1/commands/{id}", async (ICommandRepo repo, IMapper mapper, int id) =>
{
    var command = await repo.GetCommandById(id);
    if (command != null)
    {
        return Results.Ok(mapper.Map<CommandReadDto>(command));
    }
    return Results.NotFound();
});

app.MapPost("api/v1/commands", async (ICommandRepo repo, IMapper mapper, CommandCreateDto cmdCreateDto) =>
{
    var commandModel = mapper.Map<Command>(cmdCreateDto);
    await repo.CreateCommands(commandModel);
    await repo.SaveChanges();
    var cmdReadDto = mapper.Map<CommandReadDto>(commandModel);
    return Results.Created($"api/v1/commands/{cmdReadDto.Id}", cmdReadDto);
});

app.MapPut("api/v1/commands/{id}", async (ICommandRepo repo, IMapper mapper, int id, CommandUpdateDto cmdUpdateDto) =>
{
    var command = await repo.GetCommandById(id);
    if (command == null)
    {
        return Results.NotFound();
    }
    mapper.Map(cmdUpdateDto, command);
    await repo.SaveChanges();
    return Results.Ok();
});

app.MapDelete("api/v1/commands/{id}", async (ICommandRepo repo, IMapper mapper, int id) =>
{
    var command = await repo.GetCommandById(id);
    if (command == null)
    {
        return Results.NotFound();
    }
    repo.DeleteCommand(command);
    await repo.SaveChanges();
    return Results.Ok();
});

app.Run();
