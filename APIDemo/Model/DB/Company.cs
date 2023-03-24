using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static APIDemo.Model.Enums;

namespace APIDemo.Model.DB
{
    public class Company
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey("Clients")]
        public Guid ClientId { get; set; }
        public string? TIN { get; set; }

        [Required]
        public string Name { get; set; }
        public ClientStatus Status { get; set; } = 0; // New
    }
}
