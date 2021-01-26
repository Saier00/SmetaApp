using System.Linq;

namespace SmetaApp.Models
{
    public interface IJobRepository
    {
        IQueryable<Job> Jobs
        {
            get;
        }
        void CreateJob(Job j);
        void CreateJobWithMaps(Job j);
        Job ReadJob(int Id);
        void UpdateJob(Job j);
        void DeleteJob(int Id);
    }
}
