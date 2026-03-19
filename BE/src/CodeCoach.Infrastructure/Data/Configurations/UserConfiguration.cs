using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .IsRequired();

        builder.Property(user => user.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(user => user.CreatedAt)
            .IsRequired();
    }
}
