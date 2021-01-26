using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace SmetaApp.Models
{
    public class EFRepository : IDisposable, IJobRepository, IMechRepository, IMechPriceRepository, IMatRepository, IMatPriceRepository, IMechNameMapRepository, IMatNameMapRepository
    {
        private SmetaContext Db=new SmetaContext();

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Db != null)
                {
                    Db.Dispose();
                    Db = null;
                }
            }
        }
        //<IDisposable>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //</IDisposable>

        //<IJobRepository>
        public IQueryable<Job> Jobs
        {
            get
            {
                return Db.Jobs.Include(j=>j.Mechs.Select(m=>m.MechNameMap.MechPrice)).Include(j=>j.Mats.Select(m=>m.MatNameMap.MatPrice));
            }
        }
        public void CreateJob(Job j)
        {
            CJ(j, false);
        }
        public void CreateJobWithMaps(Job j)
        {
            CJ(j, true);
        }
        private void CJ(Job j, bool wm)
        {
            //Adding Mech/MatNameMaps to j Mechs/Mats:
            foreach (Mech m in j.Mechs)
            {
                MechNameMap mnm = ReadMechNameMap(m.Name);
                if (wm)
                {
                    if (mnm == null)
                    {
                        mnm = new MechNameMap()
                        {
                            MechName = m.Name
                        };
                        Db.MechNameMaps.Add(mnm);
                    }
                }
                m.MechNameMap = mnm;
                m.Job = j;
                Db.Mechs.Add(m);
            }

            foreach (Mat m in j.Mats)
            {
                MatNameMap mnm = ReadMatNameMap(m.Name);
                if (wm)
                {
                    if (mnm == null)
                    {
                        mnm = new MatNameMap()
                        {
                            MatName = m.Name
                        };
                        Db.MatNameMaps.Add(mnm);
                    }
                }
                m.MatNameMap = mnm;
                m.Job = j;
                Db.Mats.Add(m);
            }

            Db.Jobs.Add(j);
            Db.SaveChanges();
        }
        public Job ReadJob(int Id)
        {
            return Db.Jobs.Include(j => j.Mechs.Select(m => m.MechNameMap.MechPrice)).Include(j => j.Mats.Select(m => m.MatNameMap.MatPrice)).FirstOrDefault(j => j.Id == Id);
        }
        public void UpdateJob(Job j)
        {
            //Read Job with no tracking, because it cause troubles with mapping entities
            Job old = Db.Jobs.AsNoTracking().Include(job => job.Mechs.Select(m => m.MechNameMap.MechPrice)).Include(job => job.Mats.Select(m => m.MatNameMap.MatPrice)).FirstOrDefault(job => job.Id == j.Id);
            //Adding Mech/MatNameMaps to j Mechs/Mats:
            foreach (Mech m in j.Mechs)
            {
                MechNameMap mnm = ReadMechNameMap( m.Name);
                /*Supposed it has been created before update
                if (mnm == null)
                {
                    mnm = new MechNameMap()
                    {
                        MechName=m.Name
                    };
                    CreateMechNameMap(mnm);
                }
                */
                m.MechNameMap = mnm;
            }

            foreach (Mat m in j.Mats)
            {
                MatNameMap mnm = ReadMatNameMap(m.Name);
                /*
                if (mnm == null)
                {
                    mnm = new MatNameMap()
                    {
                        MatName = m.Name
                    };
                    CreateMatNameMap(mnm);
                }
                */
                m.MatNameMap = mnm;
            }


            IEnumerable<string> MatNameToDel = old.Mats.Select(m => m.Name).Where(n => !j.Mats.Select(m => m.Name).Contains(n));
            IEnumerable<string> MechNameToDel = old.Mechs.Select(m => m.Name).Where(n => !j.Mechs.Select(m => m.Name).Contains(n));

            List<Mech> MechToDel = new List<Mech>();
            List<Mat> MatToDel = new List<Mat>();
            List<MatNameMap> MatNameMapToUp = new List<MatNameMap>();
            List<MechNameMap> MechNameMapToUp = new List<MechNameMap>();

            List<string> MatToAdd = j.Mats.Select(m => m.Name).Where(n => !old.Mats.Select(m => m.Name).Contains(n)).ToList();
            List<string> MechToAdd = j.Mechs.Select(m => m.Name).Where(n => !old.Mechs.Select(m => m.Name).Contains(n)).ToList();
            //Delete
            foreach (Mat m in old.Mats.Where(m => MatNameToDel.Contains(m.Name)))
            {
                MatNameMap mnm = ReadMatNameMap(m.Name);
                MatToDel.Add(ReadMat(m.MatId));
                MatNameMapToUp.Add(mnm);
            }
            foreach (Mech m in old.Mechs.Where(m => MechNameToDel.Contains(m.Name)))
            {
                MechNameMap mnm = ReadMechNameMap(m.Name);
                MechToDel.Add(ReadMech(m.MechId));
                MechNameMapToUp.Add(mnm);
            }

            Db.Mats.RemoveRange(MatToDel);
            Db.Mechs.RemoveRange(MechToDel);

            foreach(MatNameMap mnm in MatNameMapToUp)
            {
                UpdateMatNameMap(mnm);
            }
            foreach (MechNameMap mnm in MechNameMapToUp)
            {
                UpdateMechNameMap(mnm);
            }
            //Add
            foreach (Mat mat in j.Mats.Where(m => MatToAdd.Contains(m.Name)))
            {
                mat.JobId = j.Id;
                Db.Mats.Add(mat);
            }
            foreach (Mech mech in j.Mechs.Where(m => MechToAdd.Contains(m.Name)))
            {
                mech.JobId = j.Id;
                Db.Mechs.Add(mech);
            }
            //Update if not deleted/added
            foreach (Mech mech in j.Mechs.Where(m => !MechToAdd.Contains(m.Name)))
            {
                mech.JobId = j.Id;
                Db.Entry(mech).State = EntityState.Modified;
            }
            foreach (Mat mat in j.Mats.Where(m => !MatToAdd.Contains(m.Name)))
            {
                mat.JobId = j.Id;
                Db.Entry(mat).State = EntityState.Modified;
            }
            Db.Entry(j).State = EntityState.Modified;
            Db.SaveChanges();
        }
        public void DeleteJob(int Id)
        {
            Job job = Db.Jobs.FirstOrDefault(j => j.Id == Id);
            if(job!=null)
            {
                Db.Mechs.RemoveRange(job.Mechs);
                Db.Mats.RemoveRange(job.Mats);
                Db.Jobs.Remove(job);
                Db.SaveChanges();
            }
        }
        //</IJobRepository>

        //<IMechRepository>
        public IQueryable<Mech> Mechs
        {
            get
            {
                return Db.Mechs;
            }
        }
        public Mech ReadMech(int Id)
        {
            return Db.Mechs.FirstOrDefault(m => m.MechId==Id);
        }
        //</IMechRepository>

        //<IMechPriceRepository>
        public IQueryable<MechPrice> MechPrices
        {
            get
            {
                return Db.MechPrices.Include(mp => mp.AnotherNames);
            }
        }
        public void CreateMechPrice(MechPrice mp)
        {
            mp.AnotherNames.Clear();

            MechNameMap mnm = ReadMechNameMap(mp.Name);
            if (mnm == null)
            {
                mnm = new MechNameMap()
                {
                    MechName = mp.Name,
                    MechPriceName = mp.Name
                };
            }
            else
            {
                string oldPrice = mnm.MechPriceName;
                if (oldPrice == null)
                {
                    mnm.MechPriceName = mp.Name;
                    Db.Entry(mnm).State = EntityState.Modified;
                }
                else if (oldPrice == mp.Name)
                {
                    Db.Entry(mnm).State = EntityState.Modified;
                }
                else
                {
                    var mnms = MechNameMaps.Where(m => m.MechPriceName == oldPrice).ToList();
                    foreach (MechNameMap mtoup in mnms)
                    {
                        mtoup.MechPriceName = mp.Name;
                        mtoup.MechPrice = mp;
                        Db.Entry(mtoup).State = EntityState.Modified;
                    }
                }
            }
            mp.AnotherNames.Add(mnm);
            Db.MechPrices.Add(mp);
            Db.SaveChanges();
        }
        public MechPrice ReadMechPrice(string Name)
        {
            return Db.MechPrices.Include(mp => mp.AnotherNames).FirstOrDefault(mp => mp.Name == Name || mp.AnotherNames.Select(an => an.MechName).Contains(Name));
        }
        public void UpdateMechPrice(MechPrice mp)
        {
            List<MechNameMap> oldmnms = Db.MechNameMaps.AsNoTracking().Where(m => m.MechPriceName == mp.Name).ToList();
            List<MechNameMap> toDel = oldmnms.Where(m => !mp.AnotherNames.Select(an => an.MechName).Contains(m.MechName)).ToList();
            //Adding mp name to mnm
            foreach (MechNameMap mnm in mp.AnotherNames)
            {
                mnm.MechPriceName = mp.Name;
                if (oldmnms.Any(m => m.MechName == mnm.MechName))
                    UpdateMechNameMap(mnm);
                else
                    CreateMechNameMap(mnm);
            }
            foreach (MechNameMap mnm in toDel)
            {
                mnm.MechPriceName = null;
                UpdateMechNameMap(mnm);
            }
            Db.Entry(mp).State = EntityState.Modified;
            Db.SaveChanges();
        }
        public void DeleteMechPrice(string Name)
        {
            MechPrice mp = ReadMechPrice(Name);
            if (mp != null)
            {
                List<MechNameMap> mnms = new List<MechNameMap>(mp.AnotherNames);
                foreach (MechNameMap mnm in mnms)
                {
                    mnm.MechPriceName = null;
                    Db.Entry(mnm).State = EntityState.Modified;
                }

                Db.MechPrices.Remove(mp);

                Db.SaveChanges();
            }
        }
        //</IMechPriceRepository>

        //<IMatRepository>
        public IQueryable<Mat> Mats
        {
            get
            {
                return Db.Mats;
            }
        }
        public Mat ReadMat(int Id)
        {
            return Db.Mats.FirstOrDefault(m => m.MatId == Id);
        }
        //</IMatRepository>

        //<IMatPriceRepository>
        public IQueryable<MatPrice> MatPrices
        {
            get
            {
                return Db.MatPrices.Include(mp => mp.AnotherNames);
            }
        }
        public void CreateMatPrice(MatPrice mp)
        {
            mp.AnotherNames.Clear();

            MatNameMap mnm = ReadMatNameMap(mp.Name);
            if (mnm == null)
            {
                mnm = new MatNameMap() {
                    MatName=mp.Name,
                    MatPriceName=mp.Name
                };
            }
            else
            {
                string oldPrice = mnm.MatPriceName;
                if (oldPrice == null)
                {
                    mnm.MatPriceName = mp.Name;
                    Db.Entry(mnm).State = EntityState.Modified;
                }
                else if (oldPrice == mp.Name)
                {
                    Db.Entry(mnm).State = EntityState.Modified;
                }
                else
                {
                    var mnms = MatNameMaps.Where(m => m.MatPriceName == oldPrice).ToList();
                    foreach(MatNameMap mtoup in mnms)
                    {
                        mtoup.MatPriceName = mp.Name;
                        mtoup.MatPrice = mp;
                        Db.Entry(mtoup).State = EntityState.Modified;
                    }
                }
            }
            mp.AnotherNames.Add(mnm);
            Db.MatPrices.Add(mp);
            Db.SaveChanges();
        }
        public MatPrice ReadMatPrice(string Name)
        {
            return Db.MatPrices.Include(mp=>mp.AnotherNames).FirstOrDefault(mp => mp.Name == Name||mp.AnotherNames.Select(an=>an.MatName).Contains(Name));
        }
        public void UpdateMatPrice(MatPrice mp)
        {
            List<MatNameMap> oldmnms = Db.MatNameMaps.AsNoTracking().Where(m => m.MatPriceName == mp.Name).ToList();
            List<MatNameMap> toDel = oldmnms.Where(m => !mp.AnotherNames.Select(an => an.MatName).Contains(m.MatName)).ToList();
            //Adding mp name to mnm
            foreach (MatNameMap mnm in mp.AnotherNames)
            {
                mnm.MatPriceName = mp.Name;
                if (oldmnms.Any(m => m.MatName == mnm.MatName))
                    UpdateMatNameMap(mnm);
                else
                    CreateMatNameMap(mnm);
            }
            foreach (MatNameMap mnm in toDel)
            {
                mnm.MatPriceName = null;
                UpdateMatNameMap(mnm);
            }
            Db.Entry(mp).State = EntityState.Modified;
            Db.SaveChanges();
        }
        public void DeleteMatPrice(string Name)
        {
            MatPrice mp = ReadMatPrice(Name);
            if (mp != null)
            {
                List<MatNameMap> mnms = new List<MatNameMap>(mp.AnotherNames);
                foreach (MatNameMap mnm in mnms)
                {
                    mnm.MatPriceName = null;
                    Db.Entry(mnm).State = EntityState.Modified;
                }

                Db.MatPrices.Remove(mp);

                Db.SaveChanges();
            }
        }
        //</IMatPriceRepository>

        //<IMatNameMapRepository>
        public IQueryable<MatNameMap> MatNameMaps
        {
            get 
            {
                return Db.MatNameMaps;
            }
        }
        public void CreateMatNameMap(MatNameMap mp)
        {
            Db.MatNameMaps.Add(mp);
            Db.SaveChanges();
        }
        public MatNameMap ReadMatNameMap(string MatName)
        {
            return Db.MatNameMaps.FirstOrDefault(mp => mp.MatName == MatName);
        }
        public void UpdateMatNameMap(MatNameMap mp)
        {
            Db.Entry(mp).State = EntityState.Modified;
            Db.SaveChanges();
        }
        public void DeleteMatNameMap(string MatName)
        {
            MatNameMap mp = Db.MatNameMaps.FirstOrDefault(m => m.MatName == MatName);
            if (mp != null)
            {
                Db.MatNameMaps.Remove(mp);
                Db.SaveChanges();
            }
        }
        //</IMatNameMapRepository>

        //<IMechNameMapRepository>
        public IQueryable<MechNameMap> MechNameMaps
        {
            get 
            {
                return Db.MechNameMaps;
            }
        }

        public void CreateMechNameMap(MechNameMap mp)
        {
            Db.MechNameMaps.Add(mp);
            Db.SaveChanges();
        }
        public MechNameMap ReadMechNameMap(string MechName) 
        {
            return Db.MechNameMaps.FirstOrDefault(mp => mp.MechName == MechName);
        }
        public void UpdateMechNameMap(MechNameMap mp)
        {
            Db.Entry(mp).State = EntityState.Modified;
            Db.SaveChanges();
        }
        public void DeleteMechNameMap(string MechName)
        {
            MechNameMap mp = Db.MechNameMaps.FirstOrDefault(m => m.MechName == MechName);
            if (mp != null)
            {
                Db.MechNameMaps.Remove(mp);
                Db.SaveChanges();
            }
        }
        //</IMechNameMapRepository>
    }
}