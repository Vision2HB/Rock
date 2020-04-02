using Rock.Plugin;

namespace com.bemaservices.CheckIn
{
    [MigrationNumber( 1, "1.8.8" )]
    public class CheckInGroupAttributes : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //Add No Grade Enforced group attribute for checkin 
            RockMigrationHelper.UpdateEntityAttribute(
                "Rock.Model.Group"
                , Rock.SystemGuid.FieldType.BOOLEAN
                , "GroupTypeId"
                , "15" /* Check-In By Age */
                , "No Grade Enforced (BEMA Family Check-In)"
                , "Used by BEMA Family Check-In workflow: Forces person to not have a grade, for criteria filtering."
                , 3
                , ""
                , "FAC2A15B-70F8-41DD-BB90-3FC5B59C5DAF"
                , "com.bemaservices.CheckIn.NoGradeEnforced"
                );

            // Add Exclude Other Criteria Groups for checkin
            RockMigrationHelper.UpdateEntityAttribute(
                "Rock.Model.Group"
                , Rock.SystemGuid.FieldType.BOOLEAN
                , "GroupTypeId"
                , "15" /* Check-In By Age */
                , "Exclude Other Criteria Groups (BEMA Family Check-In)"
                , "Used by BEMA Family Check-In workflow: Excludes other criteria groups if this one is available."
                , 4
                , ""
                , "744C0CBD-4D12-4F36-AC1A-086818A4C19B"
                , "com.bemaservices.CheckIn.ExcludeOtherCriteriaGroups"
                );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute("FAC2A15B-70F8-41DD-BB90-3FC5B59C5DAF");
            RockMigrationHelper.DeleteAttribute("744C0CBD-4D12-4F36-AC1A-086818A4C19B");
        }
    }
}

