using Microsoft.EntityFrameworkCore;
using MinimalApi.Data.Entities;

namespace MinimalApi.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("AI");

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("Conversations", "AI");
            entity.HasKey(conversation => conversation.Id);
            entity.Property(conversation => conversation.Title).HasMaxLength(200).IsRequired();
            entity.Property(conversation => conversation.Model).HasMaxLength(100).IsRequired();
            entity.Property(conversation => conversation.CreatedAt).IsRequired();
            entity.Property(conversation => conversation.UpdatedAt).IsRequired();
            entity.HasIndex(conversation => conversation.UpdatedAt);

            entity
                .HasMany(conversation => conversation.Messages)
                .WithOne(message => message.Conversation)
                .HasForeignKey(message => message.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages", "AI");
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Role).HasMaxLength(20).IsRequired();
            entity.Property(message => message.Content).IsRequired();
            entity.Property(message => message.Model).HasMaxLength(100);
            entity.HasIndex(message => new { message.ConversationId, message.CreatedAt });
        });
    }
}
