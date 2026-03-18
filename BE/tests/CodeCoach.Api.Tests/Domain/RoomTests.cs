using System;

using Xunit;

using CodeCoach.Domain.Entities;
using CodeCoach.Domain.Enums;

namespace CodeCoach.Api.Tests.Domain;

public class RoomTests
{
    [Fact]
    public void Room_Creation_SetsDefaults_AndTimestamps()
    {
        var mentorId = Guid.NewGuid();
        var room = new Room("Math 101", "ABCD12", mentorId, mentorId);

        Assert.NotEqual(Guid.Empty, room.Id);
        Assert.Equal("Math 101", room.Name);
        Assert.Equal("ABCD12", room.JoinCode);
        Assert.Equal(mentorId, room.MentorId);
        Assert.Equal(mentorId, room.CreatedById);
        Assert.Equal(RoomStatus.Active, room.Status);
        Assert.Equal(RoomMode.Broadcast, room.CurrentMode);
        Assert.NotEqual(default, room.CreatedAt);
        Assert.Null(room.ClosedAt);
    }

    [Fact]
    public void Room_Creation_Throws_WhenRequiredFieldsMissing()
    {
        var mentorId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => new Room(" ", "ABCD12", mentorId, mentorId));
        Assert.Throws<ArgumentException>(() => new Room("Math 101", "  ", mentorId, mentorId));
        Assert.Throws<ArgumentException>(() => new Room("Math 101", "ABCD12", Guid.Empty, mentorId));
        Assert.Throws<ArgumentException>(() => new Room("Math 101", "ABCD12", mentorId, Guid.Empty));
    }
}
