using System.Linq;

namespace SmetaApp.Models
{
    public interface IMechNameMapRepository
    {
        IQueryable<MechNameMap> MechNameMaps
        {
            get;
        }
        void CreateMechNameMap(MechNameMap mp);
        MechNameMap ReadMechNameMap(string MechName);
        void UpdateMechNameMap(MechNameMap mp);
        void DeleteMechNameMap(string MechName);
    }
}