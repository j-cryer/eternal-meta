using System.Collections.Generic;

namespace Eternal.Models.ViewModels
{
    public class DecksViewModel
    {
        public IEnumerable<DeckIndexData> Decks { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public string SearchFilter { get; set; }
        public string FactionFilter { get; set; }
        public string UserFilter { get; set; }
    }
}