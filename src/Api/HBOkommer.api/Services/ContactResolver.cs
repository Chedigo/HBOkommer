using Microsoft.Data.SqlClient;

namespace HboKommer.Api.Services;

public sealed class ContactResolver
{
    public async Task<(bool Found, string? PhoneE164, string Source)> TryResolveAsync(
        string connectionString,
        string subjectRef)
    {
        const string sql = @"
SELECT TOP 1 PhoneNumberE164, Source
FROM dbo.ContactMappings
WHERE SubjectRef = @SubjectRef AND IsActive = 1;";

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SubjectRef", subjectRef);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return (false, null, "NONE");

        var phone = reader.GetString(0);
        var source = reader.GetString(1);

        return (true, phone, source);
    }
}
