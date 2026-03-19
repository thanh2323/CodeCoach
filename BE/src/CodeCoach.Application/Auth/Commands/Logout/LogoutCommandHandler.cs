using MediatR;

using CodeCoach.Application.Abstractions;
using CodeCoach.Application.Interfaces;

namespace CodeCoach.Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenProvider _refreshTokenProvider;
    private readonly ITransactionManager _transactionManager;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenProvider refreshTokenProvider,
        ITransactionManager transactionManager)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenProvider = refreshTokenProvider;
        _transactionManager = transactionManager;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Unit.Value;
        }

        var tokenHash = _refreshTokenProvider.ComputeHash(request.RefreshToken);
        var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return Unit.Value;
        }

        await _transactionManager.ExecuteAsync(async transactionCancellationToken =>
        {
            refreshToken.Revoke();
            await _refreshTokenRepository.UpdateAsync(refreshToken, transactionCancellationToken);
        }, cancellationToken);

        return Unit.Value;
    }
}
