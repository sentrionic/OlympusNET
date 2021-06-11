using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profile
{
    public class ProfileReader : IProfileReader
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public ProfileReader(DataContext context, IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
            _context = context;
        }

        public async Task<Profile> ReadProfile(Domain.User user, Domain.User currentUser = null)
        {
            currentUser ??= await _context.Users
                .AsSplitQuery()
                .Include(x => x.Followings)
                .Include(x => x.Followers)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Username == _userAccessor.GetCurrentUsername());

            var profile = new Profile
            {
                Id = user.Id,
                Username = user.Username,
                Bio = user.Bio,
                Image = user.Image,
                Followers = user.Followers.Count,
                Followee = user.Followings.Count
            };

            if (currentUser != null && currentUser.Followings.Any(x => x.TargetId == user.Id)) profile.Following = true;

            return profile;
        }
    }
}