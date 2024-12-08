using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Notes.API.Data;
using Notes.API.Models.Entities;

namespace Notes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoteController : Controller
    {
        private readonly NoteDbContext _noteDbContext;

        public NoteController(NoteDbContext noteDbContext)
        {
                _noteDbContext = noteDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            return Ok(await _noteDbContext.Note.ToListAsync());
        }

        [HttpGet]
        [Route("{Id:Guid}")]
        [ActionName("GetNoteById")]
        public async Task<IActionResult> GetNoteById([FromRoute] Guid Id)
        {
            //await _noteDbContext.Note.FirstOrDefaultAsync(n => n.Id == Id);
            var note = await _noteDbContext.Note.FindAsync(Id);

            if (note == null)
            {
                return Ok("Not Found");
            }

            return Ok(note);
        }


        [HttpPost]
        public async Task<IActionResult> AddNote([FromBody] Note note)
        {
            note.Id = Guid.NewGuid();
            if (note == null)
                return NotFound();

            await _noteDbContext.Note.AddAsync(note);
            await _noteDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNoteById), new { Id = note.Id}, note);
        }

        [HttpPut]
        [Route("{Id:Guid}")]
        public async Task<ActionResult> UpdateNote([FromRoute] Guid Id, [FromBody] Note Updatenote)
        {
            var existingNote = await _noteDbContext.Note.FindAsync(Id);

            if (existingNote == null || Updatenote == null)
                return NotFound();

            existingNote.Title = Updatenote.Title;
            existingNote.Description = Updatenote.Description;
            existingNote.IsVisible = Updatenote.IsVisible;

            await _noteDbContext.SaveChangesAsync();

            return Ok();
        }


        [HttpDelete]
        [Route("{Id:Guid}")]
        public async Task<IActionResult> DeleteNote([FromRoute] Guid Id )
        {
            var existingNote = await _noteDbContext.Note.FindAsync( Id );

            if (existingNote == null)
                return NotFound();

            _noteDbContext.Note.Remove(existingNote);
            await _noteDbContext.SaveChangesAsync();

            return Ok();
        }

    }
}
