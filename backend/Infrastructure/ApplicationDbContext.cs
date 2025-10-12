using Dotnet_test.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dotnet_test.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        //DbSets for new application entities
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Deleting a user deletes hosted sessions
            modelBuilder
                .Entity<Session>()
                .HasOne(s => s.HostUser)
                .WithMany(u => u.HostedSessions)
                .HasForeignKey(s => s.HostUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent cascade from Participant.UserId to avoid multiple cascade paths
            modelBuilder
                .Entity<Participant>()
                .HasOne(p => p.User)
                .WithMany(u => u.SessionsJoined)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict); // <-- NO cascade

            // Deleting a session deletes participants in that session
            modelBuilder
                .Entity<Participant>()
                .HasOne(p => p.Session)
                .WithMany(s => s.Participants)
                .HasForeignKey(p => p.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // PlaylistSongs → User
            modelBuilder
                .Entity<PlaylistSong>()
                .HasOne(ps => ps.AddedByUser)
                .WithMany(u => u.AddedPlaylistSongs)
                .HasForeignKey(ps => ps.AddedByUserId)
                .OnDelete(DeleteBehavior.Restrict); // NO cascade

            // PlaylistSongs → Playlist
            modelBuilder
                .Entity<PlaylistSong>()
                .HasOne(ps => ps.Playlist)
                .WithMany(p => p.Songs)
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade); // keep cascade here
        }
    }
}
