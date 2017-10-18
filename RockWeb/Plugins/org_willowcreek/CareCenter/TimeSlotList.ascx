<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeSlotList.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.TimeSlotList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-calendar"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>
            <div class="panel-body">

                <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />

                <div class="grid grid-panel">
                    <Rock:Grid ID="gTimeSlots" runat="server" AllowPaging="true" DisplayType="Full" RowItemText="Time Slot" OnRowSelected="gTimeSlots_RowSelected" AllowSorting="false">
                        <Columns>
                            <Rock:RockBoundField DataField="ScheduleTitle" HeaderText="ScheduleTitle" />
                            <Rock:RockBoundField DataField="Schedule" HeaderText="Schedule" />
                            <Rock:RockBoundField DataField="Notification" HeaderText="Notification" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:DeleteField OnClick="gTimeSlots_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
