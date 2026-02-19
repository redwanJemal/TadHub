namespace SaasKit.Infrastructure.Settings;

public sealed class MinioSettings
{
    public const string SectionName = "Minio";

    public string Endpoint { get; init; } = "localhost:9000";
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public bool UseHttps { get; init; } = false;
    public string DefaultBucket { get; init; } = "uploads";
    public string BucketPrefix { get; init; } = "saaskit-";
}
