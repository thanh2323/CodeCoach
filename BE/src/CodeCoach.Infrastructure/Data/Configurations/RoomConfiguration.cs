using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Infrastructure.Data.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.HasKey(room => room.Id);

        builder.Property(room => room.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(room => room.JoinCode)
            .IsRequired()
            .HasMaxLength(16);

        builder.HasIndex(room => room.JoinCode)
            .IsUnique();

        builder.Property(room => room.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(room => room.CurrentMode)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(room => room.CreatedAt)
            .IsRequired();

        builder.Property(room => room.ClosedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(room => room.MentorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(room => room.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
