using EurekaDb.Migrations;
using Microsoft.EntityFrameworkCore;

namespace EurekaDb.Context;

public partial class EurekaContext : DbContext
{
    public EurekaContext(DbContextOptions<EurekaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<PlayerSession> PlayerSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("players");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LastOnline).HasColumnName("last_online");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.TotalPlayTime).HasColumnName("total_play_time");
        });

        modelBuilder.Entity<PlayerSession>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.Date });

            entity.ToTable("player_sessions");

            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.TimePlayedInSession).HasColumnName("time_played_in_session");

            entity.HasOne(d => d.Player).WithMany(p => p.PlayerSessions)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}