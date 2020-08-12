<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LocationList.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.RoomCheckIn.LocationList" %>

<meta name="viewport" content="width=device-width, initial-scale=1">

<style>
    td.text {
        padding-top:16px !important; 
        padding-bottom:16px !important;
    }
    tr:active {
        background-color:rgba(0, 0, 0, 0.05);
    }

    .center-pills {
        display: flex;
        -o-justify-content: center;
        -webkit-justify-content: center;
        justify-content: center;
        margin-bottom: 10px;
    }
</style>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:Timer ID="timer" runat="server" OnTick="timer_OnTick" />

        <div id="header-fixed text-center">
            <h1 class="text-center">
                <asp:Literal ID="lLocation" runat="server" />
            </h1>
            <ul class="nav nav-pills center-pills">
                
                <li id="liRoomList" runat="server">
                    <asp:LinkButton runat="server" ID="btnBack"><i class="fa fa-list" aria-hidden="true"></i> Launch Page</asp:LinkButton>
                </li>
                <li id="liEvacReport" runat="server">
                    <asp:LinkButton runat="server" ID="btnEvac"><i class="fa fa-exclamation-circle" aria-hidden="true"></i> Evac Report</asp:LinkButton>
                </li>
                <li>
                    <a href="#" onclick="window.location.reload();"><i class="fa fa-refresh" aria-hidden="true"></i></a>
                </li>
            </ul>
        </div>

        <Rock:Grid ID="gLocations" runat="server" DisplayType="Light" AllowSorting="false" OnRowSelected="gLocations_RowSelected" DataKeyNames="LocationId" Font-Size="Large">
            <Columns>
                <Rock:RockBoundField DataField="Area" HeaderText="Age/Grade" ItemStyle-CssClass="text" />
                <Rock:RockBoundField DataField="Location" HeaderText="Room" ItemStyle-CssClass="text" />
                <Rock:RockBoundField DataField="EnRoute" HeaderStyle-CssClass="fa fa-address-card" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" ItemStyle-CssClass="text" />
                <Rock:RockBoundField DataField="InRoom" HeaderStyle-CssClass="fa fa-sign-in" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" ItemStyle-CssClass="text" />
                <Rock:RockBoundField DataField="CheckedOut" HeaderStyle-CssClass="fa fa-sign-out" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" ItemStyle-CssClass="text" />
                <%--<Rock:RockBoundField DataField="Total" HeaderStyle-CssClass="fa fa-check-square" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" ItemStyle-CssClass="text" />--%>
            </Columns>
        </Rock:Grid>
        <%--<asp:LinkButton ID="btnBack" runat="server" Text="Launch Page" CssClass="btn btn-default btn-lg" />--%>
        <%--<asp:LinkButton ID="btnEvac" runat="server" Text="Evac Report" CssClass="btn btn-primary btn-lg pull-right" />--%>
    </ContentTemplate>
</asp:UpdatePanel>