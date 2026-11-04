using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopii.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Empty baseline for the pre-existing Shopii SQLite schema.
    ///
    /// The database was authored separately from the EF model, so there is no
    /// initial "create everything" migration. This baseline records the model
    /// as it existed before the email collation change and intentionally does
    /// no work. The schema itself is created by the hand-authored SQL scripts
    /// applied by <see cref="DatabaseMigrationRunner"/>.
    /// </summary>
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
