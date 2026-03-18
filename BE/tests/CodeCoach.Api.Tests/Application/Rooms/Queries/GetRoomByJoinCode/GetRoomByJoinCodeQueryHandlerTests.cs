using System;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using Xunit;

using CodeCoach.Application.Interfaces;
using CodeCoach.Application.Rooms.Queries.GetRoomByJoinCode;
using CodeCoach.Domain.Entities;

namespace CodeCoach.Api.Tests.Application.Rooms.Queries.GetRoomByJoinCode;

public class GetRoomByJoinCodeQueryHandlerTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly GetRoomByJoinCodeQueryHandler _handler;

    public GetRoomByJoinCodeQueryHandlerTests()
    {
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _handler = new GetRoomByJoinCodeQueryHandler(_roomRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRoomExists_ReturnsRoomDto()
    {
        var joinCode = "TEST01";
        var query = new GetRoomByJoinCodeQuery(joinCode);
        var room = new Room("Test Room", joinCode, Guid.NewGuid(), Guid.NewGuid());

        _roomRepositoryMock
            .Setup(x => x.GetByJoinCodeAsync(joinCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Test Room", result.Name);
    }

    [Fact]
    public async Task Handle_WhenRoomNotExists_ReturnsNull()
    {
        var query = new GetRoomByJoinCodeQuery("NOTEXIST");

        _roomRepositoryMock
            .Setup(x => x.GetByJoinCodeAsync("NOTEXIST", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Null(result);
    }
}
