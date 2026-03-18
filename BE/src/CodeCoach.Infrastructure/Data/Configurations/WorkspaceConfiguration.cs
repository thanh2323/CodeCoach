using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Infrastructure.Data.Configurations;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("Workspaces");

        builder.HasKey(workspace => workspace.Id);

        builder.Property(workspace => workspace.Language)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(workspace => workspace.SourceCode)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(workspace => workspace.UpdatedAt)
            .IsRequired();

        builder.HasIndex(workspace => new { workspace.RoomId, workspace.UserId })
            .IsUnique();

        builder.HasIndex(workspace => workspace.RoomId)
            .HasFilter("\"UserId\" IS NULL")
            .IsUnique();

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(workspace => workspace.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(workspace => workspace.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
