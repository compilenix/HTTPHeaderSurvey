using System.Data.Entity.Migrations;

namespace Implementation.DataAccess.Migrations
{
    public partial class Initial : DbMigration
    {
        public override void Down()
        {
            DropForeignKey("dbo.LinkedRequestJobRequestHeaders", "RequestHeaderId", "dbo.RequestHeaders");
            DropForeignKey("dbo.LinkedRequestJobRequestHeaders", "RequestJobId", "dbo.RequestJobs");
            DropIndex("dbo.LinkedRequestJobRequestHeaders", new[] { "RequestHeaderId" });
            DropIndex("dbo.LinkedRequestJobRequestHeaders", new[] { "RequestJobId" });
            DropIndex("dbo.RequestJobs", new[] { "Id" });
            DropIndex("dbo.RequestHeaders", "IX_RequestHeader_Key_ValueHash");
            DropIndex("dbo.RequestHeaders", new[] { "Id" });
            DropIndex("dbo.ApplicationLogs", new[] { "Id" });
            DropTable("dbo.LinkedRequestJobRequestHeaders");
            DropTable("dbo.RequestJobs");
            DropTable("dbo.RequestHeaders");
            DropTable("dbo.ApplicationLogs");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.ApplicationLogs",
                c =>
                    new
                        {
                            Id = c.Int(nullable: false, identity: true),
                            HostName = c.String(nullable: false, maxLength: 100),
                            Level = c.String(nullable: false),
                            Logger = c.String(nullable: false),
                            Message = c.String(nullable: false),
                            Method = c.String(nullable: false),
                            Thread = c.String(nullable: false),
                            Exception = c.String(),
                            DateCreated = c.DateTime(nullable: false)
                        }).PrimaryKey(t => t.Id).Index(t => t.Id);

            CreateTable(
                    "dbo.RequestHeaders",
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
                .Index(t => new { t.Key, t.ValueHash }, unique: true, name: "IX_RequestHeader_Key_ValueHash");

            CreateTable(
                "dbo.RequestJobs",
                c =>
                    new
                        {
                            Id = c.Int(nullable: false, identity: true),
                            Method = c.String(nullable: false, maxLength: 16),
                            Uri = c.String(nullable: false),
                            HttpVersion = c.String(nullable: false, maxLength: 4),
                            IsCurrentlyScheduled = c.Boolean(nullable: false),
                            IsRunOnce = c.Boolean(nullable: false),
                            LastCompletedDateTime = c.DateTime(),
                            DateCreated = c.DateTime(nullable: false)
                        }).PrimaryKey(t => t.Id).Index(t => t.Id);

            CreateTable(
                    "dbo.LinkedRequestJobRequestHeaders",
                    c => new { RequestJobId = c.Int(nullable: false), RequestHeaderId = c.Int(nullable: false) })
                .PrimaryKey(t => new { t.RequestJobId, t.RequestHeaderId })
                .ForeignKey("dbo.RequestJobs", t => t.RequestJobId)
                .ForeignKey("dbo.RequestHeaders", t => t.RequestHeaderId)
                .Index(t => t.RequestJobId)
                .Index(t => t.RequestHeaderId);
        }
    }
}