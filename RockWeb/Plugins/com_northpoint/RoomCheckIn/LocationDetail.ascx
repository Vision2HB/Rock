<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LocationDetail.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.RoomCheckIn.LocationDetail" %>

<meta name="viewport" content="width=device-width, initial-scale=1">

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
        $('#<%=hfPhoneNumber.ClientID %>').val('');
        $('#<%=hfSMSAliasId.ClientID %>').val('');
        var timer1 = $find('<%= timer.ClientID %>');
        timer1._startTimer();
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
    .smsbutton {
        margin-top: 5px;
        margin-bottom: 5px;
        text-align: left;
    }
    .grid-columncommand .btn {
        width: auto;
        padding: 10px 16px;
    }
    /*div#header-fixed {
        position:fixed; 
        top:0px; 
        margin:auto; 
        z-index:100000; 
        width:100%;
    }
    div.tab-content {
        margin-top:150px;
    }*/

    
</style>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:Timer ID="timer" runat="server" OnTick="timer_Tick" />
        <asp:HiddenField ID="hfActiveTab" runat="server" />
        <asp:HiddenField ID="hfActiveDialog" runat="server" />
        <asp:HiddenField ID="hfAttendanceId" runat="server" />
        <asp:HiddenField ID="hfPersonId" runat="server" />
        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfGroupTypeId" runat="server" />
        <asp:HiddenField ID="hfCampusId" runat="server" />
        <asp:HiddenField ID="hfEnRouteId" runat="server" />
        <asp:HiddenField ID="hfStaying" runat="server" Value="False" />
        <asp:HiddenField runat="server" id="hfMessages"/>
        <asp:HiddenField runat="server" id="hfFromNumber"/>
        <asp:HiddenField ID="hfMoveAttendanceId" runat="server" />


        <div id="header-fixed">
            <h1 class="text-center">
                <asp:Literal ID="lLocation" runat="server" />
            </h1>
            <ul class="nav nav-pills center-pills">
                <li id="liEnRoute" runat="server" class="active">
                    <asp:LinkButton ID="pillEnRoute" OnClick="pillEnRoute_Click" runat="server"><i class="fa fa-address-card" aria-hidden="true"></i> En Route</asp:LinkButton>
                </li>
                <li id="liInRoom" runat="server">
                    <asp:LinkButton ID="pillInRoom" OnClick="pillInRoom_Click" runat="server"><i class="fa fa-sign-in" aria-hidden="true"></i> In Room</asp:LinkButton>
                </li>
                <li id="liHealthNotes" runat="server">
                    <asp:LinkButton ID="pillHealthNotes" OnClick="pillHealthNotes_Click" runat="server"><i class="fa fa-exclamation-triangle" aria-hidden="true"></i> Notes</asp:LinkButton>
                </li>
                <li id="liCheckedOut" runat="server">
                    <asp:LinkButton ID="pillCheckedOut" OnClick="pillCheckedOut_Click" runat="server"><i class="fa fa-sign-out" aria-hidden="true"></i> Checked Out</asp:LinkButton>
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

        <div class="tab-content">
            <div id="divEnRoute" runat="server" class="tab-pane active margin-b-lg">
                <Rock:Grid ID="gEnRoute" OnGridRebind="gEnRoute_GridRebind" runat="server" DataKeyNames="Id,PersonId,GroupId,GroupTypeId" DisplayType="Light" AllowSorting="false"
                    EmptyDataText="Waiting for Check-Ins..." Font-Size="Large" ShowHeader="true" AllowPaging="false" OnRowSelected="gEnRoute_Edit">
                    <Columns>
                        <Rock:LinkButtonField CssClass="btn btn-lg btn-primary fa fa-sign-in" Text=" Check In" OnClick="gEnRoute_CheckIn" HeaderStyle-HorizontalAlign="Center" />
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Code" HeaderText="Code" SortExpression="Code" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="Group" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="SpecialNote" HeaderText="" HtmlEncode="false" HtmlEncodeFormatString="false" ItemStyle-CssClass="text"/>
                        <Rock:LinkButtonField CssClass="btn btn-lg btn-default fa fa-sign-out" Text=" Check Out" OnClick="gEnRoute_CheckOut" HeaderStyle-HorizontalAlign="Center" />
                    </Columns>
                </Rock:Grid>
                <div class="container text-center">
                    <asp:Repeater ID="rptEnRouteDismiss" runat="server" >
                        <HeaderTemplate>
                            <div class="btn-group text-center">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <Rock:BootstrapButton ID="btnCheckOut" runat="server" CssClass="btn btn-default" Text='<%# Eval("Name", "Clear Out En Route: " + Eval("Name")) %>' CommandArgument='<%# Eval("Id") %>' OnClick="btnEnRouteDismiss_Click" />
                        </ItemTemplate>
                        <FooterTemplate>
                            </div>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
                <br />
                <h4 class="text-center">Other Location Check-Ins:</h4>
                <Rock:RockTextBox ID="tbSearchOther" runat="server" Label="Search" OnTextChanged="tbSearchOther_TextChanged" />
                <Rock:Grid ID="gEnRouteOther" OnGridRebind="gEnRoute_GridRebind" runat="server" DataKeyNames="Id,PersonId,GroupId,GroupTypeId" DisplayType="Light" AllowSorting="false"
                    EmptyDataText=" " Font-Size="Medium" ShowHeader="false" AllowPaging="false" OnRowSelected="gEnRoute_Edit" >
                    
                    <Columns>
                        <Rock:LinkButtonField CssClass="btn btn-default fa fa-sign-in" Text=" Check In" OnClick="gEnRouteOther_CheckIn" HeaderStyle-HorizontalAlign="Center" />
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Code" HeaderText="Code" SortExpression="Code" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="Group" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="SpecialNote" HeaderText="" HtmlEncodeFormatString="false" HtmlEncode="false" ItemStyle-CssClass="text" />
                        <Rock:LinkButtonField CssClass="btn btn-default fa fa-sign-out" Text=" Check Out" OnClick="gEnRoute_CheckOut" HeaderStyle-HorizontalAlign="Center" />
                    </Columns>
                </Rock:Grid>
            </div>
            <div id="divInRoom" runat="server" class="tab-pane">
                <Rock:Grid ID="gInRoom" OnGridRebind="gInRoom_GridRebind" runat="server" DataKeyNames="Id,PersonId,GroupId,GroupTypeId" DisplayType="Light" AllowSorting="false"
                    EmptyDataText="This room is empty" Font-Size="Large" ShowHeader="true" AllowPaging="false" OnRowSelected="gInRoom_Edit">
                    <Columns>
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Code" HeaderText="Code" SortExpression="Code" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="Group" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="SpecialNote" HeaderText="" HtmlEncodeFormatString="false" HtmlEncode="false" ItemStyle-CssClass="text" />
                        <Rock:RockTemplateField>
                            <ItemTemplate>
                                <Rock:BootstrapButton ID="btnStaying" Visible='<%# hfStaying.Value == "True" %>' CssClass='<%# Eval("Staying").ToString() + " btn btn-lg fa fa-clock-o" %>' runat="server" CommandArgument='<%# Eval("Id") %>' OnClick="btnStaying_Click" Text=" Staying" />
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:LinkButtonField ID="btnInRoomEnRoute" CssClass="btn btn-lg btn-default fa fa-address-card" Text=" En Route" OnClick="gInRoom_EnRoute" HeaderStyle-HorizontalAlign="Center" />
                        <Rock:LinkButtonField CssClass="btn btn-lg btn-default fa fa-sign-out" Text=" Check Out" OnClick="gInRoom_CheckOut" HeaderStyle-HorizontalAlign="Center" />
                    </Columns>
                </Rock:Grid>
                <div class="container text-center">
                    <asp:Repeater ID="rptCheckOutBySchedule" runat="server" >
                        <HeaderTemplate>
                            <div class="btn-group text-center">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <Rock:BootstrapButton ID="btnCheckOut" runat="server" CssClass="btn btn-default" Text='<%# Eval("Name", "Check Out All: " + Eval("Name")) %>' CommandArgument='<%# Eval("Id") %>' OnClick="btnCheckOut_Click" />
                        </ItemTemplate>
                        <FooterTemplate>
                            </div>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
            <div id="divHealthNotes" runat="server" class="tab-pane">
                <Rock:Grid ID="gHealthNotes" OnGridRebind="gHealthNotes_GridRebind" runat="server" DataKeyNames="Id,PersonId,GroupId,GroupTypeId" DisplayType="Light" AllowSorting="false"
                    EmptyDataText="None" Font-Size="Large" ShowHeader="true" AllowPaging="false" OnRowSelected="gHealthNotes_Edit">
                    <Columns>
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Code" HeaderText="Code" SortExpression="Code" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="Group" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="SpecialNote" HeaderText="" HtmlEncodeFormatString="false" HtmlEncode="false" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Note" HeaderText="Allergy / Health Note" ItemStyle-CssClass="text" />
                    </Columns>
                </Rock:Grid>
            </div>
            <div id="divCheckedOut" runat="server" class="tab-pane">
                <Rock:Grid ID="gCheckedOut" OnGridRebind="gCheckedOut_GridRebind" runat="server" DataKeyNames="Id,PersonId,GroupId,GroupTypeId" DisplayType="Light" AllowSorting="false"
                    EmptyDataText="None" Font-Size="Large" ShowHeader="true" AllowPaging="false" OnRowSelected="gCheckedOut_Edit">
                    <Columns>
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Code" HeaderText="Code" SortExpression="Code" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="Group" ItemStyle-CssClass="text" />
                        <Rock:RockBoundField DataField="SpecialNote" HeaderText="" HtmlEncodeFormatString="false" HtmlEncode="false" ItemStyle-CssClass="text" />
                        <Rock:LinkButtonField ID="btnCheckedOutEnRoute" CssClass="btn btn-lg btn-default fa fa-building-o" Text=" En Route" OnClick="gCheckedOut_EnRoute" HeaderStyle-HorizontalAlign="Center" />
                        <Rock:LinkButtonField CssClass="btn btn-lg btn-default fa fa-sign-in" Text=" Check In" OnClick="gCheckedOut_CheckIn" HeaderStyle-HorizontalAlign="Center" />
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
                <%--<Rock:RockDropDownList ID="ddlArea" runat="server" Label="Grade" Required="true" DataValueField="id" DataTextField="Name" AutoPostBack="true" OnSelectedIndexChanged="ddlArea_SelectedIndexChanged" Help="If the desired grade is not listed, move the person to En Route first." />--%>
                <%--<Rock:RockDropDownList ID="ddlSmallGroup" runat="server" Label="Small Group" Required="true" DataValueField="id" DataTextField="Name" />--%>
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

        <Rock:ModalDialog ID="dlgOtherLocation" Title="Move Attendance" CancelLinkVisible="true" SaveButtonText="Move" OnSaveClick="dlgOtherLocation_SaveClick" runat="server" >
             
            <Content>
                Are you sure you would like to move this child to your classroom?
                
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalAlert ID="maWarning" runat="server" />
        

    </ContentTemplate>
</asp:UpdatePanel>
