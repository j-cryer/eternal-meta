using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Eternal.Models;
using Eternal.Models.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace Eternal.Utility
{
    public class DbHelper
    {
        private const string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Eternal;Trusted_Connection=True;MultipleActiveResultSets=true";

        private static IDbConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }

        // ------------- Card -------------

        // Get Card Details
        public static async Task<CardDetails> GetCardDetails(int cardId)
        {
            string sql = @"
            SELECT [Card].CardID, [Card].Name, [Card].Text, [Card].Type, [Card].Rarity, [Card].[Set], [Card].ImageUrl, Count(CardRating.CardID) as Rating
            FROM [Card]
            LEFT JOIN CardRating ON CardRating.CardID = [Card].CardID
            WHERE [Card].CardID = @cardId
            GROUP BY [Card].CardID, [Card].Name, [Card].Text, [Card].Type, [Card].Rarity, [Card].[Set], [Card].ImageUrl, CardRating.CardID;";

            CardDetails cardDetails;

            using (var conn = CreateConnection())
            {
                cardDetails = await conn.QueryFirstAsync<CardDetails>(sql, new { cardId });
            }

            return cardDetails;
        }

        // Get all Cards
        public static async Task<IEnumerable<Card>> GetAllCards()
        {
            string sql = @"
            SELECT * FROM [Card];";

            IEnumerable<Card> cards;

            using (var conn = CreateConnection())
            {
                cards = await conn.QueryAsync<Card>(sql);
            }

            return cards;
        }

        // Get Featured Cards
        public static async Task<IEnumerable<FeaturedCard>> GetFeaturedCards()
        {
            DateTime last30Days = DateTime.Today.AddDays(-30);

            string sql = @"
            SELECT TOP 6 Card.CardID, Card.ImageUrl
            FROM Card
            INNER JOIN CardRating ON Card.CardID = CardRating.CardID
            WHERE Date >= @last30Days
            GROUP BY Card.CardID, Card.ImageUrl
            ORDER BY COUNT(*) DESC;";

            IEnumerable<FeaturedCard> featuredCards;

            using (var conn = CreateConnection())
            {
                featuredCards = await conn.QueryAsync<FeaturedCard>(sql, new { last30Days });
            }

            return featuredCards;
        }



        // ------------- CardRating ---------------------------------------------------------------------

        // Add CardRating
        public static async Task AddCardRating(int cardId, int userId)
        {
            var date = DateTime.Today;

            string sql = @"
            INSERT INTO CardRating
            VALUES (@cardId, @userId, @date);";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { cardId, userId, date });
            }
        }

        // Get CardRating for CardID and UserID
        public static async Task<int> GetUserCardRating(int cardId, int userId)
        {
            string sql = @"
            SELECT Count(*) FROM CardRating
            WHERE CardID = @cardId AND UserID = @userId;";

            int userRating;

            using (var conn = CreateConnection())
            {
                userRating = await conn.QueryFirstAsync<int>(sql, new { cardId, userId });
            }

            return userRating;
        }

        // Remove CardRating
        public static async Task RemoveCardRating(int cardId, int userId)
        {
            string sql = @"
            DELETE FROM CardRating
            WHERE CardID = @cardId AND UserID = @userId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { cardId, userId });
            }
        }



        // ------------ CardComment --------------------------------------------------------------------

        // Add CardComment
        public static async Task AddCardComment(CardComment comment)
        {
            string sql = @"
            INSERT INTO CardComment (CardID, UserID, Comment, Date)
            VALUES (@CardID, @UserID, @Comment, @Date);";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, comment);
            }
        }

        // Edit CardComment
        public static async Task EditCardComment(int cardCommentId, string comment)
        {
            string sql = @"
            UPDATE CardComment
            SET Comment = @comment
            WHERE CardCommentID = @cardCommentId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { cardCommentId, comment });
            }
        }

        // Report CardComment
        public static async Task ReportCardComment(int cardCommentId)
        {
            string sql = @"
            UPDATE CardComment
            SET Reports = Reports + 1
            WHERE CardCommentID = @cardCommentId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { cardCommentId });
            }
        }

        // Remove CardComment
        public static async Task DeleteCardComment(int cardCommentId)
        {
            string sql = @"
            DELETE FROM CardComment
            WHERE CardCommentID = @cardCommentId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { cardCommentId });
            }
        }

        // Get CardComments
        public static async Task<IEnumerable<CardCommentData>> GetCardCommentData(int cardId)
        {
            string sql = @"
            SELECT CardComment.CardCommentID, CardComment.Comment, CardComment.Date, CardComment.Reports, CardComment.UserID, Count(CardCommentRating.CardCommentID) as Rating, [User].Username
            FROM CardComment
            LEFT JOIN CardCommentRating ON CardComment.CardCommentID = CardCommentRating.CardCommentID
            JOIN [User] ON CardComment.UserID = [User].UserID
            WHERE CardID = @cardId
            GROUP BY CardComment.CardCommentID, CardComment.Comment, CardComment.Date, CardComment.Reports, CardComment.UserID, CardCommentRating.CardCommentID, [User].Username;";

            IEnumerable<CardCommentData> cardComments;

            using (var conn = CreateConnection())
            {
                cardComments = await conn.QueryAsync<CardCommentData>(sql, new { cardId });
            }

            return cardComments;
        }

        // ------------ CardCommentRating ---------------------------------------------------------------

        // Add CardCommentRating
        public static async Task AddCardCommentRating(int cardCommentId, int userId)
        {
            string sql = @"
            INSERT INTO CardCommentRating
            VALUES (@cardCommentId, @userId);";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { cardCommentId, userId});
            }
        }

        // Get CardCommentRating for CommentID and UserID
        public static async Task<int> GetUserCardCommentRating(int cardCommentId, int userId)
        {
            string sql = @"
            SELECT Count(*) FROM CardCommentRating
            WHERE CardCommentID = @cardCommentId AND UserID = @userId;";

            int userRating;

            using (var conn = CreateConnection())
            {
                userRating = await conn.QueryFirstAsync<int>(sql, new { cardCommentId, userId });
            }

            return userRating;
        }

        // Remove CardCommentRating
        public static async Task RemoveCardCommentRating(int cardCommentId, int userId)
        {
            string sql = @"
            DELETE FROM CardCommentRating
            WHERE CardCommentID = @cardCommentId AND UserID = @userId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { cardCommentId, userId });
            }
        }



        // ----------- Deck --------------------------------------------------------------------------

        // Add Deck
        public static async Task<int> AddDeck(Deck deck)
        {
            string sql = @"
            INSERT INTO Deck (UserID, Name, Factions, Guide, DeckList, Date)
            VALUES (@UserID, @Name, @Factions, @Guide, @DeckList, @Date);
            SELECT SCOPE_IDENTITY();";

            int deckId;

            using (var conn = CreateConnection())
            {
                deckId = await conn.QueryFirstAsync<int>(sql, deck);
            }

            return deckId;
        }

        // Get Deck
        public static async Task<Deck> GetDeck(int deckId)
        {
            string sql = @"
            SELECT * FROM Deck
            WHERE DeckID = @deckId;";

            Deck deck;

            using (var conn = CreateConnection())
            {
                deck = await conn.QueryFirstAsync<Deck>(sql, new { deckId });
            }

            return deck;
        }

        // Get Deck Details
        public static async Task<DeckDetails> GetDeckDetails(int deckId)
        {
            string sql = @"
            SELECT Deck.DeckID, Deck.UserID, Deck.Name, Deck.Guide, Deck.DeckList, Deck.Factions, Count(DeckRating.DeckID) as Rating, [User].Username
            FROM Deck
            LEFT JOIN DeckRating on Deck.DeckID = DeckRating.DeckID
            INNER JOIN [User] on Deck.UserID = [User].UserID
            WHERE Deck.DeckID = @deckId
            GROUP BY Deck.DeckID, Deck.UserID, Deck.Name, Deck.Guide, Deck.DeckList, Deck.Factions, DeckRating.DeckID, [User].Username;";

            DeckDetails deckDetails;

            using (var conn = CreateConnection())
            {
                deckDetails = await conn.QueryFirstAsync<DeckDetails>(sql, new { deckId });
            }

            return deckDetails;
        }

        // Get all Decks for Decks/Index
        public static async Task<IEnumerable<DeckIndexData>> GetDeckIndexData()
        {
            string sql = @"
            SELECT Deck.DeckID, Deck.UserID, Deck.Name, Deck.Date, Deck.Factions, [User].Username, Count(DeckRating.DeckID) as Rating
            FROM Deck
            INNER JOIN [User] ON Deck.UserID = [User].UserID
			LEFT JOIN DeckRating ON Deck.DeckID = DeckRating.DeckID
			GROUP BY Deck.DeckID, Deck.UserID, Deck.Name, Deck.Date, Deck.Factions, [User].Username, DeckRating.DeckID;";

            IEnumerable<DeckIndexData> decks;

            using (var conn = CreateConnection())
            {
                decks = await conn.QueryAsync<DeckIndexData>(sql);
            }

            return decks;
        }

        // Get all Decks by User
        public static async Task<IEnumerable<Deck>> GetUserDecks(int userId)
        {
            string sql = @"
            SELECT * FROM Deck
            WHERE UserID = @userId;";

            IEnumerable<Deck> Decks;

            using (var conn = CreateConnection())
            {
                Decks = await conn.QueryAsync<Deck>(sql, new { userId });
            }

            return Decks;
        }

        // UPDATE 
        public static async Task EditDeck(Deck deck)
        {
            string sql = @"
            UPDATE Deck
            SET Name = @Name, Factions = @Factions, Guide = @Guide, DeckList = @DeckList
            WHERE DeckID = @DeckID;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, deck);
            }
        }

        // DELETE
        public static async Task RemoveDeck(int deckId)
        {
            string sql = @"
            DELETE FROM Deck
            WHERE DeckID = @deckId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { deckId });
            }
        }

        // Get Featured Decks
        public static async Task<IEnumerable<FeaturedDeck>> GetFeaturedDecks()
        {
            var last30Days = DateTime.Today.AddDays(-30);

            string sql = @"
            SELECT TOP 6 Deck.DeckID, Deck.UserID, Deck.Name, Deck.Factions, [User].Username
            FROM Deck
            INNER JOIN DeckRating ON Deck.DeckID = DeckRating.DeckID
            INNER JOIN [User] ON Deck.UserID = [User].UserID
            WHERE DeckRating.Date >= @last30Days
            GROUP BY Deck.DeckID, Deck.UserID, Deck.Name, Deck.Factions, [User].Username
            ORDER BY Count(*) DESC;";

            IEnumerable<FeaturedDeck> featuredDecks;

            using (var conn = CreateConnection())
            {
                featuredDecks = await conn.QueryAsync<FeaturedDeck>(sql, new { last30Days });
            }

            return featuredDecks;
        }



        // ------------ DeckRating ----------------------------------------------------------------------

        // Add DeckRating
        public static async Task AddDeckRating(int deckId, int userId)
        {
            var date = DateTime.Today;

            string sql = @"
            INSERT INTO DeckRating
            VALUES (@deckId, @userId, @date);";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { deckId, userId, date });
            }
        }

        // Get DeckRating
        public static async Task<int> GetUserDeckRating(int deckId, int userId)
        {
            string sql = @"
            SELECT Count(DeckID) FROM DeckRating
            WHERE DeckID = @deckId AND UserID = @userId;";

            int userRating;

            using (var conn = CreateConnection())
            {
                userRating = await conn.QueryFirstAsync<int>(sql, new { deckId, userId });
            }

            return userRating;
        }

        // DELETE
        public static async Task RemoveUserDeckRating(int deckId, int userId)
        {
            string sql = @"
            DELETE FROM DeckRating
            WHERE DeckID = @deckId AND UserID = @userId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { deckId, userId });
            }
        }



        // ----------- DeckComment --------------------------------------------------------------------

        // Add DeckComment
        public static async Task AddDeckComment(DeckComment comment)
        {
            string sql = @"
            INSERT INTO DeckComment (DeckID, UserID, Comment)
            VALUES (@DeckID, @UserID, @Comment);";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, comment);
            }
        }

        // Get DeckComments
        public static async Task<IEnumerable<DeckCommentData>> GetDeckCommentData(int deckId)
        {
            string sql = @"
            SELECT DeckComment.DeckCommentID, DeckComment.Comment, DeckComment.Date, DeckComment.Reports, DeckComment.UserID, Count(DeckCommentRating.DeckCommentID) as Rating, [User].Username
            FROM DeckComment
            LEFT JOIN DeckCommentRating ON DeckComment.DeckCommentID = DeckCommentRating.DeckCommentID
            JOIN [User] ON DeckComment.UserID = [User].UserID
            WHERE DeckID = @deckId
            GROUP BY DeckComment.DeckCommentID, DeckComment.Comment, DeckComment.Date, DeckComment.Reports, DeckComment.UserID, DeckCommentRating.DeckCommentID, [User].Username;";

            IEnumerable<DeckCommentData> deckComments;

            using (var conn = CreateConnection())
            {
                deckComments = await conn.QueryAsync<DeckCommentData>(sql, new { deckId });
            }

            return deckComments;
        }

        // Edit DeckComment
        public static async Task EditDeckComment(int deckCommentId, string comment)
        {
            string sql = @"
            UPDATE DeckComment
            SET Comment = @comment
            WHERE DeckCommentID = @deckCommentId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { deckCommentId, comment });
            }
        }

        // Report DeckComment
        public static async Task ReportDeckComment(int deckCommentId)
        {
            string sql = @"
            UPDATE DeckComment
            SET Reports = Reports + 1
            WHERE DeckCommentID = @deckCommentId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { deckCommentId });
            }
        }

        // Remove DeckComment
        public static async Task DeleteDeckComment(int deckCommentId)
        {
            string sql = @"
            DELETE FROM DeckComment
            WHERE DeckCommentID = @deckCommentId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { deckCommentId });
            }
        }



        // ----------- DeckCommentRating ----------------------------------------------------------------

        // Add DeckCommentRating
        public static async Task AddDeckCommentRating(int deckCommentId, int userId)
        {
            string sql = @"
            INSERT INTO DeckCommentRating
            VALUES (@deckCommentId, @userId);";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { deckCommentId, userId });
            }
        }

        // Get DeckCommentRating 
        public static async Task<int> GetUserDeckCommentRating(int deckCommentId, int userId)
        {
            string sql = @"
            SELECT Count(DeckCommentID) FROM DeckCommentRating
            WHERE DeckCommentID = @deckCommentId AND UserID = @userId;";

            int userRating;

            using (var conn = CreateConnection())
            {
                userRating = await conn.QueryFirstAsync<int>(sql, new { deckCommentId, userId });
            }

            return userRating;
        }

        // Remove DeckCommentRating
        public static async Task RemoveDeckCommentRating(int deckCommentId, int userId)
        {
            string sql = @"
            DELETE FROM DeckCommentRating
            WHERE DeckCommentID = @deckCommentId AND UserID = @userId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { deckCommentId, userId });
            }
        }



        // -------------- User ------------------------------------------------------------------------

        // Add User
        public static async Task<User> AddUser(User registrant)
        {
            string sql = @"
            INSERT INTO [User] (Email, Username, Password, Token)
            VALUES (@Email, @Username, @Password, @Token);
            SELECT * FROM [User] WHERE Email = @Email;";

            User user;

            using (var conn = CreateConnection())
            {
                user = await conn.QueryFirstAsync<User>(sql, registrant);
            }

            return user;
        }

        // Get User by Email
        public static async Task<User> GetUserByEmail(string email)
        {
            string sql = @"
            SELECT * FROM [User]
            WHERE Email = @email;";

            User user;

            using (var conn = CreateConnection())
            {
                user = await conn.QueryFirstAsync<User>(sql, new { email });
            }

            return user;
        }

        // Get User by Username
        public static async Task<User> GetUserByUsername(string username)
        {
            string sql = @"
            SELECT * FROM [User]
            WHERE EXISTS(SELECT * FROM [User] WHERE Username = @username)
            AND Username = @username;";

            IEnumerable<User> user;

            using (var conn = CreateConnection())
            {
                user = await conn.QueryAsync<User>(sql, new { username });
            }

            if (user.Count() == 0)
            {
                return null;
            }
            else
            {
                return user.First();
            }
        }

        // Get User by UserID
        public static async Task<User> GetUser(int userId)
        {
            string sql = @"
            SELECT * FROM [User]
            WHERE UserID = @userId;";

            User user;

            using (var conn = CreateConnection())
            {
                user = await conn.QueryFirstAsync<User>(sql, new { userId });
            }

            return user;
        }

        // Activate User
        public static async Task ActivateUser(int userId)
        {
            string sql = @"
            UPDATE [User]
            SET Active = 1
            WHERE UserID = @userId;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, new { userId });
            }
        }

        // Change Password
        public static async Task ChangePassword(User user)
        {
            string sql = @"
            UPDATE [User]
            SET Password = @Password
            WHERE UserID = @UserID;";

            using (var conn = CreateConnection())
            {
                await conn.ExecuteAsync(sql, user);
            }
        }

        // Check if Email exists
        public static async Task<bool> EmailExists(string email)
        {
            string sql = @"
            SELECT COUNT(*) FROM [User]
            WHERE Email = @email;";

            bool exists;

            using (var conn = CreateConnection())
            {
                exists = await conn.QueryFirstAsync<bool>(sql, new { email });
            }

            return exists;
        }

        // Check if Username exists
        public static async Task<bool> UsernameExists(string username)
        {
            string sql = @"
            SELECT COUNT(*) FROM [User]
            WHERE Username = @username;";

            bool exists;

            using (var conn = CreateConnection())
            {
                exists = await conn.QueryFirstAsync<bool>(sql, new { username });
            }

            return exists;
        }
    }
}