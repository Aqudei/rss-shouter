namespace RSSLoudReader.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ms : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RssEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GeneratedId = c.String(maxLength: 4000),
                        Title = c.String(maxLength: 4000),
                        Url = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RssSources",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 4000),
                        Url = c.String(maxLength: 4000),
                        IsMonitoring = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RssSources");
            DropTable("dbo.RssEntries");
        }
    }
}
