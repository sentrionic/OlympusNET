using System.Threading.Tasks;

namespace Application.Profile
{
    public interface IProfileReader
    {
        Task<Profile> ReadProfile(Domain.User user, Domain.User currentUser = null);
    }
}