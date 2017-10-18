<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AppointmentNotificationList.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.AppointmentNotificationList" %>


<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-comment-o"></i> Appointment Notifications</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="rGrid" runat="server" AllowSorting="false" RowItemText="Notification" OnRowSelected="rGrid_RowSelected" >
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                <Rock:RockBoundField DataField="FromName" HeaderText="From Name" />
                                <Rock:RockBoundField DataField="FromEmail" HeaderText="From Email" />
                                <Rock:BoolField DataField="SendAnnouncement" HeaderText="Announcements" />
                                <Rock:BoolField DataField="SendReminders" HeaderText="Reminders" />
                                <Rock:DeleteField OnClick="rGrid_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
