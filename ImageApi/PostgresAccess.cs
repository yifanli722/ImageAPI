using System.Security.Cryptography;
using Npgsql;
using NpgsqlTypes;

namespace WebApplication2;

public interface IPostgresAccess
{
    Task<string> UploadImage(byte[] imageData);
    Task<byte[]?> RetrieveImage(string imageHash);
}

public class PostgresAccess : IPostgresAccess
{
    private readonly string _connectionStr;
    
    public PostgresAccess(IConfiguration configuration)
    {
        _connectionStr = configuration.GetSection("Postgres").GetValue<string>("ConnectionString");
        if (string.IsNullOrEmpty(_connectionStr))
        {
            throw new Exception("appsettings postgres connection string is null");
        }
    }

    public async Task<string> UploadImage(byte[] imageData)
    {
        string hash = GetSha256Hash(imageData);
        string sql = await File.ReadAllTextAsync("./Postgres_Scripts/Insert_Image.sql");
        await using var conn = new NpgsqlConnection(_connectionStr);
        conn.Open();
        await using (var cmd = new NpgsqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@img_hash_full", hash);
            cmd.Parameters.AddWithValue("@img_hash_partial", hash[..10]);
            cmd.Parameters.AddWithValue("@img_data", NpgsqlDbType.Bytea, imageData);
            cmd.ExecuteNonQuery();
        }
        
        await conn.CloseAsync();

        return hash;
    }

    public async Task<byte[]?> RetrieveImage(string imageHash)
    {
        string sql = await File.ReadAllTextAsync("./Postgres_Scripts/Retrieve_Image.sql");
        using var conn = new NpgsqlConnection(_connectionStr);
        conn.Open();
        using (var cmd = new NpgsqlCommand(sql, conn))
        {
            cmd.Connection = conn;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@img_hash_full", NpgsqlDbType.Varchar, imageHash);
            
            using (var reader = cmd.ExecuteReader())
            {
                byte[]? imageData = null;
                while (reader.Read())
                {
                    Stream stream = reader.GetStream(2);
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    imageData = memoryStream.ToArray();
                }

                return imageData;
            }
        }
    }

    private string GetSha256Hash(byte[] data)
    {
        using var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}