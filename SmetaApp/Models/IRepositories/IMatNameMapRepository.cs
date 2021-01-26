using System.Linq;

namespace SmetaApp.Models
{
    public interface IMatNameMapRepository
    {
        IQueryable<MatNameMap> MatNameMaps
        {
            get;
        }
        void CreateMatNameMap(MatNameMap mp);
        MatNameMap ReadMatNameMap(string MatName);
        void UpdateMatNameMap(MatNameMap mp);
        void DeleteMatNameMap(string MatName);
    }
}