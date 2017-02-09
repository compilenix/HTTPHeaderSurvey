using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Integration.DataAccess.Entitys
{
    public class ApplicationLog : BaseEntity
    {
        [Key]
        [Index]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string HostName { get; set; }

        [Required]
        public string Level { get; set; }

        [Required]
        public string Logger { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string Method { get; set; }

        [Required]
        public string Thread { get; set; }

        public string Exception { get; set; }
    }
}