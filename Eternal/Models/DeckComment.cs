using System;
using System.ComponentModel.DataAnnotations;

namespace Eternal.Models
{
    public class DeckComment
    {
        public int DeckCommentID { get; set; }
        public int DeckID { get; set; }
        public int UserID { get; set; }
        [Required]
        [StringLength(300)]
        public string Comment { get; set; }
        public DateTime Date { get; set; }
    }
}
