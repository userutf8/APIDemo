using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static APIDemo.Model.Enums;

namespace APIDemo.Model.DB
{
    public class Client
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public ClientType Type { get; set; } = 0; // Company

        [StringLength(256)]
        public string? Alias { get; set; }
        public ClientStatus Status { get; set; } = 0; // New
                                                      //public virtual IdentityUser User { get; set; }

        [ForeignKey("AspNetUsers")]
        public string UserId { get; set; } = "";

        public override string ToString() =>
            $"{base.ToString()} Type={Type} Alias={Alias} Status={Status} UserID={UserId}"; // doesn't account for ID

        public override bool Equals(object? obj) => obj?.ToString() == ToString();

        public override int GetHashCode() => ToString().GetHashCode();
    }
}
