namespace CodeCoach.Application.Abstractions;

public interface ITransactionManager
{
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);

    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
