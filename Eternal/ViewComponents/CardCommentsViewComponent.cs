using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Eternal.Models.ViewModels;
using Eternal.Utility;
using System.Collections.Generic;

namespace Eternal.ViewComponents
{
    public class CardCommentsViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int id)
        {
            IEnumerable<CardCommentData> comments = await DbHelper.GetCardCommentData(id);

            if (User.Identity.IsAuthenticated)
            {
                int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
                foreach (var comment in comments)
                {
                    comment.UserRating = await DbHelper.GetUserCardCommentRating(comment.CardCommentID, userId);
                }
            }

            return View(comments);
        }
    }
}