using System.Collections.Generic;

namespace Eternal.Models.ViewModels
{
    public class DeckDetails
    {
        public int DeckID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Guide { get; set; }
        public string DeckList { get; set; }
        public string Factions { get; set; }
        public int Rating { get; set; }
        public string Username { get; set; }
        public int UserRating { get; set; }
        public IEnumerable<DeckCard> Cards { get; set; }
    }

    public class DeckCard
    {
        public int CardID { get; set; }
        public string Name { get; set; }
        public string Rarity { get; set; }
        public int Count { get; set; }
    }
}
