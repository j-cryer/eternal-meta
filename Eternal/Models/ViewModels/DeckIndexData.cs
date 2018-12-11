using System;

namespace Eternal.Models.ViewModels
{
    public class DeckIndexData
    {
        public int DeckID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Factions { get; set; }
        public DateTime Date { get; set; }
        public string Username { get; set; }
        public int Rating { get; set; }
    }
}