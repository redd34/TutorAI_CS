using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AITutorSystem.Memory.Providers;
using Xunit;

namespace AITutorSystem.Memory.Tests;

public class LocalStorageProviderTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LocalStorageProvider _provider;
    private static readonly bool _cleanupAfterTests = true; // Set to false to keep test data for inspection

    public LocalStorageProviderTests()
    {
        // Use a static test directory in the project for inspection
        _testBasePath = Path.Combine(Directory.GetCurrentDirectory(), "localtestdata");
        _provider = new LocalStorageProvider(_testBasePath);
        
        // Clean up any existing test data before starting if cleanup is enabled
        if (_cleanupAfterTests && Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
    }

    public void Dispose()
    {
        // Clean up test directory after each test if cleanup is enabled
        // Set _cleanupAfterTests to false to inspect the test data
        if (_cleanupAfterTests && Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateFile()
    {
        // Arrange
        var testData = new TestModel { Id = "test-1", Name = "Test Object", Value = 42 };

        // Act
        var result = await _provider.SaveAsync("test-key", testData);

        // Assert
        Assert.True(result);
        var expectedPath = Path.Combine(_testBasePath, "TestModel", "test-key.json");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public async Task GetAsync_ShouldReturnSavedObject()
    {
        // Arrange
        var testData = new TestModel { Id = "test-1", Name = "Test Object", Value = 42 };
        await _provider.SaveAsync("test-key", testData);

        // Act
        var retrieved = await _provider.GetAsync<TestModel>("test-key");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(testData.Id, retrieved.Id);
        Assert.Equal(testData.Name, retrieved.Name);
        Assert.Equal(testData.Value, retrieved.Value);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNullForNonExistentKey()
    {
        // Act
        var result = await _provider.GetAsync<TestModel>("non-existent-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingObject()
    {
        // Arrange
        var originalData = new TestModel { Id = "test-1", Name = "Original", Value = 10 };
        await _provider.SaveAsync("test-key", originalData);

        var updatedData = new TestModel { Id = "test-1", Name = "Updated", Value = 20 };

        // Act
        var result = await _provider.UpdateAsync("test-key", updatedData);
        var retrieved = await _provider.GetAsync<TestModel>("test-key");

        // Assert
        Assert.True(result);
        Assert.NotNull(retrieved);
        Assert.Equal(updatedData.Name, retrieved.Name);
        Assert.Equal(updatedData.Value, retrieved.Value);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForNonExistentKey()
    {
        // Arrange
        var testData = new TestModel { Id = "test-1", Name = "Test", Value = 42 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.UpdateAsync("non-existent-key", testData)
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveFile()
    {
        // Arrange
        var testData = new TestModel { Id = "test-1", Name = "Test Object", Value = 42 };
        await _provider.SaveAsync("test-key", testData);

        // Act
        var result = await _provider.DeleteAsync("test-key");

        // Assert
        Assert.True(result);
        var retrieved = await _provider.GetAsync<TestModel>("test-key");
        Assert.Null(retrieved);
    }

    // [Fact]
    // public async Task DeleteAsync_ShouldReturnFalseForNonExistentKey()
    // {
    //     // Act
    //     var result = await _provider.DeleteAsync("non-existent-key");

    //     // Assert
    //     Assert.False(result);
    // }

    [Fact]
    public async Task QueryAsync_ShouldReturnMatchingObjects()
    {
        // Arrange
        var data1 = new TestModel { Id = "1", Name = "Test 1", Value = 10 };
        var data2 = new TestModel { Id = "2", Name = "Test 2", Value = 20 };
        var data3 = new TestModel { Id = "3", Name = "Other", Value = 30 };

        await _provider.SaveAsync("key1", data1);
        await _provider.SaveAsync("key2", data2);
        await _provider.SaveAsync("key3", data3);

        // Act
        var results = await _provider.QueryAsync<TestModel>(x => x.Name.StartsWith("Test"));

        // Assert
        Assert.Equal(2, results.Count());
        Assert.Contains(results, x => x.Id == "1");
        Assert.Contains(results, x => x.Id == "2");
        Assert.DoesNotContain(results, x => x.Id == "3");
    }

    [Fact]
    public async Task QueryAsync_ShouldReturnEmptyForNoMatches()
    {
        // Arrange
        var data1 = new TestModel { Id = "1", Name = "Test 1", Value = 10 };
        await _provider.SaveAsync("key1", data1);

        // Act
        var results = await _provider.QueryAsync<TestModel>(x => x.Name == "NonExistent");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task QueryAsync_ShouldReturnEmptyForEmptyDirectory()
    {
        // Act
        var results = await _provider.QueryAsync<TestModel>(x => x.Name == "Anything");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Constructor_ShouldCreateBaseDirectory()
    {
        // Arrange
        var newPath = Path.Combine(Path.GetTempPath(), $"aitutor-new-{Guid.NewGuid()}");

        // Act
        var provider = new LocalStorageProvider(newPath);

        // Assert
        Assert.True(Directory.Exists(newPath));

        // Cleanup
        Directory.Delete(newPath, true);
    }

    [Fact]
    public async Task SaveAsync_ShouldHandleInvalidPath()
    {
        // Arrange
        // Create a valid provider first
        var provider = new LocalStorageProvider(_testBasePath);
        var testData = new TestModel { Id = "test", Name = "Test", Value = 1 };
        
        // Create a file path that will cause an error when trying to create directory
        // We'll use a path that's too long for the file system
        var veryLongPath = new string('a', 1000);
        var invalidKey = $"{veryLongPath}/more/path";

        // Act & Assert - This should throw InvalidOperationException
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.SaveAsync(invalidKey, testData)
        );
    }

    [Fact]
    public async Task GetAsync_ShouldHandleInvalidJson()
    {
        // Arrange
        var testData = new TestModel { Id = "test-1", Name = "Test Object", Value = 42 };
        await _provider.SaveAsync("test-key", testData);
        
        // Corrupt the JSON file
        var filePath = Path.Combine(_testBasePath, "TestModel", "test-key.json");
        await File.WriteAllTextAsync(filePath, "{ invalid json }");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.GetAsync<TestModel>("test-key")
        );
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateNestedDirectories()
    {
        // Arrange
        var nestedPath = Path.Combine(_testBasePath, "nested", "deep", "path");
        var provider = new LocalStorageProvider(nestedPath);
        var testData = new TestModel { Id = "test", Name = "Nested Test", Value = 100 };

        // Act
        var result = await provider.SaveAsync("nested-key", testData);

        // Assert
        Assert.True(result);
        var expectedPath = Path.Combine(nestedPath, "TestModel", "nested-key.json");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public async Task QueryAsync_ShouldHandleMultipleTypes()
    {
        // Arrange
        var testModel = new TestModel { Id = "1", Name = "Test Model", Value = 10 };
        var anotherModel = new AnotherTestModel 
        { 
            Id = "2", 
            Description = "Another Model", 
            CreatedAt = DateTime.UtcNow, 
            IsActive = true 
        };

        await _provider.SaveAsync("test-key", testModel);
        await _provider.SaveAsync("another-key", anotherModel);

        // Act
        var testResults = await _provider.QueryAsync<TestModel>(x => x.Value > 5);
        var anotherResults = await _provider.QueryAsync<AnotherTestModel>(x => x.IsActive);

        // Assert
        Assert.Single(testResults);
        Assert.Single(anotherResults);
        Assert.Equal("1", testResults.First().Id);
        Assert.Equal("2", anotherResults.First().Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteFileAndReturnTrue()
    {
        // Arrange
        var testModel = new TestModel { Id = "1", Name = "Test", Value = 10 };
        await _provider.SaveAsync("test-key", testModel);

        // Act
        var result = await _provider.DeleteAsync("test-key");

        // Assert
        Assert.True(result);
        
        // Check that file is deleted
        var testResult = await _provider.GetAsync<TestModel>("test-key");
        Assert.Null(testResult);
    }

    [Fact]
    public async Task DeleteAsync_ShouldOnlyDeleteFirstMatchingFile()
    {
        // Arrange
        // This test demonstrates the current behavior where DeleteAsync
        // only deletes the first file it finds with the given key
        var testModel = new TestModel { Id = "1", Name = "Test", Value = 10 };
        var anotherModel = new AnotherTestModel { Id = "2", Description = "Another", CreatedAt = DateTime.UtcNow, IsActive = true };

        await _provider.SaveAsync("same-key", testModel);
        await _provider.SaveAsync("same-key", anotherModel);

        // Act
        var result = await _provider.DeleteAsync("same-key");

        // Assert
        Assert.True(result);
        
        // With the current implementation, only one of the files will be deleted
        // We can't predict which one, so we just verify that at least one is deleted
        var testResult = await _provider.GetAsync<TestModel>("same-key");
        var anotherResult = await _provider.GetAsync<AnotherTestModel>("same-key");
        
        // At least one should be null
        Assert.True(testResult == null || anotherResult == null);
    }
}