namespace QRProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTwoPro : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clients", "SupportNumber", c => c.String());
            AddColumn("dbo.Clients", "LaghvPayamak", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clients", "LaghvPayamak");
            DropColumn("dbo.Clients", "SupportNumber");
        }
    }
}
