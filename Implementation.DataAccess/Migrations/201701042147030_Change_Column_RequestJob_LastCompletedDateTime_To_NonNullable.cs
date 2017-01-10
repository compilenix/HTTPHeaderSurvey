using System.Data.Entity.Migrations;

namespace Implementation.DataAccess.Migrations
{
    public partial class Change_Column_RequestJob_LastCompletedDateTime_To_NonNullable : DbMigration
    {
        public override void Down()
        {
            AlterColumn("dbo.RequestJobs", "LastCompletedDateTime", c => c.DateTime());
        }

        public override void Up()
        {
            Sql("UPDATE [dbo].[RequestJobs] SET [LastCompletedDateTime] = CONVERT(DATETIME, '2016-01-01') WHERE [LastCompletedDateTime] IS NULL;");
            AlterColumn("dbo.RequestJobs", "LastCompletedDateTime", c => c.DateTime(nullable: false));
        }
    }
}