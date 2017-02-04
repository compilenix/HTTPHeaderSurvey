namespace Implementation.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Rename_RequestJobCompletedDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestJobs", "LastTimeProcessed", c => c.DateTime(nullable: false));
            DropColumn("dbo.RequestJobs", "LastCompletedDateTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RequestJobs", "LastCompletedDateTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.RequestJobs", "LastTimeProcessed");
        }
    }
}
