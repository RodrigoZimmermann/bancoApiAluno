using Microsoft.EntityFrameworkCore;
using bancoApiAluno;

var builder = WebApplication.CreateBuilder(args);

// Configura o DbContext com a string de conexão
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aplicar migrações automaticamente ao iniciar a aplicação
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseCors();

// Endpoints da API para Alunos
app.MapGet("/alunos", async (AppDbContext db) =>
{
    return Results.Ok(await db.Alunos.ToListAsync());
});

app.MapGet("/alunos/{id}", async (int id, AppDbContext db) =>
{
    var aluno = await db.Alunos.FindAsync(id);

    if (aluno == null)
    {
        return Results.NotFound("Aluno não encontrado.");
    }

    return Results.Ok(aluno);
});

app.MapPost("/alunos", async (Aluno aluno, AppDbContext db) =>
{
    db.Alunos.Add(aluno);
    await db.SaveChangesAsync();
    return Results.Created($"/alunos/{aluno.Id}", aluno);
});

app.MapPut("/alunos/{id}", async (int id, Aluno inputAluno, AppDbContext db) =>
{
    var aluno = await db.Alunos.FindAsync(id);

    if (aluno == null)
    {
        return Results.NotFound();
    }

    aluno.Nome = inputAluno.Nome;
    aluno.Email = inputAluno.Email;
    aluno.Senha = inputAluno.Senha;
    aluno.Turma = inputAluno.Turma;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/alunos/{id}", async (int id, AppDbContext db) =>
{
    var aluno = await db.Alunos.FindAsync(id);

    if (aluno == null)
    {
        return Results.NotFound();
    }

    db.Alunos.Remove(aluno);
    await db.SaveChangesAsync();
    return Results.Ok(aluno);
});

app.MapGet("/alunos/{email}/{senha}", async (string email, string senha, AppDbContext db) =>
{
    var aluno = await db.Alunos
                        .Where(a => a.Email == email && a.Senha == senha)
                        .OrderByDescending(a => a.Id)
                        .FirstOrDefaultAsync();

    if (aluno == null)
    {
        return Results.NotFound("Aluno não encontrado.");
    }

    return Results.Ok(aluno.Id);
});


app.MapPost("/alunos/logado", async (AlunoLogado alunoLogadoRequest, AppDbContext db) =>
{
    // Limpe a tabela AlunoLogado
    var allAlunoLogado = db.AlunoLogado.ToList();
    db.AlunoLogado.RemoveRange(allAlunoLogado);

    // Adicione o novo AlunoLogado (sem definir explicitamente o LogadoId)
    db.AlunoLogado.Add(alunoLogadoRequest);
    await db.SaveChangesAsync();
});

app.MapGet("/alunos/logado", async (AppDbContext db) =>
{
    var logado = await db.AlunoLogado.FirstOrDefaultAsync();
    if (logado == null)
    {
        return Results.NotFound(0);
    }
    return Results.Ok(logado.Id);
});

app.Run();

public partial class Program { }
