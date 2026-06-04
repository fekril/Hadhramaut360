using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Had360back.Migrations
{
    public partial class AddFullName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add the column with a temporary default of '' so existing rows
            // get a non-null value and the NOT NULL constraint is satisfied immediately.
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            // Step 2: Populate FullName for every existing user.
            // Priority: UserName -> Email -> 'User' (defensive fallback).
            // Existing UserName values are intentionally NOT changed.
            migrationBuilder.Sql(@"
                UPDATE [AspNetUsers]
                SET [FullName] =
                    CASE
                        WHEN [UserName] IS NOT NULL AND LEN(LTRIM(RTRIM([UserName]))) > 0
                            THEN LTRIM(RTRIM([UserName]))
                        WHEN [Email] IS NOT NULL AND LEN(LTRIM(RTRIM([Email]))) > 0
                            THEN LTRIM(RTRIM([Email]))
                        ELSE 'User'
                    END
                WHERE [FullName] = '' OR [FullName] IS NULL
            ");

            // Step 3: Drop the temporary default constraint so future rows must supply
            // an explicit FullName value (no silent empty-string fallback).
            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: false,
                oldDefaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");
        }
    }
}
