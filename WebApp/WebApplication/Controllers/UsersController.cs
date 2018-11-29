using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Microsoft.EntityFrameworkCore.Internal;
using WebApplication.services;

namespace WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersContext _context;

        public UsersController(UsersContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public IEnumerable<User> GetUsers()
        {
            return _context.Users;
        }

        [HttpGet("xml")]
        public async Task<IActionResult> GetUsersInXml()
        {
            string output;

            XmlSerializer serializer = new XmlSerializer(typeof(DbSet<User>));

            StringWriter stringWriter = new StringWriter();

            serializer.Serialize(stringWriter, _context.Users);
            output = stringWriter.ToString();
            
            XslCompiler compiler = new XslCompiler();

            output = compiler.Transform(output, @"xslStyles/ArrayOfUsers.xsl");
            return Ok(output);
        }


        // GET: api/Users/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // GET: api/Users/Bill
        [HttpGet("{name}")]
        public async Task<IActionResult> GetUserByName([FromRoute] string name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var users = _context.Users.Where(s => s.Name == name).ToList();

            if (users.Count == 0)
            {
                return NotFound();
            }
            return Ok(users);
        }

        // GET: api/Users/5/xml
        [HttpGet("{id:int}/xml")]
        public async Task<IActionResult> GetUserInXml([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }

            string output;

            XmlSerializer serializer = new XmlSerializer(typeof(User));
            StringWriter stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, user);
            output = stringWriter.ToString();
            XslCompiler compiler = new XslCompiler();
            output = compiler.Transform(output, @"xslStyles/OneUser.xsl");

            return Ok(output);
        }

        // PUT: api/Users/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutUser([FromRoute] int id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        [HttpPost("multiple")]
        public async Task<IActionResult> PostUsers([FromBody] User[] users)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (User user in users)
            {
                _context.Users.Add(user);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}