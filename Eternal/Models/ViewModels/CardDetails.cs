using System.Collections.Generic;

namespace Eternal.Models.ViewModels
{
    public class CardDetails
    {
        public int CardID { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public string Set { get; set; }
        public string ImageUrl { get; set; }
        public int Rating { get; set; }
        public int UserRating { get; set; }
        public IEnumerable<Card> RelatedCards { get; set; }
    }
}
