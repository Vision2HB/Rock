<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LocationListAPI.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.RoomCheckIn.LocationListAPI" %>

<meta name="viewport" content="width=device-width, initial-scale=1">

<script type="text/javascript">

    $(document).ready(function () {
        //Initial Call
        getLists();

        //20 Second Timer Refresh
        setInterval(getLists, 20000);
    });

    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_endRequest(function (s, e) {
        getLists();
    });

    //Method: Get visible grid list
    function getLists() {
        //Get values from hidden fields
        var checkInTypeId = parseInt($('#<%=hfCheckInTypeId.ClientID%>').val()) || 0;
        var campusId = parseInt($('#<%=hfCampusId.ClientID%>').val()) || 0;
        
        $.ajax({
                type: "GET",
                url: "/api/NorthPoint/InRoom/LocationList",
                data: {
                    checkInTypeId: checkInTypeId,
                    campusId: campusId
                },
                success: function (data) {
                    updateGrid(data);
                }
        });
        
    }

    // Method: Update The Selected Grid with data object
    function updateGrid(data) {

        var $rockGrid = $('#<%=gLocations.ClientID%>');

        $rockGrid.find("tbody").find("tr").remove();

        $.each(data, function (i, item) {
            var $tr = $('<tr align="left" data-row-index="' + i + '" datakey="' + item.LocationId + '" onclick="event.stopPropagation(); openLocationDetail(this, ' + item.LocationId + '); return false;" > ').append(
                $('<td class="text grid-select-cell">').html(item.ParentGroup),
                $('<td class="text grid-select-cell">').html(item.Location),
                $('<td class="text grid-select-cell" style="width:50px;">').html(item.EnRouteTotal),
                $('<td class="text grid-select-cell" style="width:50px;">').html(item.InRoomTotal),
                $('<td class="text grid-select-cell" style="width:50px;">').html(item.CheckedOutTotal)
            ).appendTo($rockGrid);
        });

    }

    //Event Handler: opens details page
    function openLocationDetail(el, locationId) {

        //Get values from hidden fields
        var checkInTypeId = parseInt($('#<%=hfCheckInTypeId.ClientID%>').val()) || 0;
        var detailsPageId = parseInt($('#<%=hfDetailsPageId.ClientID%>').val()) || 0;

        //open location page
        if (detailsPageId != 0) {
            window.location.href = '/page/' + detailsPageId + '?CheckinTypeId=' + checkInTypeId + '&LocationId=' + locationId;
        }
    }


    

</script>

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
        <asp:HiddenField ID="hfCampusId" runat="server" />
        <asp:HiddenField ID="hfCheckInTypeId" runat="server" />
        <asp:HiddenField ID="hfDetailsPageId" runat="server" />
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

        <Rock:Grid ID="gLocations" runat="server" DisplayType="Light" AllowSorting="false" DataKeyNames="LocationId" Font-Size="Large">
            <Columns>
                <Rock:RockBoundField DataField="Area" HeaderText="Age/Grade" ItemStyle-CssClass="text" />
                <Rock:RockBoundField DataField="Location" HeaderText="Room" ItemStyle-CssClass="text" />
                <Rock:RockBoundField DataField="EnRoute" HeaderStyle-CssClass="fa fa-address-card" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" ItemStyle-CssClass="text" />
                <Rock:RockBoundField DataField="InRoom" HeaderStyle-CssClass="fa fa-sign-in" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" ItemStyle-CssClass="text" />
                <Rock:RockBoundField DataField="CheckedOut" HeaderStyle-CssClass="fa fa-sign-out" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" ItemStyle-CssClass="text" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>