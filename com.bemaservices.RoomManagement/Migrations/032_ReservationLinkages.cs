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
    public class ReservationLinkage : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
          Sql( @"

                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [ReservationId] [int] NOT NULL,
	                [EventItemOccurrenceId] [int] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationLinkage] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Reservation] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_Reservation] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Reservation]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Linkage] FOREIGN KEY([EventItemOccurrenceId])
                REFERENCES [dbo].[EventItemOccurrence] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Linkage]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_ModifiedByPersonAliasId]

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