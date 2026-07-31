namespace ClassAssignmentSystem.Application.Configurations;

public class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    /// <summary>Full connection string, or leave empty and set AccountName to use Managed Identity.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Used with DefaultAzureCredential when ConnectionString is not supplied (recommended for prod).</summary>
    public string? AccountName { get; set; }

    public string SubmissionsContainer { get; set; } = "assignment-submissions";
    public string AssignmentMaterialsContainer { get; set; } = "assignment-materials";

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
    public string[] AllowedExtensions { get; set; } =
        { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".zip", ".png", ".jpg", ".jpeg" };

    public int SasTokenExpiryMinutes { get; set; } = 15;
}
