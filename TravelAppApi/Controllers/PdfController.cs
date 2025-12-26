using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TravelAppApi.Models;
using TravelAppApi.Services;

namespace TravelAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    { 
        private readonly PdfService _pdfService;

        public PdfController(PdfService pdfService)
        {
            _pdfService = pdfService;
        }

        [HttpGet("{tripId}")]
        public async Task<IActionResult> GetPdf(int tripId)
        {
            var bytes = await _pdfService.CreatePdfAsync(tripId);

            return File(bytes, "application/pdf", $"trip_{tripId}.pdf");
        }
    }
}
