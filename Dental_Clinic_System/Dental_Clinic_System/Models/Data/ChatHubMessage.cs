using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dental_Clinic_System.Models.Data
{
    [Table("ChatHubMessage")]
    public class ChatHubMessage
    {
        [Key]
        public int ID { get; set; }

        [Column("Content", TypeName = "nvarchar(max)")]
        public string Content { get; set; } = null!;

        [Column("Timestamp")]
        public DateTime Timestamp { get; set; }

        [ForeignKey("Sender")]
        public int SenderId { get; set; }
        public virtual Account Sender { get; set; } = null!;

        [ForeignKey("Receiver")]
        public int ReceiverId { get; set; }
        public virtual Account Receiver { get; set; } = null!;
    }
}
