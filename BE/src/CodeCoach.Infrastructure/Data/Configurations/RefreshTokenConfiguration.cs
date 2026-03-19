using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(refreshToken => refreshToken.Id);

        builder.Property(refreshToken => refreshToken.TokenHash)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(refreshToken => refreshToken.ExpiresAtUtc)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.CreatedAtUtc)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.CreatedByIp)
            .HasMaxLength(64);

        builder.HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique();

        builder.HasIndex(refreshToken => refreshToken.UserId);

        builder.HasIndex(refreshToken => refreshToken.ReplacedByTokenId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
