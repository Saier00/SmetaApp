using System.Linq;

namespace SmetaApp.Models
{
    public interface IMatRepository
    {
        IQueryable<Mat> Mats
        {
            get;
        }
        Mat ReadMat(int Id);
    }
}
