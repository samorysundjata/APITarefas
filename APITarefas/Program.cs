using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>( opt=>
    opt.UseInMemoryDatabase("TarefasDb"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Ol? Mundo!");

app.MapGet("frases", async () => 
    await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes"));

app.MapGet("/tarefas", async (AppDbContext db) => await db.Tarefas.ToListAsync());

app.MapGet("/tarefas/{id}", async(int id, AppDbContext db) => 
    await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa): Results.NotFound());

app.MapGet("/tarefas/concluida", async (AppDbContext db) => 
                                 await db.Tarefas.Where(t => t.IsConcluida).ToListAsync());

app.MapPost("/tarefas", async (Tarefa tarefa, AppDbContext db) => 
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
});

app.Run();

class Tarefa
{
    public int Id { get; set; }

    public string? Nome { get; set; }

    public bool IsConcluida { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}