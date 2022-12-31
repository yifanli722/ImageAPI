using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace WebApplication2;

public class UploadImageController
{
    private readonly IPostgresAccess _postgresAccess;
    
    public UploadImageController(IPostgresAccess postgresAccess)
    {
        _postgresAccess = postgresAccess;
    }
    
    public async Task<IResult> RetrieveImage(string imageHash)
    {
        byte[]? imageData = await _postgresAccess.RetrieveImage(imageHash);
        return imageData is null ? 
            Results.NotFound(new
            {
                Error = $"No file found with hash {imageHash}"
            }) : 
            Results.File(imageData, "image/jpeg");
    }
    
    public async Task<IResult> UploadImage(HttpRequest request)
    {

        if (request.ContentLength is null or 0)
        {
            return Results.BadRequest(new
            {
                Error = "Request body is empty."
            });
        }
        
        var stream = request.Body;
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        byte[] imageData = memoryStream.ToArray();

        string sha256Hash = await _postgresAccess.UploadImage(imageData);
        return Results.Ok(new
        {
            sha256Hash,
            retrieveUrl = $"/api/RetrieveImage/{sha256Hash}"
        });
    }
}