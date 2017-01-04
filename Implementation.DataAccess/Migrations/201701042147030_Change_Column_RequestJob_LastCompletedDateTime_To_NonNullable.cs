namespace Implementation.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Change_Column_RequestJob_LastCompletedDateTime_To_NonNullable : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE [dbo].[RequestJobs] SET [LastCompletedDateTime] = CONVERT(DATETIME, '2016-01-01') WHERE [LastCompletedDateTime] IS NULL;");
            AlterColumn("dbo.RequestJobs", "LastCompletedDateTime", c => c.DateTime(nullable: false));
        }

        public override void Down()
        {
            AlterColumn("dbo.RequestJobs", "LastCompletedDateTime", c => c.DateTime());
        }
    }
}
