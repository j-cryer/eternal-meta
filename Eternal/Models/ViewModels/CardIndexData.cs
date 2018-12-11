using System.Collections.Generic;

namespace Eternal.Models.ViewModels
{
    public class CardIndexData
    {
        public IEnumerable<Card> Cards { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public string SearchFilter { get; set; }
        public string FactionFilter { get; set; }
        public int? CostFilter { get; set; }
    }
}
