using DiscordLite.Application.Abstractions;
using DiscordLite.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DiscordLite.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        try
        {
           return await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsDirectConversationUniqueViolation(ex))
        {
            throw new DirectConversationAlreadyExistsException();
        }
    }


    private static bool IsDirectConversationUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException postgresException
               && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
               && postgresException.ConstraintName == "Conversation_DirectUsers";
    }
}