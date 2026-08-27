using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogicLink.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Circuits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    OwnerName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    GridSize = table.Column<int>(type: "integer", nullable: false),
                    SnapToGrid = table.Column<bool>(type: "boolean", nullable: false),
                    ShowGateLabels = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Circuits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CircuitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    X = table.Column<double>(type: "double precision", nullable: false),
                    Y = table.Column<double>(type: "double precision", nullable: false),
                    Rotation = table.Column<double>(type: "double precision", nullable: false),
                    Label = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    InputValue = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gates_Circuits_CircuitId",
                        column: x => x.CircuitId,
                        principalTable: "Circuits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CircuitId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromGateId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromPinIndex = table.Column<int>(type: "integer", nullable: false),
                    ToGateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToPinIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wires_Circuits_CircuitId",
                        column: x => x.CircuitId,
                        principalTable: "Circuits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Circuits_IsDeleted",
                table: "Circuits",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Gates_CircuitId",
                table: "Gates",
                column: "CircuitId");

            migrationBuilder.CreateIndex(
                name: "IX_Wires_CircuitId_FromGateId",
                table: "Wires",
                columns: new[] { "CircuitId", "FromGateId" });

            migrationBuilder.CreateIndex(
                name: "IX_Wires_CircuitId_ToGateId",
                table: "Wires",
                columns: new[] { "CircuitId", "ToGateId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gates");

            migrationBuilder.DropTable(
                name: "Wires");

            migrationBuilder.DropTable(
                name: "Circuits");
        }
    }
}
