using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomuraApi.Models;
using Tweetinvi;
using System;

namespace HomuraApi.Controllers
{
    [Route("api/v1/artist")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        private readonly ArtistContext _context;
        private readonly TwitterClient _client;

        public ArtistController(ArtistContext context)
        {
            _context = context;
            _client = new TwitterClient(
                Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY"),
                Environment.GetEnvironmentVariable("TWITTER_CONSUMER_SECRET"),
                Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN"),
                Environment.GetEnvironmentVariable("TWITTER_ACCESS_SECRET"));
        }

        // GET: api/v1/artist
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Artist>>> GetArtists()
        {
            List<Artist> artists = await _context.Artists.ToListAsync();

            foreach (Artist artist in artists)
            {
                await artist.GetMediaTweets(_client);
            }
            
            return artists;
        }

        // GET: api/v1/artist/5
        [HttpGet("{id:long}")]
        public async Task<ActionResult<Artist>> GetArtist(long id)
        {
            Artist artist = await _context.Artists.FindAsync(id);

            if (artist == null)
            {
                return NotFound();
            }

            await artist.GetMediaTweets(_client);

            return artist;
        }

        // PUT: api/v1/artist/5
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id:long}")]
        public async Task<IActionResult> PutArtist(long id, Artist artist)
        {
            if (id != artist.TwitterId)
            {
                return BadRequest();
            }

            _context.Entry(artist).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArtistExists(id))
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

        // POST: api/v1/artist
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Artist>> PostArtist(Artist artist)
        {
            await artist.Initialize(_client);
            
            if (artist.TwitterId == 0)
            {
                return BadRequest(new
                {
                    status = "Bad Request",
                    code = 400,
                    reason = "A username or valid twitter profile URL must be supplied."
                });
            }
            
            Artist dupeArtist = await _context.Artists.FindAsync(artist.TwitterId);
            if (dupeArtist != null)
            {
                return BadRequest(new
                {
                    status = "Bad Request",
                    code = 400,
                    reason = "User is already in the database."
                });
            }
            
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetArtist), new { id = artist.TwitterId }, artist);
        }

        // DELETE: api/v1/artist/5
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteArtist(long id)
        {
            Artist artist = await _context.Artists.FindAsync(id);
            if (artist == null)
            {
                return NotFound();
            }

            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ArtistExists(long id)
        {
            return _context.Artists.Any(e => e.TwitterId == id);
        }
    }
}
