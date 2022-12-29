using System.Collections.Specialized;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Npgsql;
using NpgsqlTypes;

namespace WebApplication2;

public class UploadImageController
{
    private readonly IPostgresAccess _postgresAccess;
    
    public UploadImageController(IPostgresAccess postgresAccess)
    {
        _postgresAccess = postgresAccess;
    }
    
    private static async Task<IResult> HandleImageUpload(HttpRequest request)
    {
        var stream = request.Body;
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        byte[] bytes = memoryStream.ToArray();
        return Results.File(bytes, "image/jpeg");
    }
    
    public async Task<IResult> RetrieveImage(string imageHash)
    {
        byte[]? imageData = await _postgresAccess.RetrieveImage(imageHash);
        return imageData is null ? 
            Results.NotFound("Unable to find data") : 
            Results.File(imageData, "image/jpeg");
    }
    
    public async Task<string> UploadImage(HttpRequest request)
    {
        if (!request.ContentLength.HasValue) return "No binary data in body";
        
        var stream = request.Body;
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        byte[] imageData = memoryStream.ToArray();

        return await _postgresAccess.UploadImage(imageData);
    }
}