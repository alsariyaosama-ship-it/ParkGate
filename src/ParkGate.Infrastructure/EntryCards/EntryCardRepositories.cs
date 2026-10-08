using System.Data;
using Microsoft.Data.SqlClient;
using ParkGate.Application.EntryCards;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Security;
using ParkGate.Domain.EntryCards;

namespace ParkGate.Infrastructure.EntryCards;

public sealed class EntryCardRepository : IEntryCardRepository
{
    private readonly IAppConnectionStringProvider _connectionStringProvider;

    public EntryCardRepository(IAppConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    public async Task<EntryCardRecord?> FindAsync(Guid cardId, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionStringProvider.CurrentConnectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT BeneficiaryId, IsRevoked, PermitActive FROM dbo.vw_EntryCardLookup WHERE CardId = @CardId";
        command.Parameters.AddWithValue("@CardId", cardId);

        await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return new EntryCardRecord(
                cardId,
                reader.GetGuid(0),
                reader.GetBoolean(1),
                reader.GetBoolean(2));
        }

        return null;
    }

    public async Task ReplaceActiveCardAsync(Guid beneficiaryId, Guid cardId, byte keyId, DateOnly expiresOn, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionStringProvider.CurrentConnectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "dbo.usp_IssueEntryCard";
        command.Parameters.AddWithValue("@BeneficiaryId", beneficiaryId);
        command.Parameters.AddWithValue("@CardId", cardId);
        command.Parameters.AddWithValue("@KeyId", keyId);
        command.Parameters.AddWithValue("@ExpiresOn", expiresOn.ToDateTime(TimeOnly.MinValue));

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    public async Task RevokeAsync(Guid cardId, string reason, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionStringProvider.CurrentConnectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "dbo.usp_RevokeEntryCard";
        command.Parameters.AddWithValue("@CardId", cardId);
        command.Parameters.AddWithValue("@Reason", reason);

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}

public sealed class QrKeyRepository : IQrKeyRepository
{
    private readonly IAppConnectionStringProvider _connectionStringProvider;

    public QrKeyRepository(IAppConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    public async Task<IReadOnlyList<QrPublicKey>> GetTrustedKeysAsync(CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionStringProvider.CurrentConnectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT KeyId, PublicKeySpki FROM dbo.QrSigningKeys WHERE RevokedAt IS NULL";

        var keys = new List<QrPublicKey>();
        await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            keys.Add(new QrPublicKey(reader.GetByte(0), (byte[])reader.GetValue(1)));
        }

        return keys;
    }

    public async Task<byte> RegisterAsync(byte[] subjectPublicKeyInfo, string stationName, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionStringProvider.CurrentConnectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "dbo.usp_RegisterQrSigningKey";
        command.Parameters.AddWithValue("@PublicKeySpki", subjectPublicKeyInfo);
        command.Parameters.AddWithValue("@StationName", stationName);
        
        var outParam = new SqlParameter("@KeyId", SqlDbType.TinyInt) { Direction = ParameterDirection.Output };
        command.Parameters.Add(outParam);

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        return (byte)outParam.Value;
    }
}

public sealed class CardScanLogRepository : ICardScanLog
{
    private readonly IAppConnectionStringProvider _connectionStringProvider;

    public CardScanLogRepository(IAppConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    public async Task WriteAsync(CardScanLogEntry entry, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionStringProvider.CurrentConnectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO dbo.CardScanLog (StationName, Status, CardId, RawPrefix) VALUES (@StationName, @Status, @CardId, @RawPrefix)";
        command.Parameters.AddWithValue("@StationName", entry.StationName);
        command.Parameters.AddWithValue("@Status", (byte)entry.Status);
        command.Parameters.AddWithValue("@CardId", (object?)entry.CardId ?? DBNull.Value);
        command.Parameters.AddWithValue("@RawPrefix", (object?)entry.RawPrefix ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}
