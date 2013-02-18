using System.Collections.Generic;
using System.Linq;

namespace SqlTrigger.Data
{
    public interface IFishRepository
    {
        Fish GetFish(int id);
        void AddFish(int id, int count);
        FishType[] GetFishTypes();
    }

    public class FishRepository : IFishRepository
    {
        private FishyDb _context;

        public FishRepository()
        {
            _context = new FishyDb();
        }

        public Fish GetFish(int id)
        {
            return _context.Set<Fish>().Single(f => f.Id == id);
        }

        public void AddFish(int id, int count)
        {
            var fish = GetFish(id);
            fish.Count += count;
            _context.SaveChanges();
        }

        public FishType[] GetFishTypes()
        {
            return _context.Set<FishType>().ToArray();
        }
    }
}