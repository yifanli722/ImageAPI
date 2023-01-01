using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace WebApplication2;

public enum ImageType
{
    Unknown,
    Png,
    Gif,
    Jpeg,
    Bmp,
    Tiff
}

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
        if (imageData is null)
        {
            return Results.NotFound(new
            {
                Error = $"No file found with hash {imageHash}"
            });
        }
        ImageType type = GetContentType(imageData);
        return type is ImageType.Gif or ImageType.Png or ImageType.Jpeg
            ? Results.File(imageData, $"image/{type}")
            : Results.File(imageData);

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

        ImageType type = GetContentType(imageData);
        switch (type)
        {
            case ImageType.Gif:
            case ImageType.Jpeg:
            case ImageType.Png:
                string sha256Hash = await _postgresAccess.UploadImage(imageData);
                return Results.Ok(new
                {
                    sha256Hash,
                    retrieveUrl = $"/api/RetrieveImage/{sha256Hash}"
                });
            case ImageType.Bmp:
            case ImageType.Tiff:
            case ImageType.Unknown:
            default:
                return Results.BadRequest(new
                {
                    Error = "File uploaded is not of GIF, Jpeg/Jpg, or PNG"
                });
        }
    }

    public async Task<IResult> DeleteImage(string imageHash)
    {
        int rowsAffected = await _postgresAccess.DeleteImage(imageHash);
        return Results.Ok(new
        {
            rowsAffected
        });
    }
    
    private ImageType GetContentType(byte[] bytes)
    {
        if (bytes.Length < 4)
            return ImageType.Unknown;
        
        // check the magic numbers
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
            return ImageType.Png;
        
        if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
            return ImageType.Gif;
        
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return ImageType.Jpeg;
        
        if (bytes[0] == 0x42 && bytes[1] == 0x4D)
            return ImageType.Bmp;
        
        if (bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2A && bytes[3] == 0x00)
            return ImageType.Tiff;
        
        if (bytes[0] == 0x4D && bytes[1] == 0x4D && bytes[2] == 0x00 && bytes[3] == 0x2A)
            return ImageType.Tiff;
        

        return ImageType.Unknown;
    }

}