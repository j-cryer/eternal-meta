using System;

namespace Eternal.Models.ViewModels
{
    public class DeckCommentData
    {
        public int DeckCommentID { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public int Reports { get; set; }
        public int Rating { get; set; }
        public int UserRating { get; set; }
    }
}