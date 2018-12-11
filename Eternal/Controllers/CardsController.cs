using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eternal.Utility;
using Eternal.Models;
using Eternal.Models.ViewModels;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Eternal.Controllers
{
    public class CardsController : Controller
    {
        public int PageSize = 24;

        public async Task<IActionResult> Index(string searchFilter, string factionFilter, int? costFilter, int page = 1)
        {
            IEnumerable<Card> cards = await DbHelper.GetAllCards();

            if (!String.IsNullOrEmpty(factionFilter))
            {
                List<Card> tempCards = new List<Card>();
                List<string> factionList = new List<String> { "Fire", "Time", "Justice", "Primal", "Shadow" };
                List<string> factions = JsonConvert.DeserializeObject<List<string>>(factionFilter);

                if (factions.Count() == 1)
                {
                    cards = (factions.First() == "Multifaction") ? cards.Where(c => c.Factions.Length >= 15) : cards.Where(c => c.Factions.Contains(factions.First()) || c.Factions.Contains("Factionless"));
                    /*if (factions.First() == "Multifaction")
                    {
                        cards = cards.Where(c => c.Factions.Length >= 15);
                    }
                    else
                    {
                        cards = cards.Where(c => c.Factions.Contains(factions.First()) || c.Factions.Contains("Factionless"));
                    }*/
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

            if (costFilter != null)
            {
                cards = costFilter == 7 ? cards.Where(c => c.Cost >= costFilter) : cards.Where(c => c.Cost == costFilter);
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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CardDetails cardDetails = await DbHelper.GetCardDetails((int)id);

            IEnumerable<Card> allCards = await DbHelper.GetAllCards();
            cardDetails.RelatedCards = allCards.Where(c => c.Text.Contains(cardDetails.Name) || cardDetails.Text.Contains(c.Name));

            // cardDetails.RelatedCards = DbHelper.GetCards().Result.Where(c => c.Text.Contains(cardDetails.Name) || cardDetails.Text.Contains(c.Name));

            if (User.Identity.IsAuthenticated)
            {
                int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
                cardDetails.UserRating = await DbHelper.GetUserCardRating((int)id, userId);
            }

            return View(cardDetails);
        }

        [HttpPost]
        [Authorize]
        public async Task<int> RateCard(int id)
        {
            int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
            int userRating = await DbHelper.GetUserCardRating(id, userId);

            if (userRating == 0)
            {
                await DbHelper.AddCardRating(id, userId);
                return 1;
            }
            else
            {
                await DbHelper.RemoveCardRating(id, userId);
                return 0;
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostComment(int id, string comment)
        {
            int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);

            CardComment cardComment = new CardComment
            {
                CardID = id,
                UserID = userId,
                Comment = comment,
                Date = DateTime.Now
            };

            await DbHelper.AddCardComment(cardComment);

            return new EmptyResult();
        }

        public IActionResult LoadComments(int id)
        {
            return ViewComponent("CardComments", new { id });
        }

        [HttpPost]
        [Authorize]
        public async void EditComment(int commentId, string comment)
        {
            await DbHelper.EditCardComment(commentId, comment);
        }

        [HttpPost]
        [Authorize]
        public async void DeleteComment(int commentId)
        {
            await DbHelper.DeleteCardComment(commentId);
        }

        [HttpPost]
        [Authorize]
        public async void ReportComment(int commentId)
        {
            await DbHelper.ReportCardComment(commentId);
        }

        [HttpPost]
        [Authorize]
        public async Task<int> RateComment(int commentId)
        {
            int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
            int userRating = await DbHelper.GetUserCardCommentRating(commentId, userId);

            if (userRating == 0)
            {
                await DbHelper.AddCardCommentRating(commentId, userId);
                return 1;
            }
            else
            {
                await DbHelper.RemoveCardCommentRating(commentId, userId);
                return 0;
            }
        }
    }
}