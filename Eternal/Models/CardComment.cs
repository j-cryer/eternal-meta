using System;
using System.ComponentModel.DataAnnotations;

namespace Eternal.Models
{
    public class CardComment
    {
        public int CardCommentID { get; set; }
        public int CardID { get; set; }
        public int UserID { get; set; }
        [Required]
        [StringLength(300)]
        public string Comment { get; set; }
        public DateTime Date { get; set; }
    }
}
