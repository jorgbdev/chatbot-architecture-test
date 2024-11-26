using emma.ml.chatbot.api.Entities;
using emma.ml.chatbot.api.Entities.Chat;
using emma.ml.chatbot.api.Entities.KnowledgeBase;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options) { }

    public DbSet<Theme> Themes { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Entry> Entries { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<SeederHistory> SeederHistories { get; set; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SeederHistory>(entity =>
        {
            entity.ToTable("SeederHistories");

            entity.HasKey(sh => sh.Id);

            entity.Property(sh => sh.SeederName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(sh => sh.ExecutedAt)
                  .IsRequired();

            entity.HasIndex(sh => sh.SeederName)
                  .IsUnique();
        });

        // Participant Configuration
        modelBuilder.Entity<Participant>(entity =>
        {
            entity.ToTable("Participants");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Role)
                  .IsRequired()
                  .HasMaxLength(50);
        });

        // Conversation Configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("Conversations");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Status)
                  .IsRequired()
                  .HasMaxLength(20);

            entity.Property(c => c.CurrentRepresentativeId)
                  .IsRequired(false);

            // Relationship: Conversation -> CurrentRepresentative (Participant)
            entity.HasOne(c => c.CurrentRepresentative)
                  .WithMany()
                  .HasForeignKey(c => c.CurrentRepresentativeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ConversationParticipant>(entity =>
        {
            entity.ToTable("ConversationParticipants");

            // Composite primary key
            entity.HasKey(cp => new { cp.ConversationId, cp.ParticipantId });

            entity.Property(cp => cp.Status)
                  .HasMaxLength(50);

            entity.Property(cp => cp.JoinedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();

            // Configure relationships
            entity.HasOne(cp => cp.Conversation)
                  .WithMany(c => c.ConversationParticipants)
                  .HasForeignKey(cp => cp.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cp => cp.Participant)
                  .WithMany(p => p.ConversationParticipants)
                  .HasForeignKey(cp => cp.ParticipantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Message Configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");

            entity.HasKey(m => m.Id);

            entity.Property(m => m.MessageText)
                  .IsRequired();

            entity.Property(m => m.Timestamp)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();

            // Relationship: Message -> Conversation
            entity.HasOne(m => m.Conversation)
                  .WithMany(c => c.Messages)
                  .HasForeignKey(m => m.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Message -> Participant
            entity.HasOne(m => m.Participant)
                  .WithMany(p => p.Messages) // Specify the inverse navigation property
                  .HasForeignKey(m => m.ParticipantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.ToTable("Themes");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasIndex(t => t.Name)
                  .IsUnique();

            // Relationship: Theme -> Topics
            entity.HasMany(t => t.Topics)
                  .WithOne(tp => tp.Theme)
                  .HasForeignKey(tp => tp.ThemeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("Topics");

            entity.HasKey(tp => tp.Id);

            entity.Property(tp => tp.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasIndex(tp => new { tp.ThemeId, tp.Name })
                  .IsUnique();

            // Relationship: Topic -> Entries
            entity.HasMany(tp => tp.Entries)
                  .WithOne(e => e.Topic)
                  .HasForeignKey(e => e.TopicId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Entry Configuration
        modelBuilder.Entity<Entry>(entity =>
        {
            entity.ToTable("Entries");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Key)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Value)
                  .IsRequired();

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAddOrUpdate();

            entity.HasIndex(e => new { e.TopicId, e.Key })
                  .IsUnique();
        });

    }
}
