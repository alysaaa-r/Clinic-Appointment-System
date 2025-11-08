using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddPasswordSalt : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PasswordSalt",
            table: "Admins",
            type: "longtext",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "PasswordSalt",
            table: "Users",
            type: "longtext",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PasswordSalt", table: "Admins");
        migrationBuilder.DropColumn(name: "PasswordSalt", table: "Users");
    }
}
