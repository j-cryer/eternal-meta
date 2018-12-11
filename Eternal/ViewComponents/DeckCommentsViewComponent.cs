using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eternal.Utility;
using Eternal.Models.ViewModels;

namespace Eternal.ViewComponents
{
    public class DeckCommentsViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int id)
        {
            IEnumerable<DeckCommentData> comments = await DbHelper.GetDeckCommentData(id);

            if (User.Identity.IsAuthenticated)
            {
                int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
                foreach (var comment in comments)
                {
                    comment.UserRating = await DbHelper.GetUserDeckCommentRating(comment.DeckCommentID, userId);
                }
            }

            return View(comments);
        }
    }
}