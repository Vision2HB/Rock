// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 32, "1.10.3" )]
    public class ApprovalSqlChanges : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [InitialApproverAliasId] INT NULL
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [InitialApprovalDateTime] DATETIME NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_InitialApproverAliasId] FOREIGN KEY([InitialApproverAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_InitialApproverAliasId]             
" );
            Sql( @"
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [SpecialApproverAliasId] INT NULL
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [SpecialApprovalDateTime] DATETIME NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_SpecialApproverAliasId] FOREIGN KEY([SpecialApproverAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_SpecialApproverAliasId]             
" );
            Sql( @"
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [FinalApproverAliasId] INT NULL
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [FinalApprovalDateTime] DATETIME NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_FinalApproverAliasId] FOREIGN KEY([FinalApproverAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_FinalApproverAliasId]             
" );


        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}