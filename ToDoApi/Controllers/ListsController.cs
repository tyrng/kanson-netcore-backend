using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Models;
using ToDoApi.Helpers;

namespace ToDoApi.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ListsController : ControllerBase
    {
        private readonly TrelloKeepContext _context;

        public ListsController(TrelloKeepContext context)
        {
            _context = context;
        }

        // GET: api/Lists
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListsDTO>>> GetLists()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user != null)
            {
                var lists = user.Boards.SelectMany(o => o.Lists).ToList();
                if (lists.Count == 0)
                {
                    return NoContent();
                }
                return lists.Select(x => ItemToDTO(x)).ToList();
            }
            return BadRequest();
        }

        // GET: api/Lists/ofBoard/5
        [HttpGet("ofBoard/{id}")]
        public async Task<ActionResult<IEnumerable<ListsDTO>>> GetListsByBoardId(string id)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user != null)
            {
                var lists = user.Boards.SelectMany(o => o.Lists).Where(o => o.BoardId == id).ToList();
                return lists.Select(x => ItemToDTO(x)).ToList();
            }
            return BadRequest();
        }

        // GET: api/Lists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ListsDTO>> GetLists(string id)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user != null)
            {
                var list = user.Boards.SelectMany(o => o.Lists).FirstOrDefault(b => b.Id == id);
                if (list == null)
                {
                    return NotFound();
                }
                return ItemToDTO(list);
            }
            return BadRequest();
        }

        // PUT: api/Lists/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLists(string id, Lists lists)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            var board = user.Boards.FirstOrDefault(b => b.Id == lists.BoardId);

            if (user != null && board != null)
            {
                if (id != lists.Id && board.UserId != user.Id)
                {
                    return BadRequest();
                }
                var allLists = user.Boards.SelectMany(o => o.Lists);
                var oldList = allLists.FirstOrDefault(o => o.Id == id);
                var oldBoardLists = allLists.Where(i => i.BoardId == oldList.BoardId);
                var oldListIndex = oldList.Index;

                foreach (Lists bl in oldBoardLists)
                {
                    if (bl.Index > oldListIndex)
                    {
                        bl.Index--;
                        _context.Entry(bl).State = EntityState.Modified;
                    }
                }
                var thisList = allLists.FirstOrDefault(o => o.Id == lists.Id);
                var newBoardLists = allLists.Where(i => i.BoardId == lists.BoardId);

                thisList.Index = newBoardLists.Count();
                thisList.BoardId = lists.BoardId;
                thisList.Title = lists.Title;

                _context.Entry(thisList).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListsExists(id))
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

        // POST: api/Lists
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<ListsDTO>> PostLists(Lists lists)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user == null)
            {
                return BadRequest();
            }

            var board = user.Boards.FirstOrDefault(b => b.Id == lists.BoardId);

            if (board != null && board.UserId == user.Id)
            {
                _context.Lists.Add(lists);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (ListsExists(lists.Id))
                    {
                        return Conflict();
                    }
                    else
                    {
                        throw;
                    }
                }

                return CreatedAtAction("GetLists", new { id = lists.Id }, ItemToDTO(lists));
            }
            return BadRequest();
        }

        // DELETE: api/Lists/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ListsDTO>> DeleteLists(string id)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user == null)
            {
                return BadRequest();
            }
            var list = user.Boards.SelectMany(o => o.Lists).FirstOrDefault(b => b.Id == id);
            var boardLists = user.Boards.SelectMany(o => o.Lists).Where(l => l.BoardId == list.BoardId).ToList();

            if (list == null)
            {
                return BadRequest();
            }
            var board = user.Boards.FirstOrDefault(b => b.Id == list.BoardId);

            if (board != null && board.UserId == user.Id)
            {
                var listIndex = list.Index;

                foreach (Lists bl in boardLists)
                {
                    if (bl.Index > listIndex)
                    {
                        bl.Index--;
                        _context.Entry(bl).State = EntityState.Modified;
                    }
                }

                _context.Lists.Remove(list);
                await _context.SaveChangesAsync();

                return ItemToDTO(list);
            }
            return BadRequest();
        }

        // POST: api/Boards/sort
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("sort")]
        public async Task<ActionResult<ListsDTO>> SortLists(Dictionary<string, int> listIndices) // { "id": index }
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authenticateInfo);
            string accessToken = authenticateInfo.ToString().Substring("Bearer ".Length);
            var user = await _context.Users.Include(o => o.Boards)
                .ThenInclude(o => o.Lists)
                .FirstOrDefaultAsync(x => x.Token == accessToken);

            if (user == null)
            {
                return BadRequest();
            }

            var lists = user.Boards.SelectMany(o => o.Lists).ToList();

            if (lists.Count == 0)
            {
                return BadRequest();
            }

            foreach (Lists list in lists)
            {
                foreach (KeyValuePair<string, int> listIndex in listIndices)
                {
                    if (list.Id == listIndex.Key)
                    {
                        list.Index = listIndex.Value;
                        _context.Entry(list).State = EntityState.Modified;
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

        private bool ListsExists(string id)
        {
            return _context.Lists.Any(e => e.Id == id);
        }
        public static ListsDTO ItemToDTO(Lists lists) =>
        new ListsDTO
        {
            Id = lists.Id,
            BoardId = lists.BoardId,
            Index = lists.Index,
            Title = lists.Title
        };
    }
}
