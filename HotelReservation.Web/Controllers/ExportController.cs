using System.Text;
using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HotelReservation.Web.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class ExportController : Controller
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ExportController> _logger;

    public ExportController(IReservationService reservationService, ILogger<ExportController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ReservationsCsv()
    {
        try
        {
            var reservations = await _reservationService.GetAllReservationsAsync();
            
            var csv = new StringBuilder();
            csv.AppendLine("ID,Guest Name,Room Number,Hotel,Check-In,Check-Out,Guests,Total Price,Status,Created");

            foreach (var reservation in reservations)
            {
                csv.AppendLine($"{reservation.Id}," +
                             $"\"{reservation.UserName}\"," +
                             $"\"{reservation.RoomNumber}\"," +
                             $"\"{reservation.HotelName}\"," +
                             $"{reservation.CheckInDate:yyyy-MM-dd}," +
                             $"{reservation.CheckOutDate:yyyy-MM-dd}," +
                             $"{reservation.NumberOfGuests}," +
                             $"{reservation.TotalPrice:F2}," +
                             $"{reservation.StatusName}," +
                             $"{reservation.CreatedAt:yyyy-MM-dd HH:mm}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"reservations_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting reservations to CSV");
            TempData["Error"] = "An error occurred while exporting data";
            return RedirectToAction("Index", "Reservations");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ReservationPdf(int id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("RESERVATION RECEIPT")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // Reservation Details
                            column.Item().Text($"Reservation ID: {reservation.Id}").Bold();
                            column.Item().Text($"Status: {reservation.StatusName}").FontColor(
                                reservation.Status == Core.Enums.ReservationStatus.Confirmed ? Colors.Green.Medium : Colors.Grey.Medium);
                            
                            column.Item().PaddingTop(10).LineHorizontal(1);

                            // Guest Information
                            column.Item().PaddingTop(10).Text("Guest Information").Bold().FontSize(16);
                            column.Item().Text($"Name: {reservation.UserName}");
                            column.Item().Text($"Email: {reservation.UserEmail}");
                            column.Item().Text($"Number of Guests: {reservation.NumberOfGuests}");

                            column.Item().PaddingTop(10).LineHorizontal(1);

                            // Hotel & Room Details
                            column.Item().PaddingTop(10).Text("Accommodation Details").Bold().FontSize(16);
                            column.Item().Text($"Hotel: {reservation.HotelName}");
                            column.Item().Text($"Address: {reservation.HotelAddress}");
                            column.Item().Text($"Room Number: {reservation.RoomNumber}");
                            column.Item().Text($"Room Type: {reservation.RoomType}");

                            column.Item().PaddingTop(10).LineHorizontal(1);

                            // Stay Details
                            column.Item().PaddingTop(10).Text("Stay Details").Bold().FontSize(16);
                            column.Item().Text($"Check-In: {reservation.CheckInDate:dddd, MMMM dd, yyyy}");
                            column.Item().Text($"Check-Out: {reservation.CheckOutDate:dddd, MMMM dd, yyyy}");
                            column.Item().Text($"Number of Nights: {reservation.NumberOfNights}");

                            if (!string.IsNullOrWhiteSpace(reservation.SpecialRequests))
                            {
                                column.Item().PaddingTop(10).Text("Special Requests:").Bold();
                                column.Item().Text(reservation.SpecialRequests);
                            }

                            column.Item().PaddingTop(10).LineHorizontal(1);

                            // Pricing
                            column.Item().PaddingTop(10).Text("Payment Details").Bold().FontSize(16);
                            column.Item().Text($"Price per Night: ${reservation.PricePerNight:F2}");
                            column.Item().Text($"Number of Nights: {reservation.NumberOfNights}");
                            column.Item().PaddingTop(5).Text($"Total Price: ${reservation.TotalPrice:F2}").Bold().FontSize(18).FontColor(Colors.Green.Medium);

                            column.Item().PaddingTop(10).LineHorizontal(1);

                            // Booking Details
                            column.Item().PaddingTop(10).Text($"Booked on: {reservation.CreatedAt:MMMM dd, yyyy HH:mm}").FontSize(10);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on ");
                            x.Span(DateTime.Now.ToString("MMMM dd, yyyy")).SemiBold();
                            x.Span(" | Hotel Reservation System");
                        });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"reservation_{id}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for reservation {ReservationId}", id);
            TempData["Error"] = "An error occurred while generating the PDF";
            return RedirectToAction("Details", "Reservations", new { id });
        }
    }
}
