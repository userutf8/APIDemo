using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static APIDemo.Model.Enums;

namespace APIDemo.Model.DB
{
    public class ClientFile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey("Clients")]
        public Guid ClientId { get; set; }

        [ForeignKey("Files")]
        public Guid FileId { get; set; }
        public ClientStatus Status { get; set; } = 0; // New
        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddYears(1);

        public override string ToString() =>
            $"{base.ToString()} ClientID={ClientId} FileID={FileId} Status={Status} ExpirationDate={ExpirationDate}"; // doesn't account for ID

        public override bool Equals(object? obj) => obj?.ToString() == ToString();

        public override int GetHashCode() => ToString().GetHashCode();
    }
}
