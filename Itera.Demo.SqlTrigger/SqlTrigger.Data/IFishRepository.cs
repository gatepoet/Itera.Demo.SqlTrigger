using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SqlTrigger.Data
{
    public interface IFishRepository
    {
        int GetFishCount(int typeId);
        void AddFish(int typeId, int amount);
        void FishSold(int typeId, int amount);
        FishType[] GetFishTypes();
    }

    public class FishRepository : IFishRepository
    {
        private readonly FishyEntities _context;

        public FishRepository()
        {
            _context = new FishyEntities();
        }

        public int GetFishCount(int typeId)
        {
            return _context.Fish
                .Where(f => f.TypeId == typeId)
                .Aggregate(0, (c,f)=> f.Count);
        }

        public void AddFish(int typeId, int count)
        {
            AddFish(new Fish { Count = count, TypeId = typeId });
        }

        public void AddFish(Fish fish)
        {
            _context.Fish.Add(fish);
            _context.SaveChanges();
        }

        public void AddFishType(string name)
        {
            _context.FishType.Add(new FishType {Name = name});
            _context.SaveChanges();
        }


        public void FishSold(int typeId, int amount)
        {
            _context.Fish.Add(new Fish { Count = -amount, TypeId = typeId });
            _context.SaveChanges();
        }

        public FishType[] GetFishTypes()
        {
            return _context.FishType.ToArray();
        }

        public FishDto[] GetFish()
        {
            return _context.Fish
                           .GroupBy(f => f.FishType)
                           .Select(g => new FishDto{Type = g.Key, Count = g.Sum(f=> f.Count)})
                           .ToArray();
        }
    }

    public class FishDto
    {
        internal FishDto(){}
        public FishDto(FishType type, int count)
        {
            Type = type;
            Count = count;
        }

        public int Count { get; internal set; }
        public FishType Type { get; internal set; }
    }
}