namespace CarTek.Api.Services.Interfaces
{
    public interface IAWSS3Service
    {
        public Task UploadFileToS3(IFormFile file, string folderName, string fileName, string bucket, string contentType = "image/jpeg");

        public Task DeleteFromS3(string key);
    }
}
