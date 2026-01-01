using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetToExpenseCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "ExpenseCategories",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Budget",
                table: "ExpenseCategories");
        }
    }
}
