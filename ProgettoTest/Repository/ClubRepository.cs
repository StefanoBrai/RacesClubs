using Microsoft.EntityFrameworkCore;
using ProgettoTest.Data;
using ProgettoTest.Extensions;
using ProgettoTest.Interfaces;
using ProgettoTest.Models;
using X.PagedList;

namespace ProgettoTest.Repository
{
    public class ClubRepository : IClubRepository
    {
        private readonly ApplicationDbContext _context;
        public ClubRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(Club club)
        {
            _context.Add(club);
            return Save();
        }

        public bool Delete(Club club)
        {
            _context.Remove(club);

            return Save();
        }

        public IQueryable<Club> GetAll()
        {
            return _context.Clubs;
        }

        public async Task<IPagedList<Club>> GetPagedClubsAsync(int pageNumber, int pageSize)
        {
            return await _context.Clubs
                .OrderBy(c => c.Title) // Ordina per un campo, ad esempio il nome
                .ToPagedListAsync(pageNumber, pageSize);
        }

        public async Task<Club> GetByIdAsync(int id)
        {
            return await _context.Clubs.Include(c => c.Address).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Club> GetByIdAsyncNoTracking(int id)
        {
            return await _context.Clubs.Include(c => c.Address).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Club>> GetClubByCity(string city)
        {
            return await _context.Clubs.Where(c => c.Address.City == city).ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();

            return saved > 0 ? true : false;
        }

        public bool Update(Club club)
        {
            _context.Update(club);

            return Save();
        }
    }
}
