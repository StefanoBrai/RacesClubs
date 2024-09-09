using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace ProgettoTest.Extensions
{
    public static class IQueryableExtensions
    {
        public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new StaticPagedList<T>(items, pageNumber, pageSize, count);
        }
    }
}
