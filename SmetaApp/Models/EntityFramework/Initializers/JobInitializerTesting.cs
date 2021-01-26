using System.Data.Entity;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SmetaApp.Models
{
    public class JobInitializerTesting
    {
        static public void Seed(SmetaContext db)
        {
            /*
            MatPrice matp=new MatPrice ()
            { 
                Name = "Шнур асбестовый общего назначения марки: ШАОН диаметром 8-10 мм",
                TableNumber = 1,
                Eur = (decimal?)10.3,
                Discount = 10,
                Markup = 1,
                ProviderPrice = 15 
            };
            

            MechPrice mechp=new MechPrice ()
            { 
                Name = "Лебедки электрические тяговым усилием: 19,62 кН (2 т)",
                TableNumber = 1,
                Eur = (decimal?)10.3,
                Discount = 10,
                Markup = 1,
                ProviderPrice = 15 
            };

            

            Mat mat=new Mat ()
            { 
                Name= "Шнур асбестовый общего назначения марки: ШАОН диаметром 8-10 мм",
                Units = "т", 
                Amount = 10,
            };
            mat.MatPrices.Add(matp);
            

            Mech mech=new Mech()
            { 
                Name= "Лебедки электрические тяговым усилием: 19,62 кН (2 т)", 
                Amount = 10
            };
            mech.MechPrices.Add(mechp);

            

            Job job=new Job()
            {
                Name = "Прокладка воздуховодов из листовой, оцинкованной стали и алюминия класса Н (нормальные) толщиной:",
                Type = "0,5 мм, диаметром до 200 мм",
                Code = "20-01-001-01",
                Producer = "Someone",
                Measurer = 100,
                Units = "м2",
                Amount = 1,
                WLaborCosts = 161.3,
                MLaborCosts = 1.3,
                AvRank = 3.2
            };
            job.Mechs.Add(mech);
            job.Mats.Add(mat);

            db.MatPrices.Add(matp);
            db.MechPrices.Add(mechp);
            db.Mats.Add(mat);
            db.Mechs.Add(mech);
            db.Jobs.Add(job);
            */
            
            Random rnd = new Random();
            
            List<MatPrice> maps = new List<MatPrice>();
            List<MechPrice> meps = new List<MechPrice>();
            List<Mat> mats = new List<Mat>();
            List<Mech> mechs = new List<Mech>();
            List<Job> jobs = new List<Job>();
            List<MatNameMap> Matnm = new List<MatNameMap>();
            List<MechNameMap> Mechnm = new List<MechNameMap>();

            for (int i = 0; i < 20; i++)
            {
                MatPrice matprnd = new MatPrice()
                {
                    Name = "Гвоздь"+i,
                    Discount = 10
                };
                switch (Math.Round(2 * rnd.NextDouble()))
                {
                    case 0:
                        matprnd.Eur = (decimal?)Math.Round(100*rnd.NextDouble(),2);
                        break;
                    case 1:
                        matprnd.Dol = (decimal?)Math.Round(100 * rnd.NextDouble(), 2);
                        break;
                    case 2:
                        matprnd.Rub = (decimal?)Math.Round(100 * rnd.NextDouble(), 2);
                        break;
                }

                MechPrice mechprnd = new MechPrice()
                {
                    Name = "Гвоздезабиватель" + i,
                    Discount = 10
                };
                switch (Math.Round(2 * rnd.NextDouble()))
                {
                    case 0:
                        mechprnd.Eur = (decimal?)Math.Round(100 * rnd.NextDouble(),2);
                        break;
                    case 1:
                        mechprnd.Dol = (decimal?)Math.Round(100 * rnd.NextDouble(), 2);
                        break;
                    case 2:
                        mechprnd.Rub = (decimal?)Math.Round(100 * rnd.NextDouble(), 2);
                        break;
                }
                maps.Add(matprnd);
                meps.Add(mechprnd);
            }
            db.MatPrices.AddRange(maps);
            db.MechPrices.AddRange(meps);
            db.SaveChanges();


            for (int i=0;i<500;i++)
            {
                Job jobrnd = new Job()
                {
                    Name = "Забивание гвоздей"+i,
                    Type = "0,5 мм, диаметром до 200 мм",
                    Code = "20-01-001-"+i,
                    Producer = "Someone",
                    Measurer = 100,
                    Units = "м2",
                    Amount = 1,
                    WLaborCosts = (decimal)(500 * rnd.NextDouble()),
                    MLaborCosts = (decimal)(500 * rnd.NextDouble()),
                    AvRank = (decimal)(5 * rnd.NextDouble())
                };

                for (int k = 0; k < rnd.Next(0, 10); k++)
                {

                    Mat matrnd = new Mat()
                    {
                        Units = "т",
                        Amount = (decimal)(100 * rnd.NextDouble())
                    };
                    /*
                    if (Math.Round(rnd.NextDouble()) == 1)
                    {
                        switch (Math.Round(2 * rnd.NextDouble()))
                        {
                            case 0:
                                matrnd.AlterMatPrice.Eur = (decimal?)Math.Round(100 * rnd.NextDouble(), 2);
                                break;
                            case 1:
                                matrnd.AlterMatPrice.Dol = (decimal?)Math.Round(100 * rnd.NextDouble(), 2);
                                break;
                            case 2:
                                matrnd.AlterMatPrice.Rub = (decimal?)Math.Round(100 * rnd.NextDouble(), 2);
                                break;
                        }
                    }
                    */
                    int a = rnd.Next(0, 19);
                    MatNameMap mnm = Matnm.FirstOrDefault( ma => ma.MatName == "Гвоздь" + a);
                    if (mnm == null)
                    {
                        mnm = new MatNameMap() { MatPrice = maps.First(ma => ma.Name == "Гвоздь" + a) };
                        mnm.MatName = mnm.MatPrice.Name;
                        Matnm.Add(mnm);
                    }
                    mnm.Mats.Add(matrnd);
                    matrnd.MatNameMap = mnm;
                    mnm.MatName = matrnd.Name=mnm.MatPrice.Name;
                    jobrnd.Mats.Add(matrnd);
                    mats.Add(matrnd);
                }
                for (int k = 0; k < rnd.Next(0, 10); k++)
                {
                    Mech mechrnd = new Mech()
                    {
                        Amount = rnd.Next(0, 100)
                    };
                    /*
                    if (Math.Round(rnd.NextDouble()) == 1)
                    {
                        switch (Math.Round(2 * rnd.NextDouble()))
                        {
                            case 0:
                                mechrnd.AlterMechPrice.Eur = (decimal?)Math.Round(100 * rnd.NextDouble(), 2);
                                break;
                            case 1:
                                mechrnd.AlterMechPrice.Dol = (decimal?)Math.Round(100 * rnd.NextDouble(), 2);
                                break;
                            case 2:
                                mechrnd.AlterMechPrice.Rub = (decimal?)Math.Round(100 * rnd.NextDouble(), 2);
                                break;
                        }
                    }
                    */
                    int a = rnd.Next(0, 19);
                    MechNameMap mnm = Mechnm.FirstOrDefault(m => m.MechName == "Гвоздезабиватель" + a);
                    if (mnm == null)
                    {
                        mnm = new MechNameMap() { MechPrice = meps.First(m => m.Name == "Гвоздезабиватель" + a) };
                        mnm.MechName = mnm.MechPrice.Name;
                        Mechnm.Add(mnm);
                    }
                    mnm.Mechs.Add(mechrnd);
                    mechrnd.MechNameMap = mnm;
                    mechrnd.Name=mnm.MechName;
                    jobrnd.Mechs.Add(mechrnd);
                    mechs.Add(mechrnd);
                }
                jobs.Add(jobrnd);
            }
            db.Mats.AddRange(mats);
            db.Mechs.AddRange(mechs);
            db.Jobs.AddRange(jobs);
            db.MatNameMaps.AddRange(Matnm);
            db.MechNameMaps.AddRange(Mechnm);
            db.SaveChanges();
        }
    }
}