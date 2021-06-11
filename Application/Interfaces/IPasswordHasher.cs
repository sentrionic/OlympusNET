using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPasswordHasher
    {
        Task<byte[]> Hash(string password, byte[] salt);
    }
}