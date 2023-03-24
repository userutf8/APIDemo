using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIDemo.Model;
using APIDemo.Model.DB;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static APIDemo.Model.Enums;

namespace APIDemo.Controllers
{
    internal interface IClientsController
    {
        [HttpPost("AddClient")]
        public Task<ActionResult<Client>> AddClient(Client client);

        [HttpGet("FindClient{Alias}")]
        public Task<ActionResult<Client>> FindClient(string alias);

        [HttpGet("FindClients")]
        public Task<ActionResult<IEnumerable<Client>>> FindClients();

        [HttpPut("UpdateClient/{alias}")]
        public Task<IActionResult> UpdateClient(string alias, Client client);

        [HttpPut("UpdateClientStatus")]
        public Task<IActionResult> UpdateClientStatus(Client updClient);

        [HttpDelete("DeleteClient/{alias}")]
        public Task<IActionResult> RemoveClient(string alias);
    }

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase, IClientsController
    {
        private readonly Context _context;
        public ClientsController(Context context)
        {
            _context = context;
        }

        [HttpPost("AddClient")]
        public async Task<ActionResult<Client>> AddClient(Client client)
        {
            if (_context.Clients == null)
            {
                return Problem("Entity set 'Context.Clients'  is null.");
            }

            client.UserId = GetUserIDFromHttpContext(HttpContext);
            if (client.UserId == "")
            {
                return BadRequest("User ID is empty or unable to retrieve.");
            }

            if (ClientExists(client))
            {
                return BadRequest($"Client with Alias={client.Alias} already exists for User={client.UserId}");
            }

            client.Status = ClientStatus.New;

            _context.Clients.Add(client);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ClientExists(client.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            switch (client.Type)
            {
                case ClientType.Person:
                    {
                        var person = new Person()
                        {
                            ClientId = client.Id,
                            FullName = client.Alias
                        };
                        _context.Persons.Add(person);
                    }; break;
                case ClientType.Company:
                    {
                        var company = new Company()
                        {
                            ClientId = client.Id,
                            Name = client.Alias
                        };
                        _context.Companies.Add(company);
                    }; break;
                default: return BadRequest($"Client Type Invalid");
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return CreatedAtAction("FindClient", new { alias = client.Alias }, client);
        }

        [HttpGet("FindClient/{alias}")]
        public async Task<ActionResult<Client>> FindClient(string alias)
        {
            if (_context.Clients == null)
            {
                return NotFound();
            }
            var UserId = GetUserIDFromHttpContext(HttpContext);
            if (UserId == "")
            {
                return BadRequest("User ID is empty or unable to retrieve.");
            }
            Client? client = await FindClientByAliasAsync(alias, UserId);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        [HttpGet("FindClients")]
        public async Task<ActionResult<IEnumerable<Client>>> FindClients()
        {
            if (_context.Clients == null)
            {
                return NotFound();
            }

            var UserId = GetUserIDFromHttpContext(HttpContext);
            if (UserId == "")
            {
                return BadRequest("User ID is empty or unable to retrieve.");
            }

            return await _context.Clients.Where(c => c.UserId == UserId).ToListAsync<Client>();
        }

        [HttpPut("UpdateClient/{alias}")]
        public async Task<IActionResult> UpdateClient(string alias, Client updClient)
        {
            if (_context.Clients == null)
            {
                return NotFound();
            }
            var UserId = GetUserIDFromHttpContext(HttpContext);
            if (UserId == "")
            {
                return BadRequest("User ID is empty or unable to retrieve.");
            }

            Client? client = await FindClientByAliasAsync(alias, UserId);

            if (client == null)
            {
                return NotFound();
            }

            if ((client.Alias == updClient.Alias) &&
                (client.Type == updClient.Type))
                return NoContent(); // nothing to do

            if (updClient.Status != 0)
            {
                return BadRequest("Forbidden to change status.");
            }

            if (alias != updClient.Alias)
            {
                Client? dupClient = await FindClientByAliasAsync(updClient.Alias, UserId);
                if (dupClient != null)
                {
                    return BadRequest("Client already exists");
                }
            }

            if (client.Type != updClient.Type)
            {
                switch (updClient.Type)
                {
                    case ClientType.Company:
                        {
                            var xPerson = _context.Persons.Where(p => p.ClientId == client.Id).FirstOrDefault();
                            Company Company = new Company()
                            {
                                ClientId = client.Id,
                                Name = xPerson == null ? updClient.Alias : xPerson.FullName,
                                Status = ClientStatus.New,
                                TIN = ""
                            };
                            if (xPerson != null)
                                _context.Persons.Remove(xPerson);
                            _context.Companies.Add(Company);
                        }; break;
                    case ClientType.Person:
                        {
                            var xCompany = _context.Companies.Where(c => c.ClientId == client.Id).FirstOrDefault();
                            Person Person = new Person()
                            {
                                ClientId = client.Id,
                                FullName = xCompany == null ? updClient.Alias : xCompany.Name,
                                Status = ClientStatus.New
                            };
                            if (xCompany != null)
                                _context.Companies.Remove(xCompany);
                            _context.Persons.Add(Person);
                        }
                        break;
                    default: return NotFound(updClient.Type);
                }
            }
            client.Alias = updClient.Alias;
            client.Type = updClient.Type;
            client.UserId = UserId;
            _context.Entry(client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(client.Id))
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

        [HttpPut("UpdateClientStatus")]
        public async Task<IActionResult> UpdateClientStatus(Client updClient)
        {
            if (_context.Clients == null)
            {
                return NotFound();
            }
            var UserId = GetUserIDFromHttpContext(HttpContext);
            if (UserId == "")
            {
                return BadRequest("User ID is empty or unable to retrieve.");
            }
            if (!UserCanUpdateStatus())
            {
                return Forbid("This user cannot change status.");
            }

            var client = await _context.Clients.Where(c => c.Id == updClient.Id).FirstOrDefaultAsync();

            if (client == null)
            {
                return NotFound();
            }

            if (client.Status == updClient.Status)
            {
                return NoContent();
            }

            switch(client.Type)
            {
                case ClientType.Company:
                    {
                        var xCompany = _context.Companies.Where(c => c.ClientId == client.Id).FirstOrDefault();
                        if (xCompany != null)
                        {
                            if (xCompany.Status != updClient.Status)
                            {
                                xCompany.Status = updClient.Status;
                                _context.Entry(xCompany).State = EntityState.Modified;
                            }
                        }
                    }; break;
                case ClientType.Person:
                    {
                        var xPerson = _context.Persons.Where(p => p.ClientId == client.Id).FirstOrDefault();
                        if(xPerson != null)
                        {
                            if(xPerson.Status != updClient.Status)
                            {
                                xPerson.Status = updClient.Status;
                                _context.Entry(xPerson).State = EntityState.Modified;
                            }
                        }   
                    }; break;
                default: return BadRequest(client.Type);
            }
            var xClientFiles = _context.ClientFiles.Where(c => c.ClientId == client.Id).ToList();
            if(xClientFiles != null)
            {
                foreach (var xClientFile in xClientFiles)
                {
                    xClientFile.Status = updClient.Status;
                    _context.Entry(xClientFile).State = EntityState.Modified;
                }
            }

            client.Status = updClient.Status;
            _context.Entry(client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(client.Id))
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

        [HttpDelete("DeleteClient/{alias}")]
        public async Task<IActionResult> RemoveClient(string alias)
        {
            // TODO: duplicate code >> 
            if (_context.Clients == null)
            {
                return NotFound();
            }
            var UserId = GetUserIDFromHttpContext(HttpContext);
            if (UserId == "")
            {
                return BadRequest("User ID is empty or unable to retrieve.");
            }

            Client? client = await FindClientByAliasAsync(alias, UserId);

            if (client == null)
            {
                return NotFound();
            }
            // TODO: duplicate code <<

            switch (client.Type)
            {
                case ClientType.Person:
                    {
                        var person = await _context.Persons.Where<Person>(x => x.ClientId == client.Id).FirstOrDefaultAsync();
                        if (person != null)
                        {
                            _context.Persons.Remove(person);
                        }
                    }; break;
                case ClientType.Company:
                    {
                        var company = await _context.Companies.Where<Company>(x => x.ClientId == client.Id).FirstOrDefaultAsync();
                        if (company != null)
                        {
                            _context.Companies.Remove(company);
                        }
                    }; break;
                default: break;
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        internal async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            if (_context.Clients == null)
            {
                return NotFound();
            }
            return await _context.Clients.ToListAsync();
        }

        [HttpGet("{id}")]
        internal async Task<ActionResult<Client>> GetClient(string id)
        {
            if (_context.Clients == null)
            {
                return NotFound();
            }
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        [HttpPut("{id}")]
        internal async Task<IActionResult> PutClient(Guid id, Client client)
        {
            if (id != client.Id)
            {
                return BadRequest();
            }

            _context.Entry(client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
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

        [HttpDelete("{id}")]
        internal async Task<IActionResult> DeleteClient(string id)
        {
            if (_context.Clients == null)
            {
                return NotFound();
            }
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientExists(Guid id)
        {
            return (_context.Clients?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private bool ClientExists(Client client)
        {
            return (_context.Clients?.Any(c => c.Alias == client.Alias && c.UserId == client.UserId)).GetValueOrDefault();
        }

        private async Task<Client?> FindClientByAliasAsync(string alias, string UserId)
        {
            return await _context.Clients.Where(
                c => c.Alias == alias && c.UserId == UserId).FirstOrDefaultAsync();
        }

        private bool UserCanUpdateStatus()
        {
            // some fastpace implementation of user Role logic.
            // should implement that with AspNetRoles
            if (HttpContext.User != null)
            {
                if (HttpContext.User.Identity != null)
                {
                    return ((HttpContext.User.Identity.Name == "admin") ||
                            (HttpContext.User.Identity.Name == "operator"));
                }
            }
            return false;
        }
        private string GetUserIDFromHttpContext(HttpContext hc)
        {
            if (hc.User != null)
            {
                if (hc.User.Identity != null)
                {
                    if (hc.User.Identity.IsAuthenticated)
                    {
                        // TODO: nameidentifier at Claims[0] mentions jwt, whereas nameidentifier at Claims[3] has actual Id.
                        // They both have ClaimsType.NameIdentifier
                        // So this is more like a workaround than a normal solution to find last claim.
                        var result = hc.User.Claims.LastOrDefault(
                            x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return "";

        }

    }

}
