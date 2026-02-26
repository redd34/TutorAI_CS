using System.Linq.Expressions;
using System.Text.Json;
using AITutorSystem.Memory.Interfaces;

namespace AITutorSystem.Memory.Providers;

/// <summary>
/// Local file system storage provider for development.
/// Uses JSON serialization to store objects as files.
/// </summary>
public class LocalStorageProvider : IStorageProvider
{
    private readonly string _basePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public LocalStorageProvider(string basePath)
    {
        _basePath = basePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        
        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var filePath = GetFilePath<T>(key);
            
            if (!File.Exists(filePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve object with key '{key}'", ex);
        }
    }

    public async Task<bool> SaveAsync<T>(string key, T data) where T : class
    {
        try
        {
            var filePath = GetFilePath<T>(key);
            var directory = Path.GetDirectoryName(filePath);
            
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save object with key '{key}'", ex);
        }
    }

    public async Task<bool> UpdateAsync<T>(string key, T data) where T : class
    {
        try
        {
            var filePath = GetFilePath<T>(key);
            
            if (!File.Exists(filePath))
            {
                throw new InvalidOperationException($"Object with key '{key}' does not exist");
            }

            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update object with key '{key}'", ex);
        }
    }

    public Task<bool> DeleteAsync(string key)
    {
        try
        {
            // Since we don't know the type, we need to search for the file
            var directories = Directory.GetDirectories(_basePath);
            
            foreach (var directory in directories)
            {
                var filePath = Path.Combine(directory, $"{key}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }
            }
            
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete object with key '{key}'", ex);
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        try
        {
            var typeName = typeof(T).Name;
            var typeDirectory = Path.Combine(_basePath, typeName);
            
            if (!Directory.Exists(typeDirectory))
            {
                return Enumerable.Empty<T>();
            }

            var files = Directory.GetFiles(typeDirectory, "*.json");
            var results = new List<T>();
            var compiledPredicate = predicate.Compile();

            foreach (var file in files)
            {
                var json = await File.ReadAllTextAsync(file);
                var obj = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                
                if (obj != null && compiledPredicate(obj))
                {
                    results.Add(obj);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to query objects of type '{typeof(T).Name}'", ex);
        }
    }

    private string GetFilePath<T>(string key)
    {
        var typeName = typeof(T).Name;
        var directory = Path.Combine(_basePath, typeName);
        return Path.Combine(directory, $"{key}.json");
    }
}
