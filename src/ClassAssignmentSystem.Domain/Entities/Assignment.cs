using ClassAssignmentSystem.Domain.Common;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Domain.Entities
{
    /// <summary>
    /// An assignment posted by a teacher for a specific course.
    /// Only the teacher assigned to that course may create or update it.
    /// </summary>
    public class Assignment : BaseEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public DateTime Deadline { get; private set; }
        public int MaxMarks { get; private set; }

        // FK to Course
        public Guid CourseId { get; private set; }
        public Course Course { get; private set; } = null!;

        // The teacher who created this assignment
        public Guid CreatedByTeacherId { get; private set; } 
        
        // File metadata — replaced on each save while in Draft
        public string? FileName { get; private set; } = string.Empty;       // original name e.g. "report.pdf"
        public string? StoredFileName { get; private set; } = string.Empty; // GUID-based stored name
        public string? ContentType { get; private set; } = string.Empty;    // e.g. "application/pdf"
        public long? FileSizeBytes { get; private set; }
        public DateTime LastModifiedAt { get; private set; }

        public ICollection<Submission> Submissions { get; private set; } = new List<Submission>();
        
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp"
    };

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx",
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

        private Assignment() { }

        public static Assignment Create(
            string title, string description, DateTime deadline, int maxMarks,
            Guid courseId, Guid teacherId)
        {
            if (string.IsNullOrEmpty(title.Trim())) throw new ArgumentException("Title is required.");
            if (string.IsNullOrEmpty(description.Trim())) throw new ArgumentException("Description is required.");
            if (deadline <= DateTime.UtcNow) throw new DomainException("Deadline must be in the future.");
            if (maxMarks <= 0) throw new DomainException("Max marks must be greater than zero.");
           
            return new Assignment
            {
                Id = Guid.NewGuid(),
                Title = title.Trim(),
                Description = description.Trim(),
                Deadline = deadline,
                MaxMarks = maxMarks,
                CourseId = courseId,
                CreatedByTeacherId = teacherId,
                CreatedAt = DateTime.UtcNow,
            };
        }

        public void Update(string title, string description, DateTime deadline, int maxMarks, Guid requestingTeacherId)
        {
            EnsureOwnership(requestingTeacherId);
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.");
            if (deadline <= DateTime.UtcNow) throw new DomainException("Deadline must be in the future.");
            if (maxMarks <= 0) throw new DomainException("Max marks must be greater than zero.");

            Title = title.Trim();
            Description = description.Trim();
            Deadline = deadline;
            MaxMarks = maxMarks;
        }

        public bool IsDeadlinePassed() => DateTime.UtcNow > Deadline;

        private void EnsureOwnership(Guid teacherId)
        {
            if (CreatedByTeacherId != teacherId)
                throw new DomainException("You are not the teacher assigned to this course and cannot modify this assignment.");
        }

        public void SetMaterialBlobName(string blobName)
        {
            StoredFileName = blobName;
        }
        private static void ValidateFile(string fileName, string contentType)
        {
            var ext = Path.GetExtension(fileName);
            if (!AllowedExtensions.Contains(ext))
                throw new DomainException(
                    $"File type '{ext}' is not allowed. Accepted: PDF, Word, PowerPoint, Excel, and common image formats.");

            if (!AllowedContentTypes.Contains(contentType))
                throw new DomainException(
                    $"Content type '{contentType}' is not permitted.");
        }

        public void UploadAssignmentMaterial(string fileName, string blobName, string contentType, long fileSizeBytes)
        {
            ValidateFile(fileName, contentType);
            FileName = fileName;
            StoredFileName = blobName;
            ContentType = contentType;
            FileSizeBytes = fileSizeBytes;
            LastModifiedAt = DateTime.UtcNow;
        }
    }
}
