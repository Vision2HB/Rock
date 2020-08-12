<%@ Control Language="C#" AutoEventWireup="true"  CodeFile="GroupOccurrenceTool.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.GroupOccurrence.GroupOccurrenceTool" %>

<head>
    <style>
        .grid-container {
            display: grid;
            grid-gap: 10px;
            padding: 10px;
        }

            .grid-container > div {
                padding: 20px 0;
            }
    </style>

</head>



<asp:UpdatePanel ID="upnlContent" runat="server" ViewStateMode="Enabled" EnableViewState="true" >
    <ContentTemplate>
        <asp:HiddenField ID="hfGroupTypesInclude" runat="server" />
        <asp:HiddenField ID="hfGroupTypesExclude" runat="server" />
        <asp:HiddenField ID="hfSelectedServiceTimes" runat="server" />
        <asp:HiddenField ID="hfSelectedLocations" runat="server" />
        <asp:HiddenField ID="hfGroupIds" runat="server" />
        <div class="grid-container">
            
            <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block" >

                <div class="panel-body" style="width: 100%">

                    <div class="row">

                        <div class="col-lg-6">
                            <div class="row">
                                <div class="col-lg-6">
                                    <Rock:CampusPicker runat="server" ID="r_CampusPicker" AutoPostBack="true" OnSelectedIndexChanged="r_CampusPicker_SelectedIndexChanged" Required="true"/>

                                    <Rock:RockListBox AutoPostBack="true" runat="server" ID="rddl_ServiceTimeSelector" Help="This selector works only with a Campus attribute on the Service Hour entity."
                                        Label="Service Hour" DataValueField="Value" DataTextField="Text"
                                        OnSelectedIndexChanged="rddl_ServiceTimeSelector_SelectedIndexChanged" 
                                        SelectionMode="Multiple" Required="true" />

                                </div>
                                <div class="col-lg-6">
                                    <Rock:RockDropDownList runat="server" ID="dvpSchoolYear"
                                        OnSelectedIndexChanged="dvpSchoolYear_SelectedIndexChanged" 
                                        AutoPostBack="true" Label="School Year" DataValueField="Value" 
                                        DataTextField="Text" Required="true" />

                                    <Rock:RockListBox AutoPostBack="true" runat="server" ID="rddl_ClassSelector" Help="This selector is driven from the service time.  It gets the locations/classrooms that meet at the selected service times. Leave blank for all locations."
                                        Label="Locations" DataValueField="Value" DataTextField="Text"
                                         OnSelectedIndexChanged="rddl_ClassSelector_SelectedIndexChanged"
                                        SelectionMode = "Multiple" />

                                    <Rock:RockListBox AutoPostBack="true" runat="server" ID="rddl_ParentGroups" 
                                        Label="Parent Groups" DataValueField="Value" DataTextField="Text"
                                        
                                        SelectionMode = "Multiple" />

                                    <Rock:RockListBox AutoPostBack="true" runat="server" ID="rddl_Groups" 
                                        Label="Groups" DataValueField="Value" DataTextField="Text"
                                        
                                        SelectionMode = "Multiple" />

                                </div>
                            </div>
                        </div>
                        <div class="col-lg-6">
                            <div class="row">
                                <div class="col-lg-12">
                                    <Rock:SlidingDateRangePicker runat="server" ID="dpOccurrenceDates" Label="Occurrence Date" Required="true"
                                        EnabledSlidingDateRangeTypes="Previous, Last, Current, Next, Upcoming, DateRange"
                                        EnabledSlidingDateRangeUnits="Day, Week, Month" />
                                </div>
                            </div>
                            
                        </div>
                    </div>
                    <div class="row">
                        <div class="actions col-sm-12">
                            <Rock:BootstrapButton runat="server" ID="rbb_Filter" CssClass="btn btn-primary" Text="Filter" OnClick="rbb_Filter_Click" />
                            <Rock:BootstrapButton runat="server" ID="btnMarkDidNotMeet" Text="Mark as 'Did Not Meet'" CssClass="btn btn-danger pull-right" OnClick="btnMarkDidNotMeet_Click" />
                            
                        </div>
                    </div>
                    <hr />
                    <div class="row">
                        <div class="col-md-12">
                            <div class="grid grid-panel">
                                <Rock:Grid ID="grdGroups" runat="server" AllowSorting="true" DataKeyNames="GroupId" OnRowSelected="refreshUrl_Click" ShowActionRow="false" >
                                    <Columns>
                                        <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                                        <Rock:RockBoundField DataField="ParentGroupName" HeaderText="Parent Group Name" />
                                        <Rock:RockBoundField DataField="GroupName" HeaderText="Group Name" SortExpression="Name" />
                                        <Rock:RockBoundField DataField="MeetingLocation" HeaderText="Class Rooms" />
                                        <Rock:RockBoundField DataField="ScheduledList" HeaderText="Service Times" />
                                        <Rock:RockBoundField DataField="Attendance" HeaderText="Attendance" />
                                        <Rock:RockBoundField DataField="Notes" HeaderText="Occurrences" HtmlEncode="False" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="panel-footer" style="padding-top: 30px">
                </div>
            </asp:Panel>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
