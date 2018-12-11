using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eternal.Utility;
using Eternal.Models;
using Eternal.Models.ViewModels;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Eternal.Controllers
{
    public class DecksController : Controller
    {
        private int PageSize = 25;
        private readonly ILogger _logger;

        public DecksController(ILogger<DecksController> logger)
            => _logger = logger;

        // Decks/Index

        public async Task<IActionResult> Index(string searchFilter, string factionFilter, string userFilter, int page = 1)
        {
            IEnumerable<DeckIndexData> decks = await DbHelper.GetDeckIndexData();

            if (!String.IsNullOrEmpty(factionFilter))
            {
                List<string> factions = JsonConvert.DeserializeObject<List<string>>(factionFilter);

                decks = factions.Count() == 1 ?
                    decks.Where(d => d.Factions.Contains(factions.First())) : decks.Where(d => d.Factions == factionFilter);
            }

            if (!String.IsNullOrEmpty(searchFilter))
            {
                decks = decks.Where(d => d.Name.ToLower().Contains(searchFilter.ToLower()));
            }

            if (!String.IsNullOrEmpty(userFilter))
            {
                User user = await DbHelper.GetUserByUsername(userFilter);

                decks = user != null ?
                    decks.Where(d => d.UserID == user.UserID) : new List<DeckIndexData>();
            }

            return View(new DecksViewModel
            {
                Decks = decks.OrderBy(d => d.Date).Skip((page - 1) * PageSize).Take(PageSize),

                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = decks.Count()
                },

                SearchFilter = searchFilter,
                FactionFilter = factionFilter,
                UserFilter = userFilter
            });
        }


        // Decks/Details/{id}

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeckDetails deckDetails = await DbHelper.GetDeckDetails((int)id);
            deckDetails.Cards = JsonConvert.DeserializeObject<IEnumerable<DeckCard>>(deckDetails.DeckList);

            if (User.Identity.IsAuthenticated)
            {
                int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
                deckDetails.UserRating = await DbHelper.GetUserDeckRating((int)id, userId);
            }

            return View(deckDetails);
        }


        // Decks/Edit/{id}

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Deck deck = await DbHelper.GetDeck((int)id);

            if (int.Parse(HttpContext.User.Claims.ElementAt(0).Value) == deck.UserID)
            {
                return View(deck);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Name, Factions, Guide, DeckList")]Deck deck)
        {
            deck.DeckID = id;
            try
            {
                _logger.LogInformation("", $"Editting deck {id}");
                await DbHelper.EditDeck(deck);
            }
            catch (Exception)
            {
                _logger.LogWarning("", $"Edit({id}) failed.");
                ModelState.AddModelError("", "Unable to save changes.");
            }
            

            return RedirectToAction("Details", new { id });
        }


        // Decks/Create

        [Authorize]
        public IActionResult Create()
            => View();

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Name, Factions, Guide, DeckList")]Deck deck)
        {
            int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
            deck.UserID = userId;
            deck.Date = DateTime.Now.Date;
            int deckId = await DbHelper.AddDeck(deck);

            return RedirectToAction("Details", new { id = deckId });
        }


        // Decks/Delete/{id}

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            Deck deck = await DbHelper.GetDeck(id);
            int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);

            if (userId == deck.UserID)
            {
                await DbHelper.RemoveDeck(id);
                return new EmptyResult();
            }
            else
            {
                return Forbid();
            }
        }


        // Decks/LoadCollection

        public IActionResult LoadCollection(string searchFilter, string factionFilter, int? costFilter, int page = 1)
        {
            return ViewComponent("DeckBuilderCollection",
                new
                {
                    searchFilter,
                    factionFilter,
                    costFilter,
                    page
                });
        }


        // Decks/RateDeck

        [HttpPost]
        [Authorize]
        public async Task<int> RateDeck(int id)
        {
            int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
            int userRating = await DbHelper.GetUserDeckRating(id, userId);

            if (userRating == 0)
            {
                await DbHelper.AddDeckRating(id, userId);
                return 1;
            }
            else
            {
                await DbHelper.RemoveUserDeckRating(id, userId);
                return 0;
            }
        }


        // Decks/PostComment

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostComment(int id, string comment)
        {
            int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);

            DeckComment deckComment = new DeckComment
            {
                DeckID = id,
                UserID = userId,
                Comment = comment
            };

            await DbHelper.AddDeckComment(deckComment);

            return new EmptyResult();
        }


        // Decks/LoadComments

        public IActionResult LoadComments(int id)
        {
            return ViewComponent("DeckComments", new { id });
        }


        // Decks/EditComment

        [HttpPost]
        [Authorize]
        public async void EditComment(int commentId, string comment)
        {
            await DbHelper.EditDeckComment(commentId, comment);
        }


        // Decks/DeleteComment

        [HttpPost]
        [Authorize]
        public async void DeleteComment(int commentId)
        {
            await DbHelper.DeleteDeckComment(commentId);
        }


        // Decks/ReportComment

        [HttpPost]
        [Authorize]
        public async void ReportComment(int commentId)
        {
            await DbHelper.ReportDeckComment(commentId);
        }


        // Decks/RateComment

        [HttpPost]
        [Authorize]
        public async Task<int> RateComment(int commentId)
        {
            int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
            int userRating = await DbHelper.GetUserDeckCommentRating(commentId, userId);

            if (userRating == 0)
            {
                await DbHelper.AddDeckCommentRating(commentId, userId);
                return 1;
            }
            else
            {
                await DbHelper.RemoveDeckCommentRating(commentId, userId);
                return 0;
            }
        }

    }
}