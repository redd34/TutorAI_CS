using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using AITutorSystem.Memory.Interfaces;
using AITutorSystem.Memory.Providers;

namespace AITutorSystem.Memory;

/// <summary>
/// Factory for creating storage providers based on environment configuration.
/// </summary>
public static class StorageFactory
{
    /// <summary>
    /// Creates a storage provider based on the environment configuration.
    /// </summary>
    /// <param name="configuration">Configuration containing environment and storage settings</param>
    /// <returns>An IStorageProvider instance configured for the current environment</returns>
    /// <exception cref="InvalidOperationException">Thrown when environment is unknown or configuration is invalid</exception>
    public static IStorageProvider Create(IConfiguration configuration)
    {
        var environment = configuration["Environment"] 
            ?? throw new InvalidOperationException("Environment configuration is missing");

        return environment.ToLowerInvariant() switch
        {
            "local" => CreateLocalProvider(configuration),
            "lambda" => CreateCloudProvider(configuration),
            _ => throw new InvalidOperationException($"Unknown environment: {environment}")
        };
    }

    private static IStorageProvider CreateLocalProvider(IConfiguration configuration)
    {
        var storagePath = configuration["LocalStoragePath"] ?? "./data";
        return new LocalStorageProvider(storagePath);
    }

    private static IStorageProvider CreateCloudProvider(IConfiguration configuration)
    {
        var bucketName = configuration["S3Bucket"] 
            ?? throw new InvalidOperationException("S3Bucket configuration is missing");
        
        var tableName = configuration["DynamoTable"] 
            ?? throw new InvalidOperationException("DynamoTable configuration is missing");

        var s3Client = new AmazonS3Client();
        var dynamoClient = new AmazonDynamoDBClient();

        return new CloudStorageProvider(s3Client, dynamoClient, bucketName, tableName);
    }
}
