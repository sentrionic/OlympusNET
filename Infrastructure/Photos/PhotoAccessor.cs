using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Photos
{
    public class PhotoAccessor : IPhotoAccessor
    {
        private const int DimMax = 1080;
        private const int DimMin = 320;
        private static IAmazonS3 _client;

        public PhotoAccessor(IOptions<S3Settings> config)
        {
            _client = new AmazonS3Client(config.Value.AccessKey, config.Value.SecretAccessKey,
                RegionEndpoint.EUCentral1);
            StorageBucketName = config.Value.StorageBucketName;
            Region = config.Value.Region;
        }

        private string StorageBucketName { get; }
        private string Region { get; }

        public async Task<string> AddArticleImage(IFormFile file, string directory)
        {
            var extension = Path.GetExtension(file.FileName);
            var fileTransferUtility = new TransferUtility(_client);
            var key = $"files/{directory}{extension}";

            var stream = new MemoryStream();

            using (var image = await Image.LoadAsync(file.OpenReadStream()))
            {
                if (image.Height < DimMin || image.Width < DimMin)
                    image.Mutate(x => x.Resize(
                        new ResizeOptions
                        {
                            Size = new Size(DimMin, DimMin),
                            Mode = ResizeMode.Max
                        })
                    );
                else
                    image.Mutate(x => x.Resize(
                        new ResizeOptions
                        {
                            Size = new Size(DimMax, DimMax),
                            Mode = ResizeMode.Max
                        })
                    );

                await image.SaveAsJpegAsync(stream);
            }

            var fileTransferUtilityRequest = new TransferUtilityUploadRequest
            {
                BucketName = StorageBucketName,
                Key = key,
                InputStream = stream,
                ContentType = "image/jpg"
            };

            await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
            return $"https://{StorageBucketName}.s3.{Region}.amazonaws.com/{key}";
        }

        public async Task<string> AddProfileImage(IFormFile file, string directory)
        {
            var extension = Path.GetExtension(file.FileName);
            var fileTransferUtility = new TransferUtility(_client);
            var key = $"files/{directory}{extension}";

            var stream = new MemoryStream();

            using (var image = await Image.LoadAsync(file.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(150, 150));
                await image.SaveAsJpegAsync(stream);
            }

            var fileTransferUtilityRequest = new TransferUtilityUploadRequest
            {
                BucketName = StorageBucketName,
                Key = key,
                InputStream = stream,
                ContentType = "image/jpg"
            };

            await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
            return $"https://{StorageBucketName}.s3.{Region}.amazonaws.com/{key}";
        }

        public async void DeletePhoto(string filename)
        {
            var index = filename.IndexOf("files");

            if (index == -1) return;
            
            var key = filename.Substring(index);

            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = StorageBucketName,
                Key = key
            };

            await _client.DeleteObjectAsync(deleteObjectRequest);
        }
    }
}