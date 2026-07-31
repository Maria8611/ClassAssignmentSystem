using ClassAssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassAssignmentSystem.Infrastructure.Persistence.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.Property(s => s.FileName).HasMaxLength(255).IsRequired();
        builder.Property(s => s.StoredFileName).HasMaxLength(1024).IsRequired();
        builder.Property(s => s.ContentType).HasMaxLength(100).IsRequired();

        builder.HasOne(s => s.Assignment)
            .WithMany() // add ICollection<Submission> on Assignment if you want a nav back
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // One active submission row per student per assignment — resubmission
        // replaces the blob + row rather than inserting a new one.
        builder.HasIndex(s => new { s.AssignmentId, s.StudentId }).IsUnique();
    }
}
