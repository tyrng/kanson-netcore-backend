using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KansonBackendNetCore.Helpers;
using KansonBackendNetCore.Models;

namespace KansonBackendNetCore.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly TrelloKeepContext _context;

        public CardsController(TrelloKeepContext context)
        {
            _context = context;
        }

        // GET: api/Cards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CardsDTO>>> GetCards()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .ThenInclude(o => o.Cards)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user != null)
            {
                var cards = user.Boards.SelectMany(o => o.Lists).SelectMany(o => o.Cards).ToList();
                if (cards.Count == 0)
                {
                    return NoContent();
                }
                return cards.Select(x => ItemToDTO(x)).ToList();
            }
            return BadRequest();
        }

        // GET: api/Cards/ofList/5
        [HttpGet("ofList/{id}")]
        public async Task<ActionResult<IEnumerable<CardsDTO>>> GetCardsByListId(string id)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .ThenInclude(o => o.Cards)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user != null)
            {
                var cards = user.Boards.SelectMany(o => o.Lists).SelectMany(o => o.Cards).Where(o => o.ListId == id).ToList();
                return cards.Select(x => ItemToDTO(x)).ToList();
            }
            return BadRequest();
        }

        // GET: api/Cards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CardsDTO>> GetCards(string id)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .ThenInclude(o => o.Cards)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user != null)
            {
                var card = user.Boards.SelectMany(o => o.Lists).SelectMany(o => o.Cards).FirstOrDefault(c => c.Id == id);
                if (card == null)
                {
                    return NotFound();
                }
                return ItemToDTO(card);
            }
            return BadRequest();
        }

        // PUT: api/Cards/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCards(string id, Cards cards)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if(user == null)
            {
                return BadRequest();
            }

            var list = user.Boards.SelectMany(o => o.Lists).FirstOrDefault(o => o.Id == cards.ListId);
            if(list == null)
            {
                return BadRequest();
            }
            var board = user.Boards.FirstOrDefault(b => b.Id == list.BoardId);

            if (board != null)
            {
                if (id != cards.Id && board.UserId != user.Id)
                {
                    return BadRequest();
                }

                _context.Entry(cards).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CardsExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return NoContent();
            }
            return BadRequest();
        }

        // POST: api/Cards
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<CardsDTO>> PostCards(Cards cards)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if(user == null)
            {
                return BadRequest();
            }

            var list = user.Boards.SelectMany(o => o.Lists).FirstOrDefault(o => o.Id == cards.ListId);

            if(list == null)
            {
                return BadRequest();
            }

            var board = user.Boards.FirstOrDefault(b => b.Id == list.BoardId);

            if (board != null && board.UserId == user.Id)
            {
                _context.Cards.Add(cards);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (CardsExists(cards.Id))
                    {
                        return Conflict();
                    }
                    else
                    {
                        throw;
                    }
                }

                return CreatedAtAction("GetCards", new { id = cards.Id }, ItemToDTO(cards));
            }
            return BadRequest();
        }

        // DELETE: api/Cards/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CardsDTO>> DeleteCards(string id)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .ThenInclude(o => o.Cards)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user == null)
            {
                return BadRequest();
            }

            var card = user.Boards.SelectMany(i => i.Lists).SelectMany(o => o.Cards).FirstOrDefault(o => o.Id == id);
            var listCards = user.Boards.SelectMany(i => i.Lists).SelectMany(o => o.Cards).Where(i => i.ListId == card.ListId);

            if (card == null)
            {
                return BadRequest();
            }

            var list = user.Boards.SelectMany(o => o.Lists).FirstOrDefault(b => b.Id == card.ListId);

            if (list == null)
            {
                return BadRequest();
            }
            var board = user.Boards.FirstOrDefault(b => b.Id == list.BoardId);

            if (board != null && board.UserId == user.Id)
            {
                var cardIndex = card.Index;

                foreach (Cards lc in listCards)
                {
                    if (lc.Index > cardIndex)
                    {
                        lc.Index--;
                        _context.Entry(lc).State = EntityState.Modified;
                    }
                }

                _context.Cards.Remove(card);
                await _context.SaveChangesAsync();

                return ItemToDTO(card);
            }
            return BadRequest();

        }

        // POST: api/Boards/sort
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("sort")]
        public async Task<ActionResult<CardsDTO>> SortBoards(Dictionary<string, ChildIndexer> cardIndices) // { "id": { "parentId": "listId", "index": index } }
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .ThenInclude(o => o.Cards)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user == null)
            {
                return BadRequest();
            }

            var cards = user.Boards.SelectMany(o => o.Lists).SelectMany(o => o.Cards).ToList();

            if (cards.Count == 0)
            {
                return BadRequest();
            }

            foreach (Cards card in cards)
            {
                foreach (KeyValuePair<string, ChildIndexer> cardIndex in cardIndices)
                {
                    if (card.Id == cardIndex.Key)
                    {
                        card.ListId = cardIndex.Value.ParentId;
                        card.Index = cardIndex.Value.Index;
                        _context.Entry(card).State = EntityState.Modified;
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return NoContent();
        }

        private bool CardsExists(string id)
        {
            return _context.Cards.Any(e => e.Id == id);
        }
        public static CardsDTO ItemToDTO(Cards cards) =>
        new CardsDTO
        {
            Id = cards.Id,
            ListId = cards.ListId,
            Index = cards.Index,
            Text = cards.Text
        };
    }
}
