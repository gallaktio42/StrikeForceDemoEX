using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoExam.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Column("FIO")]
        public string FullName { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        [Column("Password")]
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        public Role Role { get; set; }
    }
}
