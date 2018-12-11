using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eternal.Utility;
using Eternal.Models;
using Eternal.Models.ViewModels;
using Newtonsoft.Json;

namespace Eternal.ViewComponents
{
    public class DeckBuilderCollectionViewComponent : ViewComponent
    {
        public int PageSize = 8;

        public async Task<IViewComponentResult> InvokeAsync(string searchFilter, string factionFilter, int? costFilter, int page = 1)
        {
            var cards = await DbHelper.GetAllCards();

            if (!String.IsNullOrEmpty(factionFilter))
            {
                var tempCards = new List<Card>();
                var factionList = new List<String> { "Fire", "Time", "Justice", "Primal", "Shadow" };
                var factions = JsonConvert.DeserializeObject<List<string>>(factionFilter);

                if (factions.Count() == 1)
                {
                    if (factions.First() == "Multifaction")
                    {
                        cards = cards.Where(c => c.Factions.Length >= 15);
                    }
                    else
                    {
                        cards = cards.Where(c => c.Factions.Contains(factions.First()) || c.Factions.Contains("Factionless"));
                    }
                }
                else
                {
                    if (factions.Contains("Multifaction"))
                    {
                        foreach (var faction in factions)
                        {
                            if (faction != "Multifaction")
                            {
                                tempCards.AddRange(cards.Where(c => c.Factions.Length >= 15 && c.Factions.Contains(faction) && !tempCards.Contains(c)));
                            }
                        }
                        if (factions.Count() >= 3)
                        {
                            foreach (var faction in factionList)
                            {
                                if (!factions.Contains(faction))
                                {
                                    tempCards.RemoveAll(c => c.Factions.Contains(faction));
                                }
                            }
                        }
                    }
                    else
                    {
                        tempCards = cards.ToList();
                        foreach (var faction in factionList)
                        {
                            if (!factions.Contains(faction))
                            {
                                tempCards.RemoveAll(c => c.Factions.Contains(faction));
                            }
                        }
                        tempCards.AddRange(cards.Where(c => c.Factions == "Factionless"));
                    }
                    cards = tempCards.OrderBy(c => c.CardID);
                }
            }

            if (!String.IsNullOrEmpty(searchFilter))
            {
                cards = cards.Where(c =>
                    c.Name.ToLower().Contains(searchFilter.ToLower()) || c.Type.ToLower().Contains(searchFilter.ToLower()) ||
                    c.Text.ToLower().Contains(searchFilter.ToLower()) || c.Rarity.ToLower() == searchFilter.ToLower());
            }

            if (!(costFilter == null))
            {
                if (costFilter == 7)
                {
                    cards = cards.Where(c => c.Cost >= costFilter);
                }
                else
                {
                    cards = cards.Where(c => c.Cost == costFilter);
                }
            }

            return View(new CardIndexData
            {
                Cards = cards.OrderBy(c => c.CardID).Skip((page - 1) * PageSize).Take(PageSize),

                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = cards.Count()
                },

                SearchFilter = searchFilter,
                FactionFilter = factionFilter,
                CostFilter = costFilter
            });
        }
    }
}
