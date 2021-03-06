using System.Data.Entity.Migrations;
using System.Diagnostics.CodeAnalysis;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Migrations
{
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public partial class Initial : DbMigration
    {
        public override void Down()
        {
            DropForeignKey("dbo.ResponseErrors", "ErrorCodeId", "dbo.ErrorCodes");
            DropForeignKey("dbo.ResponseErrors", "ResponseMessageId", "dbo.ResponseMessages");
            DropForeignKey("dbo.LinkedResponseMessageResponseHeaders", "ResponseHeaderId", "dbo.ResponseHeaders");
            DropForeignKey("dbo.LinkedResponseMessageResponseHeaders", "ResponseMessageId", "dbo.ResponseMessages");
            DropForeignKey("dbo.ResponseMessages", "RequestJobId", "dbo.RequestJobs");
            DropForeignKey("dbo.LinkedRequestJobRequestHeaders", "RequestHeaderId", "dbo.RequestHeaders");
            DropForeignKey("dbo.LinkedRequestJobRequestHeaders", "RequestJobId", "dbo.RequestJobs");
            DropIndex("dbo.LinkedResponseMessageResponseHeaders", new[] { "ResponseHeaderId" });
            DropIndex("dbo.LinkedResponseMessageResponseHeaders", new[] { "ResponseMessageId" });
            DropIndex("dbo.LinkedRequestJobRequestHeaders", new[] { "RequestHeaderId" });
            DropIndex("dbo.LinkedRequestJobRequestHeaders", new[] { "RequestJobId" });
            DropIndex("dbo.ResponseHeaders", "IX_ResponseHeader_Key_ValueHash");
            DropIndex("dbo.ResponseHeaders", new[] { "Id" });
            DropIndex("dbo.RequestHeaders", "IX_RequestHeader_Key_ValueHash");
            DropIndex("dbo.RequestHeaders", new[] { "Id" });
            DropIndex("dbo.RequestJobs", new[] { "UriHash" });
            DropIndex("dbo.RequestJobs", new[] { "Method" });
            DropIndex("dbo.RequestJobs", new[] { "Id" });
            DropIndex("dbo.ResponseMessages", "IX_RequestJob_Id");
            DropIndex("dbo.ResponseMessages", new[] { "Id" });
            DropIndex("dbo.ResponseErrors", "IX_ErrorCode_Id");
            DropIndex("dbo.ResponseErrors", "IX_ResponseMessage_Id");
            DropIndex("dbo.ResponseErrors", new[] { "Id" });
            DropIndex("dbo.ErrorCodes", new[] { "Code" });
            DropIndex("dbo.ErrorCodes", new[] { "Id" });
            DropIndex("dbo.ApplicationLogs", new[] { "Id" });
            DropTable("dbo.LinkedResponseMessageResponseHeaders");
            DropTable("dbo.LinkedRequestJobRequestHeaders");
            DropTable("dbo.ResponseHeaders");
            DropTable("dbo.RequestHeaders");
            DropTable("dbo.RequestJobs");
            DropTable("dbo.ResponseMessages");
            DropTable("dbo.ResponseErrors");
            DropTable("dbo.ErrorCodes");
            DropTable("dbo.ApplicationLogs");
        }

        public override void Up()
        {
            CreateTable("dbo.ApplicationLogs", c => new { Id = c.Int(nullable: false, identity: true), HostName = c.String(nullable: false, maxLength: 100), Level = c.String(nullable: false), Logger = c.String(nullable: false), Message = c.String(nullable: false), Method = c.String(nullable: false), Thread = c.String(nullable: false), Exception = c.String(), DateCreated = c.DateTime(nullable: false) }).PrimaryKey(t => t.Id).Index(t => t.Id);

            CreateTable("dbo.ErrorCodes", c => new { Id = c.Int(nullable: false, identity: true), Code = c.Int(nullable: false), Message = c.String(nullable: false), DateCreated = c.DateTime(nullable: false) }).PrimaryKey(t => t.Id).Index(t => t.Id).Index(t => t.Code);

            CreateTable("dbo.ResponseErrors", c => new { Id = c.Int(nullable: false, identity: true), Message = c.String(), OriginalMessage = c.String(), StackTrace = c.String(), DateCreated = c.DateTime(nullable: false), ResponseMessageId = c.Int(nullable: false), ErrorCodeId = c.Int() }).PrimaryKey(t => t.Id).ForeignKey("dbo.ResponseMessages", t => t.ResponseMessageId).ForeignKey("dbo.ErrorCodes", t => t.ErrorCodeId).Index(t => t.Id).Index(t => t.ResponseMessageId, name: "IX_ResponseMessage_Id").Index(t => t.ErrorCodeId, name: "IX_ErrorCode_Id");

            CreateTable("dbo.ResponseMessages", c => new { Id = c.Int(nullable: false, identity: true), StatusCode = c.Int(), ProtocolVersion = c.String(maxLength: 4), DateCreated = c.DateTime(nullable: false), RequestJobId = c.Int(nullable: false) }).PrimaryKey(t => t.Id).ForeignKey("dbo.RequestJobs", t => t.RequestJobId).Index(t => t.Id).Index(t => t.RequestJobId, name: "IX_RequestJob_Id");

            CreateTable("dbo.RequestJobs", c => new { Id = c.Int(nullable: false, identity: true), Method = c.String(nullable: false, maxLength: 16), Uri = c.String(nullable: false), UriHash = c.String(nullable: false, maxLength: 64), HttpVersion = c.String(nullable: false, maxLength: 4), IsCurrentlyScheduled = c.Boolean(nullable: false), IsRunOnce = c.Boolean(nullable: false), LastTimeProcessed = c.DateTime(nullable: false), DateCreated = c.DateTime(nullable: false) }).PrimaryKey(t => t.Id).Index(t => t.Id).Index(t => t.Method).Index(t => t.UriHash);

            CreateTable("dbo.RequestHeaders", c => new { Id = c.Int(nullable: false, identity: true), Key = c.String(nullable: false, maxLength: 64), Value = c.String(nullable: false), ValueHash = c.String(nullable: false, maxLength: 64), DateCreated = c.DateTime(nullable: false) }).PrimaryKey(t => t.Id).Index(t => t.Id).Index(t => new { t.Key, t.ValueHash }, unique: true, name: "IX_RequestHeader_Key_ValueHash");

            CreateTable("dbo.ResponseHeaders", c => new { Id = c.Int(nullable: false, identity: true), Key = c.String(nullable: false, maxLength: 64), Value = c.String(nullable: false), ValueHash = c.String(nullable: false, maxLength: 64), DateCreated = c.DateTime(nullable: false) }).PrimaryKey(t => t.Id).Index(t => t.Id).Index(t => new { t.Key, t.ValueHash }, unique: true, name: "IX_ResponseHeader_Key_ValueHash");

            CreateTable("dbo.LinkedRequestJobRequestHeaders", c => new { RequestJobId = c.Int(nullable: false), RequestHeaderId = c.Int(nullable: false) }).PrimaryKey(t => new { t.RequestJobId, t.RequestHeaderId }).ForeignKey("dbo.RequestJobs", t => t.RequestJobId).ForeignKey("dbo.RequestHeaders", t => t.RequestHeaderId).Index(t => t.RequestJobId).Index(t => t.RequestHeaderId);

            CreateTable("dbo.LinkedResponseMessageResponseHeaders", c => new { ResponseMessageId = c.Int(nullable: false), ResponseHeaderId = c.Int(nullable: false) }).PrimaryKey(t => new { t.ResponseMessageId, t.ResponseHeaderId }).ForeignKey("dbo.ResponseMessages", t => t.ResponseMessageId).ForeignKey("dbo.ResponseHeaders", t => t.ResponseHeaderId).Index(t => t.ResponseMessageId).Index(t => t.ResponseHeaderId);
        }
    }
}