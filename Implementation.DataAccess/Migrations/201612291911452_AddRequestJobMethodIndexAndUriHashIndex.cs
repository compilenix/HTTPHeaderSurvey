using System.Data.Entity;
using System.Data.Entity.Migrations;
using Implementation.Shared;

namespace Implementation.DataAccess.Migrations
{
    public partial class AddRequestJobMethodIndexAndUriHashIndex : DbMigration
    {
        public override void Down()
        {
            DropIndex("dbo.RequestJobs", new[] { "UriHash" });
            DropIndex("dbo.RequestJobs", new[] { "Method" });
            DropColumn("dbo.RequestJobs", "UriHash");
        }

        public override void Up()
        {
            //AddColumn("dbo.RequestJobs", "UriHash", c => c.String(nullable: false, maxLength: 64));
            //CreateIndex("dbo.RequestJobs", "Method");
            //CreateIndex("dbo.RequestJobs", "UriHash");
            Database.SetInitializer<HttpHeaderDbContext>(null);

            var sqlAddColumn = @"
ALTER TABLE [dbo].[RequestJobs] ADD [UriHash] [nvarchar](64) NOT NULL DEFAULT '';
CREATE INDEX [IX_Method] ON [dbo].[RequestJobs]([Method]);
CREATE INDEX [IX_UriHash] ON [dbo].[RequestJobs]([UriHash]);";

            using (var context = new HttpHeaderDbContext())
            using (var unit = new UnitOfWork(context))
            {
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.ValidateOnSaveEnabled = false;

                context.Database.ExecuteSqlCommand(sqlAddColumn);

                foreach (var requestJob in context.RequestJobs.AsNoTracking())
                {
                    requestJob.UriHash = HashUtils.Hash(requestJob.Uri);
                }

                unit.Complete();
                GarbageCollectionUtils.CollectNow();
            }
        }
    }
}