namespace GCloud.Models.Domain
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "InvitationCode", c => c.String());
            AddColumn("dbo.AspNetUsers", "TotalPoints", c => c.String());
            AddColumn("dbo.AspNetUsers", "InvitationCodeSender", c => c.String());
            AddColumn("dbo.AspNetUsers", "DataProtection", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "AGB", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "MarketingAgreement", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "MarketingAgreement");
            DropColumn("dbo.AspNetUsers", "AGB");
            DropColumn("dbo.AspNetUsers", "DataProtection");
            DropColumn("dbo.AspNetUsers", "InvitationCodeSender");
            DropColumn("dbo.AspNetUsers", "TotalPoints");
            DropColumn("dbo.AspNetUsers", "InvitationCode");
        }
    }
}
