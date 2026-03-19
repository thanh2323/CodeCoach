# CodeCoach Implementation Slices

This file breaks the current backend roadmap into small vertical slices so we can implement the project incrementally with minimal rewrite risk.

## Principles

- Each slice should end in a buildable and testable state.
- Finish infrastructure cleanup before adding more endpoints.
- Prefer vertical delivery over broad horizontal refactors.
- Keep controllers thin and route requests through CQRS handlers.
- Keep handlers thin: handlers coordinate requests, but non-trivial orchestration should move into dedicated application services.
- Push domain invariants and state validation into domain entities or domain-level rules when the rule belongs to the model itself.
- Introduce a service when a use case spans multiple repositories, aggregates, or business steps.

## Recommended Order

1. Application DI cleanup
2. Rooms API slice
3. Join room slice
4. Workspace read/write slice
5. Mentor room queries slice
6. Close room slice
7. Users minimal slice
8. SignalR foundation slice

## Slice 1: Application DI Cleanup

**Scope**
- Create `AddApplicationServices()` in the Application layer.
- Register MediatR in Application instead of Infrastructure.
- Register `IJoinCodeGenerator` in Application.
- Keep repository and DbContext registration in Infrastructure.
- Update `Program.cs` to call both `AddApplicationServices()` and `AddInfrastructureServices()`.

**Depends on**
- Task 5
- Task 6
- Task 7 partial CQRS implementation

**Done when**
- API builds successfully.
- MediatR handlers resolve from DI.
- Infrastructure no longer owns Application service registration.

## Slice 2: Rooms API Slice

**Scope**
- Add `RoomsController`.
- Add API request contract for room creation.
- Implement:
  - `POST /api/v1/rooms`
  - `GET /api/v1/rooms/{joinCode}`
- Map requests to:
  - `CreateRoomCommand`
  - `GetRoomByJoinCodeQuery`
- Add controller tests.

**Depends on**
- Slice 1

**Done when**
- Room can be created via HTTP.
- Room can be fetched by `joinCode` via HTTP.
- Controller tests pass.

## Slice 3: Join Room Slice

**Scope**
- Implement `JoinRoomCommand` and a dedicated `JoinRoomService`.
- Keep the handler as a thin delegator to the service.
- Move room state validation into domain behavior such as `Room.EnsureActive()`.
- Validate room exists and user exists.
- Prevent invalid join scenarios as needed for MVP.
- Create a `RoomParticipant` with role `Student`.
- Create a default `Workspace` for the joining student.
- Return a room details DTO suitable for the first student entry flow.
- Add service tests for happy path and core failure paths.
- Keep handler tests focused on delegation and wiring.

**Depends on**
- Slice 1
- Repositories for `Room`, `RoomParticipant`, and `Workspace`

**Done when**
- Student can join a room by `joinCode`.
- Participant and workspace records are created.
- Service tests and handler tests pass.

## Slice 4: Workspace Read/Write Slice

**Scope**
- Implement `GetWorkspaceQuery`.
- Implement `SaveWorkspaceSnapshotCommand`.
- Add `WorkspacesController`.
- Implement:
  - `GET /api/v1/rooms/{roomId}/workspaces/{userId}`
  - `PUT /api/v1/rooms/{roomId}/workspaces/{userId}/snapshot`
- Add handler and controller tests.

**Depends on**
- Slice 3

**Done when**
- Latest workspace snapshot can be retrieved.
- Workspace snapshot can be updated.
- Handler tests and controller tests pass.

## Slice 5: Mentor Room Queries Slice

**Scope**
- Implement `GetMentorActiveRoomsQuery`.
- Implement `GetRoomParticipantsQuery`.
- Extend `RoomsController` with mentor-facing read endpoints.
- Return DTOs appropriate for dashboard views.
- Add tests for both queries and controller endpoints.

**Depends on**
- Slice 2
- Slice 3

**Done when**
- Mentor can list active rooms.
- Mentor can list room participants.
- Read models are stable enough for dashboard integration.

## Slice 6: Close Room Slice

**Scope**
- Implement `CloseRoomCommand`.
- Validate the acting mentor is allowed to close the room.
- Mark room status as closed.
- Add API endpoint for room closing.
- Add tests for authorization and happy path.

**Depends on**
- Slice 2
- Room repository update support

**Done when**
- Room can be closed through API.
- Closed rooms no longer appear in active-room queries.
- Tests cover owner and non-owner behavior.

## Slice 7: Users Minimal Slice

**Scope**
- Implement `CreateUserCommand`.
- Implement `GetUserQuery`.
- Add `UsersController`.
- Implement:
  - `POST /api/v1/users`
  - `GET /api/v1/users/{id}`
- Add tests for handlers and controller.

**Depends on**
- Slice 1
- User repository

**Done when**
- Minimal user flow works for MVP bootstrapping.
- User endpoints are available for future room and workspace flows.

## Slice 8: SignalR Foundation Slice

**Scope**
- Add initial SignalR hub.
- Implement:
  - room group join
  - cursor update
  - typing status update
  - code delta broadcast
- Keep transient state out of EF-backed application flows.
- Add a minimal connection mapping strategy suitable for single-node MVP.

**Depends on**
- Slice 2
- Slice 3
- Basic room identity flow

**Done when**
- Clients can join room groups.
- Realtime events broadcast to the correct room.
- REST and SignalR responsibilities remain separated.

## Suggested Immediate Next Step

Start with **Slice 1: Application DI Cleanup** because it reduces future rework and makes every later slice cleaner to implement.
