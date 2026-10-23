using FnacDarty.TechnicalTest.Library.Domain.Entities;
using FnacDarty.TechnicalTest.Library.Domain.Interfaces;
using FnacDarty.TechnicalTest.Library.Models;
using FnacDarty.TechnicalTest.Library.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace FnacDarty.TechnicalTest.Library.Controllers
{
    [ApiController]
    [Route("api/library")]
    public class LibraryController : ControllerBase
    {
        private readonly ILibraryService _libraryService;
        private readonly ICustomerService _customerService;
        public LibraryController(ILibraryService libraryService, ICustomerService customerService)
        {
            _libraryService = libraryService;
            _customerService = customerService;
        }

        [HttpGet("getAllBooks")]
        public IActionResult GetAllBooks()
        {
            var books = _libraryService.GetAllBooks();

            return Ok(books);
        }

        [HttpPost("addBook")]
        public IActionResult AddBook([FromForm] AddBookRequest request)
        {
            _libraryService.AddBook(request.Title, request.Author);

            return Ok();
        }

        
        [HttpPost("borrow")]
        public IActionResult Borrow([FromBody] LoanRequest request)
        {
            if (request is null || request.CustomerId <= 0 || request.BookIds is null || request.BookIds.Count == 0)
                return BadRequest("La requête doit contenir un customerId valide et au moins un identifiant de livre.");

            try
            {
                var result = _customerService.BorrowBooks(request.CustomerId, request.BookIds);
                return Ok(result);
            }
            catch (CustomerNotFoundException)
            {
                return NotFound("Le client est introuvable.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Une erreur inattendue est survenue, veuillez réessayer ultérieurement.");
            }
        }
    }
}