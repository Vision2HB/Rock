<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LocationDetailAPI.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.RoomCheckIn.LocationDetailAPI" %>

<meta name="viewport" content="width=device-width, initial-scale=1">

<script type="text/javascript">
    //20 Second Timer Refresh
    var refreshInterval = setInterval(getLists, 20000);

    $(document).ready(function () {
        //Initial Call
        getLists();

    });

    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_endRequest(function (s, e) {
        getLists();
    });

    //Method: Get visible grid list
    function getLists() {
        //Get values from hidden fields
        var locationId = parseInt($('#<%=hfLocationId.ClientID%>').val()) || 0;
        var checkInTypeId = parseInt($('#<%=hfCheckInTypeId.ClientID%>').val()) || 0;
        var campusId = parseInt($('#<%=hfCampusId.ClientID%>').val()) || 0;

        //Set by Active Tab
        var activeTab = "EnRoute";
        if ($('#<%=hfActiveTab.ClientID%>').val()) {
            activeTab = $('#<%=hfActiveTab.ClientID%>').val();
        }
        SetTabUI(activeTab);

        if (activeTab.endsWith("InRoom")) {

            $.ajax({
                type: "GET",
                url: "/api/NorthPoint/InRoom/LocationDetail",
                data: {
                    locationId: locationId,
                    checkInTypeId: checkInTypeId,
                    campusId: campusId,
                    grid: "InRoom"
                },
                success: function (data) {
                    updateGrid(data, "InRoom");
                }
            });
        }
        else if (activeTab.endsWith("HealthNotes")) {
            $.ajax({
                type: "GET",
                url: "/api/NorthPoint/InRoom/LocationDetail",
                data: {
                    locationId: locationId,
                    checkInTypeId: checkInTypeId,
                    campusId: campusId,
                    grid: "Notes"
                },
                success: function (data) {
                    updateGrid(data, "HealthNotes");
                }
            });
        }
        else if (activeTab.endsWith("CheckedOut")) {
            $.ajax({
                type: "GET",
                url: "/api/NorthPoint/InRoom/LocationDetail",
                data: {
                    locationId: locationId,
                    checkInTypeId: checkInTypeId,
                    campusId: campusId,
                    grid: "CheckedOut"
                },
                success: function (data) {
                    updateGrid(data, "CheckedOut");
                }
            });
        }
        else {
            $.ajax({
                type: "GET",
                url: "/api/NorthPoint/InRoom/LocationDetail",
                data: {
                    locationId: locationId,
                    checkInTypeId: checkInTypeId,
                    campusId: campusId,
                    grid: "EnRoute"
                },
                success: function (data) {
                    updateGrid(data, "EnRoute");
                }
            });

            $.ajax({
                type: "GET",
                url: "/api/NorthPoint/InRoom/LocationDetail",
                data: {
                    locationId: locationId,
                    checkInTypeId: checkInTypeId,
                    campusId: campusId,
                    grid: "EnRouteOther"
                },
                success: function (data) {
                    updateGrid(data, "EnRouteOther");
                }
            });
        }

    }

    // Method: Update The Selected Grid with data object
    function updateGrid(data, grid) {

        //Reset Refresh Interval ( do not refresh immediately after a button press )
        clearInterval(refreshInterval);
        refreshInterval = setInterval(getLists, 20000);

        //Update Totals:
        $('#<%=pillEnRoute.ClientID%>').find("span.badge").html(data.Totals.EnRoute);
        $('#<%=pillInRoom.ClientID%>').find("span.badge").html(data.Totals.InRoom);
        $('#<%=pillHealthNotes.ClientID%>').find("span.badge").html(data.Totals.Notes);
        $('#<%=pillCheckedOut.ClientID%>').find("span.badge").html(data.Totals.CheckOut);

        $('#<%=lRoomCount.ClientID%>').html(data.Totals.InRoom + " In Room");

        if (grid == "EnRoute") {

            var $rockGrid = $('#<%=gEnRoute.ClientID%>');

            $rockGrid.find("tbody").find("tr").remove();

            $.each(data.CheckInResults, function (i, item) {
                var $tr = $('<tr align="left" data-row-index="' + i + '" datakey="' + item.AttendanceId + '" onclick="event.stopPropagation(); openEditDialog(this, ' + item.AttendanceId + '); return false;" > ').append(
                    $('<td class="grid-columncommand" align="center">').html('<a class="btn btn-lg btn-primary fa fa-sign-in" onclick="event.stopPropagation(); moveAttendance(this, ' + item.AttendanceId + ', \'InRoom\', \'EnRoute\'); return false;"> Check In</a>'),
                    $('<td class="text grid-select-cell">').html(item.Name),
                    $('<td class="text grid-select-cell">').html(item.Code),
                    $('<td class="text grid-select-cell">').html(item.Group),
                    $('<td class="text grid-select-cell">').html(item.SpecialNote),
                    $('<td class="grid-columncommand" align="center">').html('<a class="btn btn-lg btn-default fa fa-sign-out" onclick="event.stopPropagation(); moveAttendance(this, ' + item.AttendanceId + ', \'CheckedOut\', \'EnRoute\'); return false;"> Check Out</a>')
                ).appendTo($rockGrid);
            });

            //Add Schedule Check Out All
            if ($('#<%=hfCheckOutAll.ClientID%>').val() && $('#<%=hfCheckOutAll.ClientID%>').val() == 'True') {
                var schedules = {};
                $.each(data.CheckInResults, function (i, item) {
                    if (item.ScheduleId > 0) {

                        schedules[item.ScheduleId] = { ScheduleId: item.ScheduleId, Schedule: item.Schedule };
                    }
                });

                var $checkOutAll = $('#<%=enRouteCheckOutAll.ClientID %>');
                $checkOutAll.find("a").remove();

                $.each(schedules, function (i, item) {
                    $checkOutAll.append('<a class="btn btn-default" onclick="checkOutAll(' + item.ScheduleId + ', \'EnRoute\')">Clear Out En Route: ' + item.Schedule + '</a>');
                });
            }
            
        }
        else if (grid == "EnRouteOther") {
            var $rockGrid = $('#<%=gEnRouteOther.ClientID%>');

            $rockGrid.find("tbody").find("tr").remove();

            //filter by search bar results
            if ($('#enRouteSearch').val()) {
                var searchValue = $('#enRouteSearch').val().toLowerCase();
                data.CheckInResults = $.grep(data.CheckInResults, function (n, i) {
                    return (n.Name.toLowerCase().indexOf(searchValue) !== -1 || n.Code.toLowerCase().indexOf(searchValue) !== -1);
                });
            }

            $.each(data.CheckInResults, function (i, item) {
                var $tr = $('<tr align="left" data-row-index="' + i + '" datakey="' + item.AttendanceId + '" onclick="event.stopPropagation(); openEditDialog(this, ' + item.AttendanceId + '); return false;" > ').append(
                    $('<td class="grid-columncommand" align="center">').html('<a class="btn btn-lg btn-primary fa fa-sign-in" onclick="event.stopPropagation(); confirmMoveToNewRoom(this, ' + item.AttendanceId + '); return false;"> Check In</a>'),
                    $('<td class="text grid-select-cell">').html(item.Name),
                    $('<td class="text grid-select-cell">').html(item.Code),
                    $('<td class="text grid-select-cell">').html(item.Group),
                    $('<td class="text grid-select-cell">').html(item.SpecialNote),
                    $('<td class="grid-columncommand" align="center">')//.html('<a class="btn btn-lg btn-default fa fa-sign-out" onclick="moveAttendance(this, ' + item.AttendanceId + ', \'CheckedOut\', \'EnRouteOther\'); return false;"> Check Out</a>')
                ).appendTo($rockGrid);
            });
        }
        else if (grid == 'InRoom') {
            var $rockGrid = $('#<%=gInRoom.ClientID%>');

            $rockGrid.find("tbody").find("tr").remove();
            var stayingEnabled = ($('#<%=hfStaying.ClientID%>').val() == 'True');

            $.each(data.CheckInResults, function (i, item) {
                var staying = (item.Staying == true) ? 'btn-warning' : 'btn-default';
                var $tr = $('<tr align="left" data-row-index="' + i + '" datakey="' + item.AttendanceId + '" onclick="event.stopPropagation(); openEditDialog(this, ' + item.AttendanceId + '); return false;" > ').append(
                    $('<td class="text grid-select-cell">').html(item.Name),
                    $('<td class="text grid-select-cell">').html(item.Code),
                    $('<td class="text grid-select-cell">').html(item.Group),
                    $('<td class="text grid-select-cell">').html(item.SpecialNote),
                    ( stayingEnabled == true ? $('<td class="grid-columncommand" align="center">').html('<a class="btn btn-lg ' + staying + ' fa fa-clock-o" onclick="event.stopPropagation(); stayAttendance(this, ' + item.AttendanceId + ' ); return false; "> Staying</a>') : ''),
                    $('<td class="grid-columncommand" align="center">').html('<a class="btn btn-lg btn-primary fa fa-address-card" onclick="event.stopPropagation(); moveAttendance(this, ' + item.AttendanceId + ', \'EnRoute\', \'InRoom\'); return false;"> En Route</a>'),
                    $('<td class="grid-columncommand" align="center">').html('<a class="btn btn-lg btn-default fa fa-sign-out" onclick="event.stopPropagation(); moveAttendance(this, ' + item.AttendanceId + ', \'CheckedOut\', \'InRoom\'); return false;"> Check Out</a>')
                ).appendTo($rockGrid);
            });

            //Add Schedule Check Out All
            if ($('#<%=hfCheckOutAll.ClientID%>').val() && $('#<%=hfCheckOutAll.ClientID%>').val() == 'True') {
                var schedules = {};
                $.each(data.CheckInResults, function (i, item) {
                    schedules[item.ScheduleId] = { ScheduleId: item.ScheduleId, Schedule: item.Schedule };
                });

                var $checkOutAll = $('#<%=divInRoomCheckOutAll.ClientID %>');
                $checkOutAll.find("a").remove();

                $.each(schedules, function (i, item) {
                    if (item.ScheduleId > 0) {

                        $checkOutAll.append('<a class="btn btn-default" onclick="checkOutAll(' + item.ScheduleId + ', \'InRoom\')">Check Out All: ' + item.Schedule + '</a>');
                    }
                });
            }
        }
        else if (grid == 'HealthNotes') {
            var $rockGrid = $('#<%=gHealthNotes.ClientID%>');

            $rockGrid.find("tbody").find("tr").remove();

            $.each(data.CheckInResults, function (i, item) {
                var $tr = $('<tr align="left" data-row-index="' + i + '" datakey="' + item.AttendanceId + '" onclick="event.stopPropagation(); openEditDialog(this, ' + item.AttendanceId + '); return false;" > ').append(
                    $('<td class="text grid-select-cell">').html(item.Name),
                    $('<td class="text grid-select-cell">').html(item.Code),
                    $('<td class="text grid-select-cell">').html(item.Group),
                    $('<td class="text grid-select-cell">').html(item.SpecialNote),
                    $('<td class="text grid-select-cell">').html(item.Allergy),
                ).appendTo($rockGrid);
            });
        }
        else if (grid == 'CheckedOut') {
            var $rockGrid = $('#<%=gCheckedOut.ClientID%>');

            $rockGrid.find("tbody").find("tr").remove();

            $.each(data.CheckInResults, function (i, item) {
                var $tr = $('<tr align="left" data-row-index="' + i + '" datakey="' + item.AttendanceId + '" onclick="event.stopPropagation(); openEditDialog(this, ' + item.AttendanceId + '); return false;" > ').append(
                    $('<td class="text grid-select-cell">').html(item.Name),
                    $('<td class="text grid-select-cell">').html(item.Code),
                    $('<td class="text grid-select-cell">').html(item.Group),
                    $('<td class="text grid-select-cell">').html(item.SpecialNote),
                    $('<td class="grid-columncommand" align="center">').html('<a class="btn btn-lg btn-primary fa fa-address-card" onclick="event.stopPropagation(); moveAttendance(this, ' + item.AttendanceId + ', \'EnRoute\', \'CheckedOut\'); return false;"> En Route</a>'),
                    $('<td class="grid-columncommand" align="center">').html('<a class="btn btn-lg btn-default fa fa-sign-in" onclick="event.stopPropagation(); moveAttendance(this, ' + item.AttendanceId + ', \'InRoom\', \'CheckedOut\'); return false;"> Check In</a>')
                ).appendTo($rockGrid);
            });
        }
        
    }

    //Event Handler: Check-In button clicked
    function moveAttendance(el, attendanceId, command, grid) {

        //Get values from hidden fields
        var locationId = parseInt($('#<%=hfLocationId.ClientID%>').val()) || 0;
        var checkInTypeId = parseInt($('#<%=hfCheckInTypeId.ClientID%>').val()) || 0;
        var campusId = parseInt($('#<%=hfCampusId.ClientID%>').val()) || 0;

        //Disable current row with button press
        $(el).closest("tr").removeAttr("onclick");
        $(el).closest("tr").find("a").removeAttr("onclick");
        $(el).closest("tr").addClass("tr-inactive");

        //Ajax call to update
        if (command == "EnRoute") {
            $.ajax({
                type: "POST",
                url: "/api/NorthPoint/InRoom/LocationDetail/MoveAttendance?" + 
                    $.param({
                        attendanceId: attendanceId,
                        locationId: locationId,
                        checkInTypeId: checkInTypeId,
                        campusId: campusId,
                        moveDirection: "EnRoute",
                        grid: grid
                    }),
                success: function (data) {
                    updateGrid(data, grid);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    //reload page from the server (could cause a loop of refreshes)
                    location.reload(true);
                }
            });
        }
        else if (command == "InRoom") {
            $.ajax({
                type: "POST",
                url: "/api/NorthPoint/InRoom/LocationDetail/MoveAttendance?" +
                    $.param({
                        attendanceId: attendanceId,
                        locationId: locationId,
                        checkInTypeId: checkInTypeId,
                        campusId: campusId,
                        moveDirection: "InRoom",
                        grid: grid
                    }),
                success: function (data) {
                    updateGrid(data, grid);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    //reload page from the server (could cause a loop of refreshes)
                    location.reload(true);
                }
            });
        }
        else if (command == "CheckedOut") {
            $.ajax({
                type: "POST",
                url: "/api/NorthPoint/InRoom/LocationDetail/MoveAttendance?" +
                    $.param({
                        attendanceId: attendanceId,
                        locationId: locationId,
                        checkInTypeId: checkInTypeId,
                        campusId: campusId,
                        moveDirection: "CheckedOut",
                        grid: grid
                    }),
                success: function (data) {
                    updateGrid(data, grid);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    //reload page from the server (could cause a loop of refreshes)
                    location.reload(true);
                }
            });
        }
    }

    function checkOutAll(scheduleId, grid) {
        // Confirm CheckOutAll
        var c = confirm("Please Confirm: Check Out All?");

        if (c != true) {
            return;
        }

        //Get values from hidden fields
        var locationId = parseInt($('#<%=hfLocationId.ClientID%>').val()) || 0;
        var checkInTypeId = parseInt($('#<%=hfCheckInTypeId.ClientID%>').val()) || 0;
        var campusId = parseInt($('#<%=hfCampusId.ClientID%>').val()) || 0;

        $.ajax({
            type: "POST",
            url: "/api/NorthPoint/InRoom/LocationDetail/CheckOutAttendanceGrid?" +
                $.param({
                    LocationId: locationId,
                    CheckInTypeId: checkInTypeId,
                    CampusId: campusId,
                    Grid: grid,
                    ScheduleId: scheduleId
                }),
            success: function (data) {
                getLists();

            },
            error: function (jqXHR, textStatus, errorThrown) {
                //reload page from the server (could cause a loop of refreshes)
                location.reload(true);
            }
        });
    }

    function stayAttendance(el, attendanceId ) {
        //Get values from hidden fields
        var locationId = parseInt($('#<%=hfLocationId.ClientID%>').val()) || 0;
        var checkInTypeId = parseInt($('#<%=hfCheckInTypeId.ClientID%>').val()) || 0;
        var campusId = parseInt($('#<%=hfCampusId.ClientID%>').val()) || 0;

        $.ajax({
            type: "POST",
            url: "/api/NorthPoint/InRoom/LocationDetail/MarkAsStaying?" +
                $.param({
                    attendanceId: attendanceId,
                    campusId: campusId,
                    checkInTypeId: checkInTypeId
                }),
            success: function (data) {
                //updateGrid(data, grid);
                if (data == true) {
                    $(el).removeClass("btn-default");
                    $(el).addClass("btn-warning");
                } else {
                    $(el).removeClass("btn-warning");
                    $(el).addClass("btn-default");
                }
                
            },
            error: function (jqXHR, textStatus, errorThrown) {
                //reload page from the server (could cause a loop of refreshes)
                location.reload(true);
            }
        });
    }

    // Event Handler: navigation tab clicked
    function NavBar_Click(el, tabString) {
        //Set active tab Hidden Field 
        $('#<%=hfActiveTab.ClientID%>').val(tabString);
        
        //Call getLists() to bind grids
        getLists();
    }

    //Set Tab UI:
    function SetTabUI(activeTab) {
        

        $('#<%=pillEnRoute.ClientID%>').parent().parent().find(".NavBarButton").each(function () {
            $(this).parent().removeClass("active");
        });

        if (activeTab == 'EnRoute') {
            $('#<%=pillEnRoute.ClientID%>').parent().addClass("active");
        }
        else if (activeTab == 'InRoom') {
            $('#<%=pillInRoom.ClientID%>').parent().addClass("active");
        }
        else if (activeTab == 'HealthNotes') {
            $('#<%=pillHealthNotes.ClientID%>').parent().addClass("active");
        }
        else if (activeTab == 'CheckedOut') {
            $('#<%=pillCheckedOut.ClientID%>').parent().addClass("active");
        }

        //Clear all tab content divs
        $("#<%=divContent.ClientID %>").children().each(function () {
            $(this).removeClass("active");
        });

        //Set active tab
        $("#<%=divContent.ClientID %>").children('div[data-id="' + activeTab + '"]').addClass("active");

    }

    //Event Handler: on search bar Enter or Button Press
    function searchEnRoute(el) {
        getLists();
    }

    //Event Handler: KeyUP on Search Bar
    $('#enRouteSearch').keyup(function (event) {
        if (event.keyCode === 13) {
            $('#enRouteSearchButton').click();
        }
    });

    // Event Handler: Confirm Move to New Room
    function confirmMoveToNewRoom(el, attendanceId) {
        //__doPostBack('showConfirmDialog', attendanceId);
        var r = confirm("Are you sure you would like to move this child to your classroom?");
        if (r == true) {
            moveAttendance(el, attendanceId, "InRoom", "EnRouteOther");
        }
        //getLists();
    }

    //Show Edit dialog on row click
    function openEditDialog(el, attendanceId) {
        //Assign attendance ID to HF
        $('#<%=hfAttendanceId.ClientID%>').val(attendanceId);

        //Do Postback to updatepanel
        __doPostBack('<%=upContent.ClientID %>', 'ShowEditDialog' )
    }


    // Event Handler: 
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
        $('#<%=hfPhoneNumber.ClientID %>').val('');
        $('#<%=hfSMSAliasId.ClientID %>').val('');
    }



    Sys.Application.add_load(function () {
        $('.tenkey a.digit').click(function () {
            $phoneNumber = $("input[id$='tbPIN']");
            $phoneNumber.val($phoneNumber.val() + $(this).html());
        });
        $('.tenkey a.back').click(function () {
            $phoneNumber = $("input[id$='tbPIN']");
            $phoneNumber.val($phoneNumber.val().slice(0, -1));
        });
        $('.tenkey a.clear').click(function () {
            $phoneNumber = $("input[id$='tbPIN']");
            $phoneNumber.val('');
        });

        // set focus to the input unless on a touch device
        var isTouchDevice = 'ontouchstart' in document.documentElement;
        if (!isTouchDevice) {
            if ($('.checkin-phone-entry').length) {
                $('.checkin-phone-entry').focus();
            }
        }
    });
    
</script>
<style>
    .center-pills {
        display: flex;
        -o-justify-content: center;
        -webkit-justify-content: center;
        justify-content: center;
        margin-bottom: 10px;
    }

    td.text {
        padding-top: 16px !important;
        padding-bottom: 16px !important;
    }

    tr:active {
        background-color: rgba(0, 0, 0, 0.05);
    }

    .tr-inactive {
        background-color: #ececec;
        color: #b9b9b9;
    }
    .smsbutton {
        margin-top: 5px;
        margin-bottom: 5px;
        text-align: left;
    }
    .grid-columncommand .btn {
        width: auto;
        padding: 10px 16px;
    }

    
</style>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfActiveTab" runat="server" />
        <asp:HiddenField ID="hfActiveDialog" runat="server" />
        <asp:HiddenField ID="hfAttendanceId" runat="server" />
        <asp:HiddenField ID="hfPersonId" runat="server" />
        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfGroupTypeId" runat="server" />
        <asp:HiddenField ID="hfCampusId" runat="server" />
        <asp:HiddenField ID="hfCheckInTypeId" runat="server" />
        <asp:HiddenField ID="hfLocationId" runat="server" />
        <asp:HiddenField ID="hfEnRouteId" runat="server" />
        <asp:HiddenField ID="hfStaying" runat="server" Value="False" />
        <asp:HiddenField ID="hfCheckOutAll" runat="server" Value="False" />
        <asp:HiddenField ID="hfMessages" runat="server"/>
        <asp:HiddenField ID="hfFromNumber" runat="server"/>
        <asp:HiddenField ID="hfMoveAttendanceId" runat="server" />


        <div id="header-fixed">
            <div class="row">
                <div class="col-sm-6">
                    <h1 class="text-center">
                         <asp:Literal ID="lLocation" runat="server" />
                    </h1>
                </div>
                <div class="col=sm-6">
                    <h1 class="text-center">
                        <asp:Label ID="lRoomCount" runat="server" />
                    </h1>
                </div>
            </div>
            
            <ul Id="ulNav" class="nav nav-pills center-pills">
                <li id="liEnRoute" runat="server" class="active">
                    <asp:LinkButton ID="pillEnRoute" CssClass="NavBarButton" OnClientClick="NavBar_Click(this, 'EnRoute'); return false;" runat="server"><i class="fa fa-address-card" aria-hidden="true"></i> En Route<span class="badge">0</span></asp:LinkButton>
                </li>
                <li id="liInRoom" runat="server">
                    <asp:LinkButton ID="pillInRoom" CssClass="NavBarButton" OnClientClick="NavBar_Click(this, 'InRoom'); return false;" runat="server"><i class="fa fa-sign-in" aria-hidden="true"></i> In Room<span class="badge">0</span></asp:LinkButton>
                </li>
                <li id="liHealthNotes" runat="server">
                    <asp:LinkButton ID="pillHealthNotes" CssClass="NavBarButton" OnClientClick="NavBar_Click(this, 'HealthNotes'); return false;" runat="server"><i class="fa fa-exclamation-triangle" aria-hidden="true"></i> Notes<span class="badge">0</span></asp:LinkButton>
                </li>
                <li id="liCheckedOut" runat="server">
                    <asp:LinkButton ID="pillCheckedOut" CssClass="NavBarButton" OnClientClick="NavBar_Click(this, 'CheckedOut'); return false;" runat="server"><i class="fa fa-sign-out" aria-hidden="true"></i> Checked Out<span class="badge">0</span></asp:LinkButton>
                </li>
                <li id="liRoomList" runat="server">
                    <a href="<%= _roomListUrl %>"><i class="fa fa-list" aria-hidden="true"></i> Room List</a>
                </li>
                <li id="liEvacReport" runat="server">
                    <a href="<%= _evacUrl %>"><i class="fa fa-exclamation-circle" aria-hidden="true"></i> Evac Report</a>
                </li>
                <li>
                    <a href="#" onclick="window.location.reload();"><i class="fa fa-refresh" aria-hidden="true"></i></a>
                </li>
            </ul>
        </div>

        <div id="divContent" class="tab-content" runat="server">
            <div id="divEnRoute" runat="server" class="tab-pane active margin-b-lg" data-id="EnRoute">
                <Rock:Grid ID="gEnRoute" runat="server" DataKeyNames="Id,PersonId,GroupId,GroupTypeId" DisplayType="Light" AllowSorting="false"
                    Font-Size="Large" ShowHeader="true" AllowPaging="false" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <Rock:LinkButtonField CssClass="btn btn-lg btn-primary fa fa-sign-in" Text=" Check In" HeaderStyle-HorizontalAlign="Center" />
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Code" HeaderText="Code" SortExpression="Code" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="Group" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="SpecialNote" HeaderText="" HtmlEncode="false" HtmlEncodeFormatString="false" ItemStyle-CssClass="text"/>
                        <Rock:LinkButtonField CssClass="btn btn-lg btn-default fa fa-sign-out" Text=" Check Out" HeaderStyle-HorizontalAlign="Center" />
                    </Columns>
                </Rock:Grid>
                <div class="container text-center">
                    <div id="enRouteCheckOutAll" class="btn-group text-center" runat="server">
                    </div>
                </div>
                <br />
                <h4 class="text-center">Other Location Check-Ins:</h4>
                <div class="input-group">
                    <input id="enRouteSearch" type="search" name="Search" class="form-control" placeholder="Search By Name or Code..."/>
                    <div class="input-group-btn">
                        <button id="enRouteSearchButton" class="btn btn-primary" type="submit" onclick="searchEnRoute(this); return false;">
                            <span class="fa fa-search"></span>
                        </button>
                    </div>
                </div>
                <Rock:Grid ID="gEnRouteOther" runat="server" DataKeyNames="Id,PersonId,GroupId,GroupTypeId" DisplayType="Light" AllowSorting="false"
                     Font-Size="Medium" ShowHeader="false" AllowPaging="false" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <Rock:LinkButtonField CssClass="btn btn-default fa fa-sign-in" Text=" Check In"  HeaderStyle-HorizontalAlign="Center" />
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Code" HeaderText="Code" SortExpression="Code" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="Group" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="SpecialNote" HeaderText="" HtmlEncodeFormatString="false" HtmlEncode="false" ItemStyle-CssClass="text" />
                        <Rock:LinkButtonField CssClass="btn btn-default fa fa-sign-out" Text=" Check Out"  HeaderStyle-HorizontalAlign="Center" />
                    </Columns>
                </Rock:Grid>
            </div>
            <div id="divInRoom" runat="server" class="tab-pane" data-id="InRoom">
                <Rock:Grid ID="gInRoom" runat="server" DataKeyNames="Id,PersonId,GroupId,GroupTypeId" DisplayType="Light" AllowSorting="false"
                    Font-Size="Large" ShowHeader="true" AllowPaging="false" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Code" HeaderText="Code" SortExpression="Code" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="Group" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="SpecialNote" HeaderText="" HtmlEncodeFormatString="false" HtmlEncode="false" ItemStyle-CssClass="text" />
                        <Rock:RockTemplateField>
                            <ItemTemplate>
                                <Rock:BootstrapButton ID="btnStaying" Visible='<%# hfStaying.Value == "True" %>' CssClass='<%# Eval("Staying").ToString() + " btn btn-lg fa fa-clock-o" %>' runat="server" CommandArgument='<%# Eval("Id") %>' Text=" Staying" />
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:LinkButtonField ID="btnInRoomEnRoute" CssClass="btn btn-lg btn-default fa fa-address-card" Text=" En Route" HeaderStyle-HorizontalAlign="Center" />
                        <Rock:LinkButtonField CssClass="btn btn-lg btn-default fa fa-sign-out" Text=" Check Out" HeaderStyle-HorizontalAlign="Center" />
                    </Columns>
                </Rock:Grid>
                <div class="container text-center">
                    <div id="divInRoomCheckOutAll" class="btn-group text-center" runat="server">
                    </div>
                </div>
            </div>
            <div id="divHealthNotes" runat="server" class="tab-pane" data-id="HealthNotes">
                <Rock:Grid ID="gHealthNotes" runat="server" DataKeyNames="Id,PersonId,GroupId,GroupTypeId" DisplayType="Light" AllowSorting="false"
                     Font-Size="Large" ShowHeader="true" AllowPaging="false" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Code" HeaderText="Code" SortExpression="Code" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="Group" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="SpecialNote" HeaderText="" HtmlEncodeFormatString="false" HtmlEncode="false" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Note" HeaderText="Allergy / Health Note" ItemStyle-CssClass="text" />
                    </Columns>
                </Rock:Grid>
            </div>
            <div id="divCheckedOut" runat="server" class="tab-pane" data-id="CheckedOut">
                <Rock:Grid ID="gCheckedOut" runat="server" DataKeyNames="Id,PersonId,GroupId,GroupTypeId" DisplayType="Light" AllowSorting="false"
                     Font-Size="Large" ShowHeader="true" AllowPaging="false" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Code" HeaderText="Code" SortExpression="Code" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="Group" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="SpecialNote" HeaderText="" HtmlEncodeFormatString="false" HtmlEncode="false" ItemStyle-CssClass="text" />
                        <Rock:LinkButtonField ID="btnCheckedOutEnRoute" CssClass="btn btn-lg btn-default fa fa-building-o" Text=" En Route" HeaderStyle-HorizontalAlign="Center" />
                        <Rock:LinkButtonField CssClass="btn btn-lg btn-default fa fa-sign-in" Text=" Check In"  HeaderStyle-HorizontalAlign="Center" />
                    </Columns>
                </Rock:Grid>
            </div>
        </div>

        <Rock:ModalDialog ID="dlgEdit" runat="server" Title="Edit Person" OnSaveClick="dlgEdit_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="EditPerson">
            <Content>

                <Rock:RockTextBox ID="tbNickName" runat="server" Label="First Name" Required="true" />
                <Rock:DatePartsPicker ID="dpBirthdate" runat="server" Label="Birthdate" Required="true" />
                <Rock:NotificationBox ID="nbSendNote" runat="server" Dismissable="true" NotificationBoxType="Info" Visible="false" />
               
                <Rock:RockTextBox ID="tbNotes" runat="server" Label="Note To Group Director" TextMode="MultiLine" />
                <Rock:BootstrapButton ID="btnSendNote" runat="server" Text="Send Note" CssClass="btn btn-primary pull-right" OnClick="btnSendNote_Click" />
               
                
                <Rock:BootstrapButton ID="btnShowEdit" runat="server" Text="Staff PIN Code" CssClass="btn btn-default" OnClick="btnShowEdit_Click" />

                <asp:Panel ID="pnlPIN" runat="server" CssClass="text-center row">
                    <div class="checkin-search-body col-sm-offset-3 col-sm-6 ">
                        <Rock:RockTextBox ID="tbPIN" CssClass="checkin-phone-entry" TextMode="Password" runat="server" Label="PIN" />

                        <div class="tenkey checkin-phone-keypad">
                            <div>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">1</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">2</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">3</a>
                            </div>
                            <div>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">4</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">5</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">6</a>
                            </div>
                            <div>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">7</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">8</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">9</a>
                            </div>
                            <div>
                                <a href="#" class="btn btn-default btn-lg btn-keypad command clear">Clr</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">0</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad command back"><i class="fas fa-backspace"></i></a>
                            </div>
                        </div>
                        <Rock:BootstrapButton ID="btnLoginPIN" runat="server" Text="Login" OnClick="lbLogin_Click" CssClass="btn btn-primary btn-lg" />

                    </div>
                </asp:Panel>

                <Rock:RockTextBox ID="tbAllergy" runat="server" Label="Allergies" Required="false" />
                <Rock:RockTextBox ID="tbHealthNote" runat="server" Label="Medical Note" Required="false" />
                <Rock:RockTextBox ID="tbLegalNote" runat="server" Label="Legal Note" Required="false" />
                <Rock:RockTextBox ID="tbParentLocation" runat="server" Label="Attendance Note" Required="false" />
                <label class="control-label">Label Printer<Rock:HelpBlock Text="Choose where to reprint the tag" runat="server" /></label>
                <div class="input-group">
                    <span class="input-group-addon"><i class="fa fa-id-card-o" aria-hidden="true"></i></span>
                    <Rock:RockDropDownList ID="ddlKiosk" runat="server" CssClass="input-xlarge" OnSelectedIndexChanged="ddlKiosk_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" />
                    <span class="input-group-btn">
                        <asp:LinkButton CssClass="btn btn-primary" ID="btnPrint" OnClick="btnPrint_Click" runat="server" Text="Reprint Tag" />
                    </span>
                </div>
                <br />
                <label class="control-label">Checked In By</label>
                <div class="input-group">
                    <ul style="padding-left: 0; margin: 0; list-style:none;">
                    <asp:Repeater ID="rAdults" runat="server" OnItemDataBound="rAdults_OnItemDataBound">
                        <ItemTemplate>
                                <li style="float:left; margin-right: 15px; text-decoration:none; margin-bottom: 5px;">
                                    <%#Eval( "Name" ) %><%#Eval("Caption") %>

                                    <asp:Repeater runat="server" ID="rNumbers" OnItemCommand="rNumbers_OnItemCommand" OnItemDataBound="rNumbers_OnItemDataBound">
                                        <ItemTemplate><br/>
                                            <asp:LinkButton CommandName="SMS" ID="btnSMS" CommandArgument='<%# Eval("PersonAliasId") + ";" + Eval("Number") %>' width="100%" runat="server"
                                                            CssClass= <%# DataBinder.Eval(Container.DataItem, "NumberTypeValue").ToString() == "Mobile" 
                                                                              ? "btn btn-primary smsbutton" 
                                                                              : "btn btn-default smsbutton" %> >
                                                <i class="fa fa-comment-alt" aria-hidden="true"></i> 

                                                <%# DataBinder.Eval(Container.DataItem, "NumberFormatted" ) %> 
                                                <small><%# DataBinder.Eval(Container.DataItem, "NumberTypeValue" ) %></small>

                                            </asp:LinkButton>
                                            
                                            
                                            <span runat="server" id="sNumber">
                                                <%# DataBinder.Eval(Container.DataItem, "NumberFormatted" ) %> 
                                                <small><%# DataBinder.Eval(Container.DataItem, "NumberTypeValue" ) %></small>
                                            </span>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                
                                </li>
                        </ItemTemplate>
                    </asp:Repeater>
                    </ul>
                </div>
            </Content>
        </Rock:ModalDialog>
        
        <Rock:ModalDialog ID="dlgSMS" Title="Contact Parent" SaveButtonText="Cancel" OnCancelScript="clearActiveDialog();" ValidationGroup="SMSParent" runat="server" >
            <Content>
                                
                <asp:Repeater runat="server" id="rMessages" OnItemCommand="rMessages_OnItemCommand">
                    <ItemTemplate>
                        <Rock:BootstrapButton ID="lbSMS" runat="server" CommandArgument='<%# Container.DataItem.ToString() %>' CssClass="btn btn-primary btn-lg btn-block btn-checkin-select"  DataLoadingText="Loading..." ><i class="fa fa-comment-alt" aria-hidden="true"></i> <%# Container.DataItem.ToString() %></Rock:BootstrapButton>
                    </ItemTemplate>
                </asp:Repeater>
                
                <asp:HiddenField runat="server" id="hfPhoneNumber"/>
                <asp:HiddenField runat="server" ID="hfSMSAliasId"/>
            </Content>
            
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgOtherLocation" Title="Move Attendance" CancelLinkVisible="true" SaveButtonText="Move" runat="server" OnOkScript="" >
             
            <Content>
                Are you sure you would like to move this child to your classroom?
                
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalAlert ID="maWarning" runat="server" />
        

    </ContentTemplate>
</asp:UpdatePanel>
