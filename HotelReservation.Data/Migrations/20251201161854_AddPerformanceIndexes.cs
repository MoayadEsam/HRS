using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Salaries_Month_Year",
                table: "Salaries",
                columns: new[] { "Month", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_Salaries_Status",
                table: "Salaries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CheckInDate",
                table: "Reservations",
                column: "CheckInDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CheckOutDate",
                table: "Reservations",
                column: "CheckOutDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Status",
                table: "Reservations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_IncomeDate_Type",
                table: "Incomes",
                columns: new[] { "IncomeDate", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_Type",
                table: "Incomes",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseDate_Status",
                table: "Expenses",
                columns: new[] { "ExpenseDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_Status",
                table: "Expenses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_Date",
                table: "Attendances",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_Status",
                table: "Attendances",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Salaries_Month_Year",
                table: "Salaries");

            migrationBuilder.DropIndex(
                name: "IX_Salaries_Status",
                table: "Salaries");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CheckInDate",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CheckOutDate",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_Status",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Incomes_IncomeDate_Type",
                table: "Incomes");

            migrationBuilder.DropIndex(
                name: "IX_Incomes_Type",
                table: "Incomes");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ExpenseDate_Status",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_Status",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_Date",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_Status",
                table: "Attendances");
        }
    }
}
