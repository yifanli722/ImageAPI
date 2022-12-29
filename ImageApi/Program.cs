using WebApplication2;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Development.json");
builder.Services.AddTransient<IPostgresAccess, PostgresAccess>();
builder.Services.AddTransient<UploadImageController>();
var app = builder.Build();
var serviceProvider = builder.Services.BuildServiceProvider();
var controller = serviceProvider.GetService<UploadImageController>();

RetrieveImageDelegate retrieveImageDelegate = new RetrieveImageDelegate(controller.RetrieveImage);
UploadImageDelegate imageUploadDelegate = new UploadImageDelegate(controller.UploadImage);

app.MapGet("/api/RetrieveImage/{imageHash}", retrieveImageDelegate);
app.MapPost("/api/ImageUpload/", imageUploadDelegate);

app.Run($"http://localhost:{builder.Configuration["ApiPort"]}");

delegate Task<IResult> RetrieveImageDelegate(string imageHash);
delegate Task<string> UploadImageDelegate(HttpRequest request);