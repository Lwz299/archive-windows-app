using Archive.Application.Interfaces;
using Archive.Contracts.Requests;
using Archive.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>عرض الكتب — كل الأدوار المسجّلة</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetBooks([FromQuery] string? search, [FromQuery] string? category)
        {
            return Ok(await _bookService.GetBooksAsync(search, category));
        }

        /// <summary>إضافة كتاب — Admin و SuperAdmin فقط</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<BookResponse>> CreateBook(CreateBookRequest request)
        {
            var created = await _bookService.CreateBookAsync(request);
            return Ok(created);
        }

        /// <summary>تعديل كتاب — Admin و SuperAdmin فقط</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<BookResponse>> UpdateBook(int id, UpdateBookRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { error = "معرف الكتاب غير متطابق" });
            }

            var updated = await _bookService.UpdateBookAsync(request);
            return Ok(updated);
        }

        /// <summary>حذف كتاب — Admin و SuperAdmin فقط</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> DeleteBook(int id)
        {
            var result = await _bookService.DeleteBookAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
