using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelAppApi.Models;
using TravelAppApi.Repository;
using TravelAppApi.Data;
namespace TravelAppApi.Services
{
    public class PdfService
    {
        private readonly TripService _tripService;

        public PdfService(TripService tripService)
        {
            _tripService = tripService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> CreatePdfAsync(int tripId)
        {
            var trip = await _tripService.GetTripsByIdAsync(tripId);

            if (trip == null)
                throw new Exception("Trip not found");

            var stops = trip.Stops.OrderBy(s => s.ArrivalDate).ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().AlignCenter().Text("LỊCH TRÌNH CHUYẾN ĐI")
                        .FontSize(22).Bold().FontColor(Colors.Blue.Darken3);

                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        // Trip info
                        col.Item().Text($"Tên chuyến đi: {trip.Title}").Bold();

                        col.Item().Text($"Điểm đến: {trip.DestinationName}");
                        col.Item().Text($"Từ ngày: {trip.StartDate:dd/MM/yyyy}");
                        col.Item().Text($"Đến ngày: {trip.EndDate:dd/MM/yyyy}");
                        col.Item().Text($"Ngân sách: {trip.Budget:N0} VND");

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken2);

                        // Stops
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(TitleCellStyle).Text("Địa điểm");
                                h.Cell().Element(TitleCellStyle).Text("Đến");
                                h.Cell().Element(TitleCellStyle).Text("Đi");
                                h.Cell().Element(TitleCellStyle).Text("Địa chỉ");
                                h.Cell().Element(TitleCellStyle).Text("Ghi chú");
                            });

                            foreach (var stop in stops)
                            {
                                table.Cell().Element(ContentStyle).Text(stop.Location);
                                table.Cell().Element(ContentStyle).Text(stop.ArrivalDate.ToString("dd/MM HH:mm"));
                                table.Cell().Element(ContentStyle).Text(stop.DepartureDate.ToString("dd/MM HH:mm"));
                                table.Cell().Element(ContentStyle).Text(stop.Address);
                                table.Cell().Element(ContentStyle).Text(stop.Notes);
                            }

                            static IContainer TitleCellStyle(IContainer container)
                            {
                                return container.Padding(5).Background(Colors.Grey.Lighten3).DefaultTextStyle(x => x.Bold());
                            }

                            static IContainer ContentStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.DefaultTextStyle(t => t.FontSize(9).FontColor(Colors.Grey.Darken2));
                        text.Span("Xuất ngày: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        text.Span(" | Trang ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}