using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SmetaApp.Models
{
    public class Job
    {
        public Job()
        {
            Mechs = new List<Mech>();
            Mats = new List<Mat>();
        }
        [Key]
        public int Id { get; set; }
        //Name+Type has unique key constrant
        [Index("IX_FirstAndSecond", 1, IsUnique = true)]
        [MaxLength(150)]
        public string Name { get; set; }
        [Index("IX_FirstAndSecond", 2, IsUnique = true)]
        [MaxLength(150)]
        public string Type { get; set; }
        [Required]
        [StringLength(150)]
        [Index(IsUnique = true)]
        public string Code { get; set; }
        public string Producer { get; set; }
        public int? Measurer { get; set; }
        public string Units { get; set; }
        public decimal Amount { get; set; }
        public decimal? WLaborCosts { get; set; }
        public decimal? AvRank { get; set; }
        public decimal? MLaborCosts { get; set; }
        public decimal? Mass { get; set; }
        public string Note { get; set; }
        public  ICollection<Mech> Mechs { get; set; }
        public  ICollection<Mat> Mats { get; set; }
        //Job's page, can be added when it is creating, or sometime else
        public Page Page { get; set; }
        public string PageName { get; set; }
        //Пусконаладочные работы
    }

    public class Mech
    {
        public Mech()
        {
            //AlterMechPrice = new AlterMechPrice();
        }
        [Key]
        public int MechId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string Name { get; set; }
        public MechNameMap MechNameMap { get; set; }
        //public AlterMechPrice AlterMechPrice { get; set; }
        [JsonIgnore]
        public Job Job { get; set; }
        [JsonIgnore]
        public int JobId { get; set; }
    }
    public class Mat
    {
        public Mat()
        {
            //AlterMatPrice = new AlterMatPrice();
        }
        [Key]
        public int MatId { get; set; }
        [Required]
        public string Units { get; set; }
        [Required]
        public string Name { get; set; }
        public MatNameMap MatNameMap { get; set; }
        [Required]
        public decimal Amount { get; set; }
        //public AlterMatPrice AlterMatPrice { get; set; }
        [JsonIgnore]
        public Job Job { get; set; }
        [JsonIgnore]
        public int JobId { get; set; }
    }
    public class MechNameMap
    {
        public MechNameMap()
        {
            Mechs = new List<Mech>();
        }
        [Key]
        [JsonIgnore]
        public string MechName { get; set; }
        public string MechPriceName { get; set; }
        public MechPrice MechPrice { get; set; }
        [JsonIgnore]
        public ICollection<Mech> Mechs { get; set; }
    }
    public class MatNameMap
    {
        public MatNameMap()
        {
            Mats = new List<Mat>();
        }
        [Key]
        [JsonIgnore]
        public string MatName { get; set; }
        public string MatPriceName { get; set; }
        public MatPrice MatPrice { get; set; }
        [JsonIgnore]
        public ICollection<Mat> Mats { get; set; }
    }
    public class MechPrice
    {
        public MechPrice()
        {
            AnotherNames = new List<MechNameMap>();
        }
        [Key]
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal? Eur { get; set; }
        public decimal? Dol { get; set; }
        public decimal? Rub { get; set; }
        public decimal? Discount { get; set; }
        [JsonIgnore]
        public ICollection<MechNameMap> AnotherNames { get; set; }
    }
    public class MatPrice
    {
        public MatPrice()
        {
            AnotherNames = new List<MatNameMap>();
        }
        [Key]
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal? Eur { get; set; }
        public decimal? Dol { get; set; }
        public decimal? Rub { get; set; }
        public decimal? Discount { get; set; }
        [JsonIgnore]
        public ICollection<MatNameMap> AnotherNames { get; set; }
    }

    public class Page
    {
        [Key]
        public string Name { get; set; }
        public ICollection<Job> Jobs { get; set; }
    }

    /*
    //Optional additional prices for MechPrices
    [ComplexType]
    public class AlterMechPrice
    {
        public decimal? Eur { get; set; }
        public decimal? Dol { get; set; }
        public decimal? Rub { get; set; }
    }
    //Optional additional prices for MechPrices
    [ComplexType]
    public class AlterMatPrice
    {
        public decimal? Eur { get; set; }
        public decimal? Dol { get; set; }
        public decimal? Rub { get; set; }
    }
    */

    //Utility models:
    //AddJobs:
    public class NameType
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
    //Prices:
    public class MatPriceToDel
    {
        public MatPrice MatPrice { get; set; }
        public bool toDel { get; set; }
        public string oldName { get; set; }
    }
    public class MechPriceToDel
    {
        public MechPrice MechPrice { get; set; }
        public bool toDel { get; set; }
        public string oldName { get; set; }
    }
    //Binding models:
    public class BindingModel
    {
        public string New { get; set; }
        public string Old { get; set; }
    }
}