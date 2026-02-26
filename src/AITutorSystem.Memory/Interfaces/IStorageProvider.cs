using System.Linq.Expressions;

namespace AITutorSystem.Memory.Interfaces;

/// <summary>
/// Provides abstraction for storage operations supporting both local and cloud storage.
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// Retrieves an object from storage by key.
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;
    
    /// <summary>
    /// Saves an object to storage with the specified key.
    /// </summary>
    Task<bool> SaveAsync<T>(string key, T data) where T : class;
    
    /// <summary>
    /// Updates an existing object in storage.
    /// </summary>
    Task<bool> UpdateAsync<T>(string key, T data) where T : class;
    
    /// <summary>
    /// Deletes an object from storage by key.
    /// </summary>
    Task<bool> DeleteAsync(string key);
    
    /// <summary>
    /// Queries storage for objects matching a predicate.
    /// </summary>
    Task<IEnumerable<T>> QueryAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
}
