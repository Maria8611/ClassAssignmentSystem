using ClassAssignmentSystem.Domain.Common;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Exceptions;

namespace ClassAssignmentSystem.Domain.Entities
{
    /// <summary>
    /// A student's submission for an assignment.
    /// Lifecycle: Draft (editable) → Submitted (permanently locked).
    /// </summary>
    public class Submission : BaseEntity
    {
        public Guid AssignmentId { get; private set; }
        public Assignment Assignment { get; private set; } = null!;

        public Guid StudentId { get; private set; }
        public User Student { get; private set; } = null!;

        public SubmissionStatus Status { get; private set; }

        // File metadata — replaced on each save while in Draft
        public string FileName { get; private set; } = string.Empty;       // original name e.g. "report.pdf"
        public string StoredFileName { get; private set; } = string.Empty; // GUID-based stored name
        public string ContentType { get; private set; } = string.Empty;    // e.g. "application/pdf"
        public long FileSizeBytes { get; private set; }

        public DateTime? SubmittedAt { get; private set; }  // set when confirmed
        public DateTime LastModifiedAt { get; private set; }

        // Grading
        public GradeStatus GradeStatus { get; private set; }
        public int? MarksObtained { get; private set; }
        public string? TeacherComment { get; private set; }
        public Guid? GradedByTeacherId { get; private set; }
        public DateTime? GradedAt { get; private set; }

        // Allowed MIME types
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

        private Submission() { }

        /// <summary>Creates a new Draft submission with an uploaded file.</summary>
        public static Submission CreateDraft(
            Guid assignmentId, Guid studentId,
            string fileName, string storedFileName, string contentType, long fileSizeBytes)
        {
            ValidateFile(fileName, contentType);

            return new Submission
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignmentId,
                StudentId = studentId,
                Status = SubmissionStatus.Draft,
                FileName = fileName,
                StoredFileName = storedFileName,
                ContentType = contentType,
                FileSizeBytes = fileSizeBytes,
                GradeStatus = GradeStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
        }

        /// <summary>Replace the draft file (allowed any number of times before confirmation).</summary>
        public void UpdateDraftFile(string fileName, string storedFileName, string contentType, long fileSizeBytes)
        {
            EnsureDraft();
            ValidateFile(fileName, contentType);

            FileName = fileName;
            StoredFileName = storedFileName;
            ContentType = contentType;
            FileSizeBytes = fileSizeBytes;
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>Confirms and permanently locks the submission.</summary>
        public void Confirm()
        {
            EnsureDraft();
            Status = SubmissionStatus.Submitted;
            SubmittedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>Teacher adds or updates a grade and comment.</summary>
        public void Grade(int marksObtained, int maxMarks, string? comment, Guid teacherId)
        {
            EnsureSubmitted();
            if (marksObtained < 0) throw new DomainException("Marks cannot be negative.");
            if (marksObtained > maxMarks) throw new DomainException($"Marks cannot exceed max marks ({maxMarks}).");

            MarksObtained = marksObtained;
            TeacherComment = comment?.Trim();
            GradeStatus = GradeStatus.Graded;
            GradedByTeacherId = teacherId;
            GradedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>Teacher adds or updates a comment (without changing the grade).</summary>
        public void AddComment(string comment, Guid teacherId)
        {
            EnsureSubmitted();
            if (string.IsNullOrWhiteSpace(comment)) throw new DomainException("Comment cannot be empty.");
            TeacherComment = comment.Trim();
            GradedByTeacherId = teacherId;
            LastModifiedAt = DateTime.UtcNow;
        }

        public bool IsLocked() => Status == SubmissionStatus.Submitted;

        private void EnsureDraft()
        {
            if (Status != SubmissionStatus.Draft)
                throw new DomainException("This submission has been confirmed and can no longer be modified.");
        }

        private void EnsureSubmitted()
        {
            if (Status != SubmissionStatus.Submitted)
                throw new DomainException("You can only grade or comment on a confirmed submission.");
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

        public static bool IsAllowedExtension(string extension) => AllowedExtensions.Contains(extension);
        public static IReadOnlyCollection<string> GetAllowedExtensions() => AllowedExtensions;
    }
}
