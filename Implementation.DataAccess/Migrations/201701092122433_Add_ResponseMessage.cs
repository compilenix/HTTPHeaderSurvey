using System.Data.Entity.Migrations;

namespace Implementation.DataAccess.Migrations
{
    public partial class Add_ResponseMessage : DbMigration
    {
        public override void Down()
        {
            DropForeignKey("dbo.LinkedResponseMessageResponseHeaders", "ResponseHeaderId", "dbo.ResponseHeaders");
            DropForeignKey("dbo.LinkedResponseMessageResponseHeaders", "ResponseMessageId", "dbo.ResponseMessages");
            DropForeignKey("dbo.ResponseMessages", "RequestJobId", "dbo.RequestJobs");
            DropIndex("dbo.LinkedResponseMessageResponseHeaders", new[] { "ResponseHeaderId" });
            DropIndex("dbo.LinkedResponseMessageResponseHeaders", new[] { "ResponseMessageId" });
            DropIndex("dbo.ResponseHeaders", "IX_ResponseHeader_Key_ValueHash");
            DropIndex("dbo.ResponseHeaders", new[] { "Id" });
            DropIndex("dbo.ResponseMessages", "IX_RequestJob_Id");
            DropIndex("dbo.ResponseMessages", new[] { "Id" });
            DropTable("dbo.LinkedResponseMessageResponseHeaders");
            DropTable("dbo.ResponseHeaders");
            DropTable("dbo.ResponseMessages");
        }

        public override void Up()
        {
            CreateTable(
                    "dbo.ResponseMessages",
                    c =>
                        new
                            {
                                Id = c.Int(nullable: false, identity: true),
                                StatusCode = c.Int(nullable: false),
                                ProtocolVersion = c.String(nullable: false, maxLength: 4),
                                DateCreated = c.DateTime(nullable: false),
                                RequestJobId = c.Int(nullable: false)
                            })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RequestJobs", t => t.RequestJobId)
                .Index(t => t.Id)
                .Index(t => t.RequestJobId, name: "IX_RequestJob_Id");

            CreateTable(
                    "dbo.ResponseHeaders",
                    c =>
                        new
                            {
                                Id = c.Int(nullable: false, identity: true),
                                Key = c.String(nullable: false, maxLength: 64),
                                Value = c.String(nullable: false),
                                ValueHash = c.String(nullable: false, maxLength: 64),
                                DateCreated = c.DateTime(nullable: false)
                            })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Id)
                .Index(t => new { t.Key, t.ValueHash }, unique: true, name: "IX_ResponseHeader_Key_ValueHash");

            CreateTable(
                    "dbo.LinkedResponseMessageResponseHeaders",
                    c => new { ResponseMessageId = c.Int(nullable: false), ResponseHeaderId = c.Int(nullable: false) })
                .PrimaryKey(t => new { t.ResponseMessageId, t.ResponseHeaderId })
                .ForeignKey("dbo.ResponseMessages", t => t.ResponseMessageId)
                .ForeignKey("dbo.ResponseHeaders", t => t.ResponseHeaderId)
                .Index(t => t.ResponseMessageId)
                .Index(t => t.ResponseHeaderId);
        }
    }
}