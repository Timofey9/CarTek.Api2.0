using Amazon.S3;

namespace CarTek.Api.Services.Interfaces
{
    public interface IAWSS3ClientFactory
    {
        public AmazonS3Client GetClient();
    }
}
