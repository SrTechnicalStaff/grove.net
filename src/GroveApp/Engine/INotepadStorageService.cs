using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;

namespace GroveApp.Engine
{
    /// <summary>
    /// Deep storage service interface hiding disk I/O, draft buffers, and spatial grid synchronization.
    /// Exposes simple LoadAsync/SaveAsync contract per ADR-064.
    /// </summary>
    public interface INotepadStorageService
    {
        Task<string> LoadAsync(string noteId);
        Task SaveAsync(string noteId, string text);
    }

    public class InMemoryNotepadStorageService : INotepadStorageService
    {
        private readonly Dictionary<string, string> _storage = new();

        public Task<string> LoadAsync(string noteId)
        {
            if (_storage.TryGetValue(noteId, out var text))
            {
                return Task.FromResult(text);
            }
            return Task.FromResult(string.Empty);
        }

        public Task SaveAsync(string noteId, string text)
        {
            _storage[noteId] = text;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Durable default adapter for local editor drafts. The editor depends only on
    /// INotepadStorageService; path policy and file I/O stay behind this seam.
    /// </summary>
    public sealed class FileNotepadStorageService : INotepadStorageService
    {
        private readonly string _rootDirectory;

        public FileNotepadStorageService(string? rootDirectory = null)
        {
            _rootDirectory = rootDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Grove",
                "notes");
        }

        public async Task<string> LoadAsync(string noteId)
        {
            string path = GetPath(noteId);
            return File.Exists(path)
                ? await File.ReadAllTextAsync(path).ConfigureAwait(false)
                : string.Empty;
        }

        public async Task SaveAsync(string noteId, string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            Directory.CreateDirectory(_rootDirectory);
            await File.WriteAllTextAsync(GetPath(noteId), text).ConfigureAwait(false);
        }

        private string GetPath(string noteId)
        {
            if (string.IsNullOrWhiteSpace(noteId) ||
                noteId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("A valid note identifier is required.", nameof(noteId));
            }

            return Path.Combine(_rootDirectory, $"{noteId}.md");
        }
    }
}
