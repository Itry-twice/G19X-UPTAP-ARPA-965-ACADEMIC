using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PluriOne.Shared.Data;

/// <summary>
/// Establece las variables de sesión PostgreSQL `app.user_id` y `app.role`
/// al abrir la conexión (para RLS) y las limpia al cerrarla, evitando fugas
/// entre peticiones con connection pooling.
/// </summary>
public sealed class PostgresUserContextInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        => Apply(connection);

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await ApplyAsync(connection, cancellationToken);
    }

    public override void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData)
        => Clear(connection);

    public override async Task ConnectionClosedAsync(DbConnection connection, ConnectionEndEventData eventData)
    {
        await ClearAsync(connection);
    }

    private static void Apply(DbConnection connection)
    {
        var userId = DbUserContext.UserId;
        var role = DbUserContext.Role;

        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "SELECT set_config('app.user_id', @uid, false), set_config('app.role', @role, false);";
        AddParam(cmd, "@uid", string.IsNullOrEmpty(userId) ? null : userId);
        AddParam(cmd, "@role", string.IsNullOrEmpty(role) ? null : role);
        cmd.ExecuteNonQuery();
    }

    private static async Task ApplyAsync(DbConnection connection, CancellationToken ct)
    {
        var userId = DbUserContext.UserId;
        var role = DbUserContext.Role;

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "SELECT set_config('app.user_id', @uid, false), set_config('app.role', @role, false);";
        AddParam(cmd, "@uid", string.IsNullOrEmpty(userId) ? null : userId);
        AddParam(cmd, "@role", string.IsNullOrEmpty(role) ? null : role);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void Clear(DbConnection connection)
    {
        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "RESET app.user_id; RESET app.role;";
            cmd.ExecuteNonQuery();
        }
        catch
        {
            // La conexión puede estar rota al cerrarse; no propagar.
        }
    }

    private static async Task ClearAsync(DbConnection connection)
    {
        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "RESET app.user_id; RESET app.role;";
            await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            // La conexión puede estar rota al cerrarse; no propagar.
        }
    }

    private static void AddParam(DbCommand cmd, string name, string? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = (object?)value ?? DBNull.Value;
        p.DbType = DbType.String;
        cmd.Parameters.Add(p);
    }
}
