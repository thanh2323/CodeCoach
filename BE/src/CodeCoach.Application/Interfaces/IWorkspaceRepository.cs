using System;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Application.Interfaces;

public interface IWorkspaceRepository
{
    Task<Workspace?> GetWorkspaceAsync(Guid roomId, Guid? userId, CancellationToken cancellationToken = default);
    Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default);
    Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default);
}
