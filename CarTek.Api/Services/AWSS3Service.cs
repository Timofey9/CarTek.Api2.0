using Amazon.S3.Transfer;
using Amazon.S3;
using CarTek.Api.Services.Interfaces;

namespace CarTek.Api.Services
{
    public class AWSS3Service : IAWSS3Service
    {
        private IAWSS3ClientFactory _awsS3ClientFactory;

        public AWSS3Service(IAWSS3ClientFactory awsS3ClientFactory)
        {
            _awsS3ClientFactory = awsS3ClientFactory;
        }

        public async Task UploadFileToS3(IFormFile file, string folderName, string fileName, string bucket, string contentType = "image/jpeg")
        {
            using (var s3Client = _awsS3ClientFactory.GetClient())
            {
                using (var newMemoryStream = new MemoryStream())
                {
                    file.CopyTo(newMemoryStream);

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = $"{folderName}/{fileName}",
                        BucketName = "cartek",
                        CannedACL = S3CannedACL.PublicRead,
                        ContentType = file.ContentType
                    };

                    var fileTransferUtility = new TransferUtility(s3Client);

                    await fileTransferUtility.UploadAsync(uploadRequest);
                }
            }
        }
    }
}
