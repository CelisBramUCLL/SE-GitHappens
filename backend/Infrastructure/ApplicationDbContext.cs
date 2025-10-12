using Dotnet_test.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dotnet_test.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        //DbSets for application entities
        public DbSet<User> Users { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // Deleting a user deletes hosted parties
            modelBuilder
                .Entity<Party>()
                .HasOne(s => s.HostUser)
                .WithMany(u => u.HostedParties)
                .HasForeignKey(s => s.HostUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent cascade from Participant.UserId to avoid multiple cascade paths
            modelBuilder
                .Entity<Participant>()
                .HasOne(p => p.User)
                .WithMany(u => u.PartiesJoined)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Deleting a party deletes participants in that party
            modelBuilder
                .Entity<Participant>()
                .HasOne(p => p.Party)
                .WithMany(s => s.Participants)
                .HasForeignKey(p => p.PartyId)
                .OnDelete(DeleteBehavior.Cascade);

            // PlaylistSongs → User
            modelBuilder
                .Entity<PlaylistSong>()
                .HasOne(ps => ps.AddedByUser)
                .WithMany(u => u.AddedPlaylistSongs)
                .HasForeignKey(ps => ps.AddedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PlaylistSongs → Playlist
            modelBuilder
                .Entity<PlaylistSong>()
                .HasOne(ps => ps.Playlist)
                .WithMany(p => p.Songs)
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
