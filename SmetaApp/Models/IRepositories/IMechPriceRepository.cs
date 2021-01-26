using System.Linq;

namespace SmetaApp.Models
{
    public interface IMechPriceRepository
    {
        IQueryable<MechPrice> MechPrices
        {
            get;
        }
        void CreateMechPrice(MechPrice mp);
        MechPrice ReadMechPrice(string Name);
        void UpdateMechPrice(MechPrice mp);
        void DeleteMechPrice(string Name);
    }
}
