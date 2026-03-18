using System;

namespace CodeCoach.Domain.Entities;

public class Workspace
{
    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Language { get; private set; }
    public string SourceCode { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Workspace(Guid roomId, Guid? userId, string language, string sourceCode)
    {
        if (roomId == Guid.Empty)
        {
            throw new ArgumentException("RoomId is required.", nameof(roomId));
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            throw new ArgumentException("Language is required.", nameof(language));
        }

        if (sourceCode is null)
        {
            throw new ArgumentNullException(nameof(sourceCode));
        }

        Id = Guid.NewGuid();
        RoomId = roomId;
        UserId = userId;
        Language = language;
        SourceCode = sourceCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSnapshot(string language, string sourceCode)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            throw new ArgumentException("Language is required.", nameof(language));
        }

        if (sourceCode is null)
        {
            throw new ArgumentNullException(nameof(sourceCode));
        }

        Language = language;
        SourceCode = sourceCode;
        UpdatedAt = DateTime.UtcNow;
    }
}
