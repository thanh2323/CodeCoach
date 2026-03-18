using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Infrastructure.Data.Configurations;

public class RoomParticipantConfiguration : IEntityTypeConfiguration<RoomParticipant>
{
    public void Configure(EntityTypeBuilder<RoomParticipant> builder)
    {
        builder.ToTable("RoomParticipants");

        builder.HasKey(participant => participant.Id);

        builder.Property(participant => participant.Role)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(participant => participant.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(participant => participant.JoinedAt)
            .IsRequired();

        builder.Property(participant => participant.LastActiveAt);

        builder.HasIndex(participant => new { participant.RoomId, participant.UserId })
            .IsUnique();

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(participant => participant.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(participant => participant.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
