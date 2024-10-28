using BookRentalService.Models;
using BookRentalService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookRentalService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks([FromQuery] string title, [FromQuery] string genre)
        {
            _logger.LogInformation("SearchBooks endpoint called with Title: '{Title}' and Genre: '{Genre}'", title, genre);

            try
            {
                var books = await _bookService.SearchBooksAsync(title, genre);
                _logger.LogInformation("SearchBooks operation completed successfully with {Count} books found.", books.Count());
                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching for books with Title: '{Title}' and Genre: '{Genre}'", title, genre);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while searching for books." });
            }
        }

        [HttpPost("rent")]
        public async Task<IActionResult> RentBook([FromBody] RentRequest request)
        {
            _logger.LogInformation("RentBook endpoint called for UserId: {UserId} to rent BookId: {BookId}", request.UserId, request.BookId);

            try
            {
                var result = await _bookService.RentBookAsync(request.UserId, request.BookId);
                _logger.LogInformation("Book with BookId: {BookId} successfully rented by UserId: {UserId}", request.BookId, request.UserId);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while UserId: {UserId} was renting BookId: {BookId}", request.UserId, request.BookId);
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("return/{rentalId}")]
        public async Task<IActionResult> ReturnBook(int rentalId)
        {
            _logger.LogInformation("ReturnBook endpoint called for RentalId: {RentalId}", rentalId);

            try
            {
                await _bookService.ReturnBookAsync(rentalId);
                _logger.LogInformation("Book returned successfully for RentalId: {RentalId}", rentalId);
                return Ok(new { Message = "Book returned successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while returning the book for RentalId: {RentalId}", rentalId);
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("extend-due-date/{rentalId}")]
        public async Task<IActionResult> ExtendDueDate(int rentalId, [FromQuery] int days)
        {
            _logger.LogInformation("ExtendDueDate endpoint called for RentalId: {RentalId} to extend by {Days} days", rentalId, days);

            try
            {
                await _bookService.ExtendDueDateAsync(rentalId, days);
                _logger.LogInformation("Due date for RentalId: {RentalId} extended by {Days} days successfully", rentalId, days);
                return Ok(new { Message = "Due date extended successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while extending due date for RentalId: {RentalId} by {Days} days", rentalId, days);
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("waiting-list")]
        public async Task<IActionResult> AddToWaitingList([FromBody] WaitingListRequest request)
        {
            _logger.LogInformation("AddToWaitingList endpoint called for UserId: {UserId} to add BookId: {BookId} to waiting list", request.UserId, request.BookId);

            try
            {
                await _bookService.AddToWaitingListAsync(request.UserId, request.BookId);
                _logger.LogInformation("UserId: {UserId} successfully added to the waiting list for BookId: {BookId}", request.UserId, request.BookId);
                return Ok(new { Message = "Added to waiting list successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding UserId: {UserId} to the waiting list for BookId: {BookId}", request.UserId, request.BookId);
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
