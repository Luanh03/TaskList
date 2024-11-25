using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskList.Models
{
    [Table("Tasks")]
    public class Tasks
    {
        [Column("Id")]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Column("Name")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Column("Cost")]
        [Display(Name = "Custo")]
        [Range(0, double.MaxValue, ErrorMessage = "O custo deve ser um valor positivo.")]
        public decimal Cost { get; set; }

        [Column("Deadline")]
        [Display(Name = "Data de Limite")]
        public DateTime Deadline { get; set; }

        [Column("PresentationOrder")]
        [Display(Name = "Order of Presentation")]
        public int PresentationOrder { get; set; }
    }
}
