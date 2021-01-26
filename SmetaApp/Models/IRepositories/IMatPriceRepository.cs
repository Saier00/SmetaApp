using System.Linq;

namespace SmetaApp.Models
{
    public interface IMatPriceRepository
    {
        IQueryable<MatPrice> MatPrices
        {
            get;
        }
        void CreateMatPrice(MatPrice mp);
        MatPrice ReadMatPrice(string Name);
        void UpdateMatPrice(MatPrice mp);
        void DeleteMatPrice(string Name);
    }
}
