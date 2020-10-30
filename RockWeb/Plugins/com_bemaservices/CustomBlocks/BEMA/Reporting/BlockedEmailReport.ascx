<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockedEmailReport.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.CustomBlocks.Reporting.BlockedEmailReport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Blocked Emails
                </h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbErrorMessages" runat="server" NotificationBoxType="Danger" Visible="false" />
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnApplyFilterClick="rFilter_ApplyFilterClick" OnDisplayFilterValue="rFilter_DisplayFilterValue" OnClearFilterClick="rFilter_ClearFilterClick">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                        <Rock:RockTextBox ID="tbEmail" runat="server" Label="Email" />
                        <Rock:RockTextBox ID="tbDomain" runat="server" Label="Domain" />
                        <Rock:RockCheckBoxList ID="cblTypeFilter" runat="server" RepeatDirection="Horizontal" Label="Type">
                            <asp:ListItem Text="Bounce" Value="Bounce" />
                            <asp:ListItem Text="Unsubscribe" Value="Unsubscribe" />
                            <asp:ListItem Text="Complaint" Value="Complaint" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockTextBox ID="tbErrorCode" runat="server" Label="Error Code" />

                        <Rock:DateRangePicker ID="drpCreatedDateTime" runat="server" Label="Date Range" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:SelectField></Rock:SelectField>
                            <Rock:RockBoundField DataField="Id" HeaderText="Person Id" SortExpression="Id" />
                            <Rock:RockBoundField DataField="NickName" HeaderText="Nick Name" SortExpression="NickName" />
                            <Rock:RockBoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" />
                            <Rock:RockBoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                            <Rock:RockBoundField DataField="Domain" HeaderText="Domain" SortExpression="Domain" />
                            <Rock:RockBoundField DataField="Type" HeaderText="Type" SortExpression="Type" />
                            <Rock:RockBoundField DataField="ErrorCode" HeaderText="Error Code" SortExpression="ErrorCode" />
                            <Rock:RockBoundField DataField="CreatedDateTime" HeaderText="Created" SortExpression="CreatedDateTime" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
