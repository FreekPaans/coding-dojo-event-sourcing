using System.Collections.Generic;
using System.Linq;

namespace NerdDinner.Models {

    public interface IDinnerRepository : IRepository<Dinner>
    {
        IQueryable<Dinner> FindByLocation(float latitude, float longitude);
        IQueryable<Dinner> FindUpcomingDinners();
        IQueryable<Dinner> FindDinnersByText(string q);
        IQueryable<PopularDinner> FindMostPopularDinners();

        void StoreEvents(ICollection<Event> events); // TODO naar eventrepository?
        IQueryable<Event> AllEvents { get; }
    }
}
