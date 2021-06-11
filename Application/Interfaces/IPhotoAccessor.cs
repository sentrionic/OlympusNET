using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IPhotoAccessor
    {
        Task<string> AddArticleImage(IFormFile file, string directory);
        Task<string> AddProfileImage(IFormFile file, string directory);
        void DeletePhoto(string filename);
    }
}