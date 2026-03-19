using MediatR;

using CodeCoach.Application.Abstractions;
using CodeCoach.Application.DTOs;
using CodeCoach.Application.Exceptions;
using CodeCoach.Application.Interfaces;
using CodeCoach.Domain.Entities;

namespace CodeCoach.Application.Auth.Commands.RefreshSession;

public class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, RefreshSessionResultDto>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenProvider _refreshTokenProvider;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ITransactionManager _transactionManager;

    public RefreshSessionCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IRefreshTokenProvider refreshTokenProvider,
        IJwtTokenGenerator jwtTokenGenerator,
        ITransactionManager transactionManager)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _refreshTokenProvider = refreshTokenProvider;
        _jwtTokenGenerator = jwtTokenGenerator;
        _transactionManager = transactionManager;
    }

    public async Task<RefreshSessionResultDto> Handle(
        RefreshSessionCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedException("Refresh token is required.");
        }

        var tokenHash = _refreshTokenProvider.ComputeHash(request.RefreshToken);
        var existingRefreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingRefreshToken is null || !existingRefreshToken.IsActive)
        {
            throw new UnauthorizedException("Refresh token is invalid or expired.");
        }

        var user = await _userRepository.GetByIdAsync(existingRefreshToken.UserId, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Refresh token is invalid or expired.");
        }

        return await _transactionManager.ExecuteAsync(async transactionCancellationToken =>
        {
            var newRefreshTokenDescriptor = _refreshTokenProvider.Generate();
            var newRefreshToken = new RefreshToken(
                user.Id,
                newRefreshTokenDescriptor.TokenHash,
                newRefreshTokenDescriptor.ExpiresAtUtc);

            existingRefreshToken.Revoke(newRefreshToken.Id);

            await _refreshTokenRepository.AddAsync(newRefreshToken, transactionCancellationToken);
            await _refreshTokenRepository.UpdateAsync(existingRefreshToken, transactionCancellationToken);

            var accessToken = _jwtTokenGenerator.Generate(user);

            return new RefreshSessionResultDto(
                accessToken.AccessToken,
                accessToken.ExpiresAtUtc,
                newRefreshTokenDescriptor.Token,
                newRefreshTokenDescriptor.ExpiresAtUtc);
        }, cancellationToken);
    }
}
