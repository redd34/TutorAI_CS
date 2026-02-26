using System;

namespace AITutorSystem.Memory.Tests;

// Simple test model for unit tests
public class TestModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

// Another test model for testing different types
public class AnotherTestModel
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}