namespace Infrastructure.Photos
{
    public class S3Settings
    {
        public string AccessKey { get; set; }
        public string SecretAccessKey { get; set; }
        public string StorageBucketName { get; set; }
        public string Region { get; set; }
    }
}