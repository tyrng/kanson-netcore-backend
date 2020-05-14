using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KansonNetCoreBackend.Models;
using Microsoft.AspNetCore.Authentication;
using System.Diagnostics;

namespace KansonNetCoreBackend.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class BoardsController : ControllerBase
    {
        private readonly TrelloKeepContext _context;

        public BoardsController(TrelloKeepContext context)
        {
            _context = context;
        }

        // GET: api/Boards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardsDTO>>> GetBoards()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards).FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user != null)
            {
                var boards = user.Boards.ToList();
                if (boards.Count == 0)
                {
                    return NoContent();
                }
                return boards.Select(x => ItemToDTO(x)).ToList();
            }
            return BadRequest();
        }

        // GET: api/Boards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BoardsDTO>> GetBoards(string id)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards).FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user != null)
            {
                var board = user.Boards.FirstOrDefault(b => b.Id == id);
                if (board == null)
                {
                    return NotFound();
                }
                return ItemToDTO(board);
            }
            return BadRequest();
        }

        // PUT: api/Boards/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBoards(string id, Boards boards)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user != null)
            {
                if (id != boards.Id && boards.UserId != user.Id)
                {
                    return BadRequest();
                }

                _context.Entry(boards).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoardsExists(id))
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

        // POST: api/Boards
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<BoardsDTO>> PostBoards(Boards boards)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user != null && boards.UserId == user.Id)
            {
                _context.Boards.Add(boards);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (BoardsExists(boards.Id))
                    {
                        return Conflict();
                    }
                    else
                    {
                        throw;
                    }
                }

                return CreatedAtAction("GetBoards", new { id = boards.Id }, ItemToDTO(boards));
            }
            return BadRequest();
        }

        // DELETE: api/Boards/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<BoardsDTO>> DeleteBoards(string id)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards).FirstOrDefaultAsync(x => x.Token == accessToken);
            var boards = user.Boards.ToList();
            var board = user.Boards.FirstOrDefault(b => b.Id == id);

            if (user != null && board.UserId == user.Id)
            {
                if (board == null)
                {
                    return NotFound();
                }

                var boardIndex = board.Index;

                foreach(Boards b in boards)
                {
                    if(b.Index > boardIndex)
                    {
                        b.Index--;
                        _context.Entry(b).State = EntityState.Modified;
                    }
                }

                _context.Boards.Remove(board);
                await _context.SaveChangesAsync();

                return ItemToDTO(board);
            }
            return BadRequest();
        }

        // POST: api/Boards/sort
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("sort")]
        public async Task<ActionResult<BoardsDTO>> SortBoards(Dictionary<string, int> boardIndices) // { "id": index }
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards).FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user == null)
            {
                return BadRequest();
            }

            var boards = user.Boards.ToList();

            if (boards.Count == 0)
            {
                return BadRequest();
            }

            foreach(Boards board in boards)
            {
                foreach(KeyValuePair<string, int> boardIndex in boardIndices)
                {
                    if(board.Id == boardIndex.Key)
                    {
                        board.Index = boardIndex.Value;
                        _context.Entry(board).State = EntityState.Modified;
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

        private bool BoardsExists(string id)
        {
            return _context.Boards.Any(e => e.Id == id);
        }
        public static BoardsDTO ItemToDTO(Boards boards) =>
        new BoardsDTO
        {
            Id = boards.Id,
            UserId = boards.UserId,
            Index = boards.Index,
            Title = boards.Title
        };
    }
}
