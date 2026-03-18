using Microsoft.EntityFrameworkCore;

using CodeCoach.Domain.Entities;
using CodeCoach.Application.Interfaces;
using CodeCoach.Infrastructure.Data;

namespace CodeCoach.Infrastructure.Data.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly ApplicationDbContext _dbContext;

    public WorkspaceRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Workspace?> GetWorkspaceAsync(
        Guid roomId,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Workspaces
            .AsNoTracking()
            .FirstOrDefaultAsync(
                workspace => workspace.RoomId == roomId && workspace.UserId == userId,
                cancellationToken);
    }

    public async Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        await _dbContext.Workspaces.AddAsync(workspace, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return workspace;
    }

    public async Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        _dbContext.Workspaces.Update(workspace);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
