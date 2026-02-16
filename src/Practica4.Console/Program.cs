using System.Net.Http.Json;

var baseUrl = "http://localhost:5032"; // <- tu API está aquí
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

Console.WriteLine($"Consumiendo API en: {baseUrl}");

while (true)
{
    Console.WriteLine("\n=== PRACTICA 4 - CONSUMIDOR API ===");
    Console.WriteLine("1) Health Live");
    Console.WriteLine("2) Health DB");
    Console.WriteLine("3) Crear User (POST /users)");
    Console.WriteLine("4) Listar Users (GET /users)");
    Console.WriteLine("0) Salir");
    Console.Write("Opción: ");
    var opt = Console.ReadLine();

    try
    {
        switch (opt)
        {
            case "1":
                await HealthLive(http);
                break;

            case "2":
                await HealthDb(http);
                break;

            case "3":
                await CreateUser(http);
                break;

            case "4":
                await ListUsers(http);
                break;

            case "0":
                return;

            default:
                Console.WriteLine("Opción inválida.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {ex.Message}");
    }
}

static async Task HealthLive(HttpClient http)
{
    var res = await http.GetAsync("/health/live");
    Console.WriteLine($"Status: {(int)res.StatusCode} {res.StatusCode}");
    Console.WriteLine(await res.Content.ReadAsStringAsync());
}

static async Task HealthDb(HttpClient http)
{
    var res = await http.GetAsync("/health/db");
    Console.WriteLine($"Status: {(int)res.StatusCode} {res.StatusCode}");
    Console.WriteLine(await res.Content.ReadAsStringAsync());
}

static async Task CreateUser(HttpClient http)
{
    Console.Write("Username: ");
    var username = Console.ReadLine()?.Trim();

    Console.Write("Password: ");
    var password = Console.ReadLine()?.Trim();

    var payload = new CreateUserRequest(username ?? "", password ?? "");

    var res = await http.PostAsJsonAsync("/users", payload);
    Console.WriteLine($"Status: {(int)res.StatusCode} {res.StatusCode}");
    Console.WriteLine(await res.Content.ReadAsStringAsync());
}

static async Task ListUsers(HttpClient http)
{
    // Podemos leer como texto para ver el JSON tal cual lo devuelve tu API
    var res = await http.GetAsync("/users");
    Console.WriteLine($"Status: {(int)res.StatusCode} {res.StatusCode}");
    Console.WriteLine(await res.Content.ReadAsStringAsync());

    // Si quieres deserializar formalmente, descomenta esto y usa el record UserDto:
    // var users = await http.GetFromJsonAsync<List<UserDto>>("/users");
    // if (users is not null)
    //     foreach (var u in users)
    //         Console.WriteLine($"{u.id} | {u.username} | {u.created_at}");
}

record CreateUserRequest(string Username, string Password);

// Úsalo si decides deserializar tipado (List<UserDto>)
// record UserDto(int id, string username, DateTime created_at);