using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// OpenAPI solo en Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

/* =========================================================
   HEALTH
========================================================= */

// Liveness: app viva (no toca DB)
app.MapGet("/health/live", () =>
{
    return Results.Ok(new
    {
        status = "live",
        env = app.Environment.EnvironmentName,
        utc = DateTime.UtcNow
    });
});

// Readiness (DB): valida conexión real + query simple
app.MapGet("/health/db", async () =>
{
    var cs = app.Configuration.GetConnectionString("SqlServer");
    if (string.IsNullOrWhiteSpace(cs))
        return Results.Problem("Falta ConnectionStrings:SqlServer");

    try
    {
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("SELECT 1", conn);
        var result = await cmd.ExecuteScalarAsync();

        return Results.Ok(new { status = "ready", db = "ok", result });
    }
    catch (Exception ex)
    {
        return Results.Problem($"DB not ready: {ex.Message}");
    }
});

/* =========================================================
   USERS (texto plano) - Práctica 4
========================================================= */

// Insert inicial / alta de usuario (sin hash)
app.MapPost("/users", async (CreateUserRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Username y Password son obligatorios.");

    var cs = app.Configuration.GetConnectionString("SqlServer");
    if (string.IsNullOrWhiteSpace(cs))
        return Results.Problem("Falta ConnectionStrings:SqlServer");

    try
    {
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        // 1) Evitar duplicado de username
        const string existsSql = "SELECT COUNT(1) FROM dbo.users WHERE username = @username;";
        await using var existsCmd = new SqlCommand(existsSql, conn);
        existsCmd.Parameters.AddWithValue("@username", req.Username);

        var existsObj = await existsCmd.ExecuteScalarAsync();
        var exists = Convert.ToInt32(existsObj);

        if (exists > 0)
            return Results.Conflict("Ese username ya existe.");

        // 2) Insert
        const string insertSql = @"
            INSERT INTO dbo.users (username, password, created_at)
            VALUES (@username, @password, GETDATE());

            SELECT CAST(SCOPE_IDENTITY() AS INT);
        ";

        await using var cmd = new SqlCommand(insertSql, conn);
        cmd.Parameters.AddWithValue("@username", req.Username);
        cmd.Parameters.AddWithValue("@password", req.Password);

        var newIdObj = await cmd.ExecuteScalarAsync();
        var newId = Convert.ToInt32(newIdObj);

        return Results.Created($"/users/{newId}", new { id = newId, username = req.Username });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al insertar usuario: {ex.Message}");
    }
});

// (Opcional pero útil) Listar usuarios para validar inserciones
app.MapGet("/users", async () =>
{
    var cs = app.Configuration.GetConnectionString("SqlServer");
    if (string.IsNullOrWhiteSpace(cs))
        return Results.Problem("Falta ConnectionStrings:SqlServer");

    var users = new List<object>();

    await using var conn = new SqlConnection(cs);
    await conn.OpenAsync();

    const string sql = "SELECT id, username, created_at FROM dbo.users ORDER BY id DESC;";
    await using var cmd = new SqlCommand(sql, conn);
    await using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        users.Add(new
        {
            id = reader.GetInt32(0),
            username = reader.GetString(1),
            created_at = reader.GetDateTime(2)
        });
    }

    return Results.Ok(users);
});

/* =========================================================
   WEATHERFORECAST (template)
========================================================= */

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

/* =========================================================
   TYPE DECLARATIONS (al final para evitar CS8803)
========================================================= */

record CreateUserRequest(string Username, string Password);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
