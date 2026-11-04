using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopii.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeUsersEmailCollation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normalize existing addresses first so the NOCASE unique index is
            // created against canonical values. Two live rows that differed only
            // by case would now collide, which is the intended behavior (the
            // database never legitimately held two accounts for one address).
            migrationBuilder.Sql("UPDATE Users SET Email = LOWER(TRIM(Email));");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "TEXT",
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldCollation: "NOCASE");
        }
    }
}
