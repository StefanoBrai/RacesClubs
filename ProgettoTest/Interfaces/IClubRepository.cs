using ProgettoTest.Models;
using X.PagedList;

namespace ProgettoTest.Interfaces
{
    public interface IClubRepository
    {
        IQueryable<Club> GetAll();
        Task<IPagedList<Club>> GetPagedClubsAsync(int pageNumber, int pageSize);
        Task<Club> GetByIdAsync(int id);
        Task<IEnumerable<Club>> GetClubByCity(string city);
        bool Add(Club club);
        bool Update(Club club);
        bool Delete(Club club);
        bool Save();
        Task<Club> GetByIdAsyncNoTracking(int id);
    }
}
