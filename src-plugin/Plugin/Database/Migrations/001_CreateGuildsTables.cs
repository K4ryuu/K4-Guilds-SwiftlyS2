using FluentMigrator;

namespace K4_Guilds.Database.Migrations;

[Migration(202512021353)]
public class CreateGuildsTables : Migration
{
    public override void Up()
    {
        if (!Schema.Table("k4_guilds").Exists())
        {
            // Guilds table
            Create.Table("k4_guilds")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("name").AsString(128).NotNullable().Unique()
                .WithColumn("tag").AsString(16).NotNullable()
                .WithColumn("leader_steam_id").AsInt64().NotNullable()
                .WithColumn("bank_balance").AsInt64().NotNullable().WithDefaultValue(0)
                .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("updated_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Index("ix_guilds_leader").OnTable("k4_guilds").OnColumn("leader_steam_id");
        }


        if (!Schema.Table("k4_guild_members").Exists())
        {
            // Guild members table
            Create.Table("k4_guild_members")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("guild_id").AsInt32().NotNullable().ForeignKey("k4_guilds", "id").OnDelete(System.Data.Rule.Cascade)
                .WithColumn("steam_id").AsInt64().NotNullable().Unique()
                .WithColumn("player_name").AsString(128).NotNullable()
                .WithColumn("rank_priority").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("joined_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("last_seen").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Index("ix_guild_members_guild").OnTable("k4_guild_members").OnColumn("guild_id");
            Create.Index("ix_guild_members_steam").OnTable("k4_guild_members").OnColumn("steam_id");
        }

        if (!Schema.Table("k4_guild_upgrades").Exists())
        {
            // Guild upgrades table
            Create.Table("k4_guild_upgrades")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("guild_id").AsInt32().NotNullable().ForeignKey("k4_guilds", "id").OnDelete(System.Data.Rule.Cascade)
                .WithColumn("upgrade_type").AsInt32().NotNullable()
                .WithColumn("level").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("purchased_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Index("ix_guild_upgrades_guild").OnTable("k4_guild_upgrades").OnColumn("guild_id");
            Create.UniqueConstraint("uq_guild_upgrade").OnTable("k4_guild_upgrades").Columns("guild_id", "upgrade_type");
        }

        if (!Schema.Table("k4_guild_perks").Exists())
        {
            // Guild perks table
            Create.Table("k4_guild_perks")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("guild_id").AsInt32().NotNullable().ForeignKey("k4_guilds", "id").OnDelete(System.Data.Rule.Cascade)
                .WithColumn("perk_id").AsString(128).NotNullable()
                .WithColumn("level").AsInt32().NotNullable().WithDefaultValue(1)
                .WithColumn("enabled").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("purchased_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("updated_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Index("ix_guild_perks_guild").OnTable("k4_guild_perks").OnColumn("guild_id");
            Create.UniqueConstraint("uq_guild_perk").OnTable("k4_guild_perks").Columns("guild_id", "perk_id");
        }
    }

    public override void Down()
    {
        Delete.Table("k4_guild_perks");
        Delete.Table("k4_guild_upgrades");
        Delete.Table("k4_guild_members");
        Delete.Table("k4_guilds");
    }
}
