using Amazon;
using Amazon.S3;
using CarTek.Api.Services.Interfaces;

namespace CarTek.Api.Services
{
    public class AWSS3ClientFactory : IAWSS3ClientFactory
    {
        public AmazonS3Client GetClient()
        {
            AmazonS3Config configsS3 = new AmazonS3Config
            {
                ServiceURL = "https://s3.yandexcloud.net"
            };

            var location = AWSConfigs.AWSProfilesLocation;

            AmazonS3Client s3client = new AmazonS3Client(configsS3);

            return s3client;
        }
    }
}
