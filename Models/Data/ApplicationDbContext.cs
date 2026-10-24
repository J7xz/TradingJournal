using System.Data.Entity;
using J7Journal.Models;

namespace J7Journal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        public DbSet<TradeEntry> TradeEntries { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TradeTag> TradeTags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configure decimal precision
            modelBuilder.Entity<TradeEntry>()
                .Property(e => e.ProfitLoss)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TradeEntry>()
                .Property(e => e.EntryPrice)
                .HasPrecision(18, 4);

            modelBuilder.Entity<TradeEntry>()
                .Property(e => e.ExitPrice)
                .HasPrecision(18, 4);
            modelBuilder.Entity<TradeEntry>()
    .HasIndex(t => t.MT5Ticket)
    .IsUnique()
    .HasName("IX_TradeEntries_MT5Ticket");
            base.OnModelCreating(modelBuilder);
        }
    }
}