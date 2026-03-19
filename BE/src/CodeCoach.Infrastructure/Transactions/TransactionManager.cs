using Microsoft.EntityFrameworkCore;

using CodeCoach.Application.Abstractions;
using CodeCoach.Infrastructure.Data;

namespace CodeCoach.Infrastructure.Transactions;

public class TransactionManager : ITransactionManager
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionManager(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        if (!_dbContext.Database.IsRelational() || _dbContext.Database.CurrentTransaction is not null)
        {
            return await operation(cancellationToken);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async innerCancellationToken =>
        {
            await operation(innerCancellationToken);
            return true;
        }, cancellationToken);
    }
}
