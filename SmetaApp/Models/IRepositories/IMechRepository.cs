using System.Linq;


namespace SmetaApp.Models
{
    public interface IMechRepository
    {
        IQueryable<Mech> Mechs
        {
            get;
        }
        Mech ReadMech(int Id);
    }
}
