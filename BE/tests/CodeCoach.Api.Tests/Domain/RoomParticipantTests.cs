using System;

using Xunit;

using CodeCoach.Domain.Entities;
using CodeCoach.Domain.Enums;

namespace CodeCoach.Api.Tests.Domain;

public class RoomParticipantTests
{
    [Fact]
    public void RoomParticipant_Creation_SetsRoleStatus()
    {
        var participant = new RoomParticipant(Guid.NewGuid(), Guid.NewGuid(), RoomRole.Student);

        Assert.Equal(RoomRole.Student, participant.Role);
        Assert.Equal(ParticipantStatus.Active, participant.Status);
        Assert.NotEqual(default, participant.JoinedAt);
    }

    [Fact]
    public void RoomParticipant_Creation_Throws_WhenIdsInvalid()
    {
        Assert.Throws<ArgumentException>(() => new RoomParticipant(Guid.Empty, Guid.NewGuid(), RoomRole.Student));
        Assert.Throws<ArgumentException>(() => new RoomParticipant(Guid.NewGuid(), Guid.Empty, RoomRole.Student));
    }
}
