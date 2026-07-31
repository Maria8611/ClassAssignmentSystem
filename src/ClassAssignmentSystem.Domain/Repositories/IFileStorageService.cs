using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Domain.Repositories
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves an uploaded file stream and returns the stored file name (GUID-based).
        /// </summary>
        Task<string> SaveAsync(Stream fileStream, string originalFileName, CancellationToken ct = default);

        /// <summary>
        /// Returns the file stream and content type for download.
        /// </summary>
        Task<(Stream Stream, string ContentType, string FileName)> GetAsync(string storedFileName, string originalFileName, CancellationToken ct = default);

        /// <summary>
        /// Deletes a previously stored file (used when a draft file is replaced).
        /// </summary>
        Task DeleteAsync(string storedFileName, CancellationToken ct = default);
    }
}
