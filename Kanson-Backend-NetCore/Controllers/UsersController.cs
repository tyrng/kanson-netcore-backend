using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using KansonBackendNetCore.Helpers;
using KansonBackendNetCore.Models;

namespace KansonBackendNetCore.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TrelloKeepContext _context;
        private readonly AppSettings _appSettings;

        public UsersController(TrelloKeepContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsersDTO>>> GetUsers()
        {
            return await _context.Users.Select(x => ItemToDTO(x)).ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UsersDTO>> GetUsers(string id)
        {
            var users = await _context.Users.FindAsync(id);

            if (users == null)
            {
                return NotFound();
            }

            return ItemToDTO(users);
        }

        // GET: api/Users/boards/5
        [HttpGet("boards/{id}")]
        public async Task<ActionResult<IEnumerable<BoardsDTO>>> GetUsersBoards(string id)
        {
            // lazy loading (enable in Startup configuration)
            //var users = await _context.Users.FindAsync(id);
            // eager loading
            var users = await _context.Users.Include(o => o.Boards).FirstOrDefaultAsync(o => o.Id == id);

            if (users == null)
            {
                return NotFound();
            }

            return users.Boards.Select(o => BoardsController.ItemToDTO(o)).ToList();
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(string id, Users users)
        {
            if (id != users.Id)
            {
                return BadRequest();
            }

            users.Token = AuthController.GenerateNewToken(users.Id, _appSettings.Secret);

            _context.Entry(users).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
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

        // POST: api/Users
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<UsersDTO>> PostUsers(Users users)
        {
            _context.Users.Add(users);
            users.Token = AuthController.GenerateNewToken(users.Id, _appSettings.Secret);
            Boards defaultBoard = new Boards 
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Default Board",
                Index = 0,
                UserId = users.Id
            };
            _context.Boards.Add(defaultBoard);
            Lists toDoList = new Lists 
            {
                Id = Guid.NewGuid().ToString(),
                Title = "To do",
                Index = 0,
                BoardId = defaultBoard.Id
            };
            _context.Lists.Add(toDoList);
            Lists inProgressList = new Lists
            {
                Id = Guid.NewGuid().ToString(),
                Title = "In progress",
                Index = 1,
                BoardId = defaultBoard.Id
            };
            _context.Lists.Add(inProgressList);
            Lists doneList = new Lists
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Done",
                Index = 2,
                BoardId = defaultBoard.Id
            };
            _context.Lists.Add(doneList);
            Cards firstCard = new Cards
            {
                Id = Guid.NewGuid().ToString(),
                Text = "This is a card. \n\nIt is currently attached to the 'To do' list. \n\nFeel free to drag-and-drop me anywhere you want!",
                Index = 0,
                ListId = toDoList.Id
            };
            _context.Cards.Add(firstCard);
            Cards secondCard = new Cards
            {
                Id = Guid.NewGuid().ToString(),
                Text = "The 'All Boards' page is where all the boards combined. \n\nThis is different from most Kanban boards which only let users to navigate within one board at a time.",
                Index = 1,
                ListId = toDoList.Id
            };
            _context.Cards.Add(secondCard);
            Cards thirdCard = new Cards
            {
                Id = Guid.NewGuid().ToString(),
                Text = "You can also drag-and-drop lists, but only in the board the list is attached to (anywhere other than 'All Boards'). \n\nTo create, edit or switch to another board, use the side menu.",
                Index = 0,
                ListId = inProgressList.Id
            };
            _context.Cards.Add(thirdCard);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UsersExists(users.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUsers", new { id = users.Id }, ItemToDTO(users));
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<UsersDTO>> DeleteUsers(string id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            _context.Users.Remove(users);
            await _context.SaveChangesAsync();

            return ItemToDTO(users);
        }

        private bool UsersExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        public static UsersDTO ItemToDTO(Users users) =>
        new UsersDTO
        {
            Id = users.Id,
            FirstName = users.FirstName,
            LastName = users.LastName,
            Username = users.Username,
            Token = users.Token
        };
    }
}
