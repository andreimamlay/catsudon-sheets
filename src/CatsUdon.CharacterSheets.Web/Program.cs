using CatsUdon.CharacterSheets;
using System.Text.Encodings.Web;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddRazorPages();
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddCharacterSheets();
builder.Services.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
});

var app = builder.Build();

app.MapStaticAssets();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.Run();
