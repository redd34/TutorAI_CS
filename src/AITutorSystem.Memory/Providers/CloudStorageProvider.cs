using System.Linq.Expressions;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using AITutorSystem.Memory.Interfaces;

namespace AITutorSystem.Memory.Providers;

/// <summary>
/// Cloud storage provider using AWS S3 and DynamoDB.
/// Routes small objects to DynamoDB and large objects to S3.
/// Includes retry logic with exponential backoff.
/// </summary>
public class CloudStorageProvider : IStorageProvider
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonDynamoDB _dynamoClient;
    private readonly string _bucketName;
    private readonly string _tableName;
    private readonly JsonSerializerOptions _jsonOptions;
    private const int MaxRetries = 3;
    private const int SizeThresholdBytes = 400_000; // 400KB threshold for DynamoDB

    public CloudStorageProvider(
        IAmazonS3 s3Client,
        IAmazonDynamoDB dynamoClient,
        string bucketName,
        string tableName)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _dynamoClient = dynamoClient ?? throw new ArgumentNullException(nameof(dynamoClient));
        _bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
        _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var fullKey = GetFullKey<T>(key);
                
                // Try DynamoDB first
                var getRequest = new GetItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "Id", new AttributeValue { S = fullKey } }
                    }
                };

                var response = await _dynamoClient.GetItemAsync(getRequest);
                
                if (response.Item.Count > 0 && response.Item.ContainsKey("Data"))
                {
                    var json = response.Item["Data"].S;
                    return JsonSerializer.Deserialize<T>(json, _jsonOptions);
                }
                
                // Try S3 if not in DynamoDB
                try
                {
                    var s3Request = new GetObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = fullKey
                    };

                    using var s3Response = await _s3Client.GetObjectAsync(s3Request);
                    using var reader = new StreamReader(s3Response.ResponseStream);
                    var json = await reader.ReadToEndAsync();
                    return JsonSerializer.Deserialize<T>(json, _jsonOptions);
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve object with key '{key}'", ex);
            }
        });
    }

    public async Task<bool> SaveAsync<T>(string key, T data) where T : class
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var fullKey = GetFullKey<T>(key);
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var sizeBytes = System.Text.Encoding.UTF8.GetByteCount(json);

                if (sizeBytes < SizeThresholdBytes)
                {
                    // Store in DynamoDB
                    var putRequest = new PutItemRequest
                    {
                        TableName = _tableName,
                        Item = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = fullKey } },
                            { "Data", new AttributeValue { S = json } },
                            { "Type", new AttributeValue { S = typeof(T).Name } },
                            { "UpdatedAt", new AttributeValue { S = DateTime.UtcNow.ToString("o") } }
                        }
                    };

                    await _dynamoClient.PutItemAsync(putRequest);
                }
                else
                {
                    // Store in S3
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = fullKey,
                        ContentBody = json,
                        ContentType = "application/json"
                    };

                    await _s3Client.PutObjectAsync(putRequest);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save object with key '{key}'", ex);
            }
        });
    }

    public async Task<bool> UpdateAsync<T>(string key, T data) where T : class
    {
        // For cloud storage, update is the same as save (upsert behavior)
        return await SaveAsync(key, data);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // Try to delete from DynamoDB
                var deleteRequest = new DeleteItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "Id", new AttributeValue { S = key } }
                    }
                };

                await _dynamoClient.DeleteItemAsync(deleteRequest);

                // Try to delete from S3
                try
                {
                    var s3DeleteRequest = new DeleteObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = key
                    };

                    await _s3Client.DeleteObjectAsync(s3DeleteRequest);
                }
                catch (AmazonS3Exception)
                {
                    // Object might not exist in S3, which is fine
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete object with key '{key}'", ex);
            }
        });
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var typeName = typeof(T).Name;
                var results = new List<T>();

                // Query DynamoDB
                var scanRequest = new ScanRequest
                {
                    TableName = _tableName,
                    FilterExpression = "#type = :typename",
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        { "#type", "Type" }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":typename", new AttributeValue { S = typeName } }
                    }
                };

                var response = await _dynamoClient.ScanAsync(scanRequest);
                var compiledPredicate = predicate.Compile();

                foreach (var item in response.Items)
                {
                    if (item.ContainsKey("Data"))
                    {
                        var json = item["Data"].S;
                        var obj = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                        
                        if (obj != null && compiledPredicate(obj))
                        {
                            results.Add(obj);
                        }
                    }
                }

                // Query S3 (list objects with type prefix)
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = $"{typeName}/"
                };

                var s3Response = await _s3Client.ListObjectsV2Async(listRequest);

                foreach (var s3Object in s3Response.S3Objects)
                {
                    var getRequest = new GetObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = s3Object.Key
                    };

                    using var objectResponse = await _s3Client.GetObjectAsync(getRequest);
                    using var reader = new StreamReader(objectResponse.ResponseStream);
                    var json = await reader.ReadToEndAsync();
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
        });
    }

    private string GetFullKey<T>(string key)
    {
        var typeName = typeof(T).Name;
        return $"{typeName}/{key}";
    }

    private async Task<TResult> ExecuteWithRetryAsync<TResult>(Func<Task<TResult>> operation)
    {
        var retryCount = 0;
        var delay = TimeSpan.FromMilliseconds(100);

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (IsTransientError(ex) && retryCount < MaxRetries)
            {
                retryCount++;
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
            }
        }
    }

    private bool IsTransientError(Exception ex)
    {
        // Check for transient AWS errors
        return ex is AmazonS3Exception s3Ex && (
            s3Ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
            s3Ex.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
            s3Ex.ErrorCode == "RequestTimeout" ||
            s3Ex.ErrorCode == "SlowDown"
        ) || ex is AmazonDynamoDBException dynamoEx && (
            dynamoEx.ErrorCode == "ProvisionedThroughputExceededException" ||
            dynamoEx.ErrorCode == "ThrottlingException" ||
            dynamoEx.ErrorCode == "RequestLimitExceeded"
        );
    }
}
