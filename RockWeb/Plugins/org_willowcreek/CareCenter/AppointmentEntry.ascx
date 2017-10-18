<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AppointmentEntry.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.AppointmentEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <script>
            Sys.Application.add_load( function () {
                $("div.photo-round").lazyload({
                    effect: "fadeIn"
                });
            });
        </script>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar-plus-o"></i> Service Schedule</h1>
            </div>

            <div class="panel-body">

                <h2>
                    <asp:Literal ID="lPhoto" runat="server" />
                    <asp:Literal ID="lPrimaryContact" runat="server" />
                </h2>

                <Rock:NotificationBox ID="nbMessage" runat="server" visible="false" />

                <asp:Panel ID="pnlSearch" runat="server">

                    <div class="row margin-b-lg">
                        <div class="col-md-4">
                            <h3 class="margin-t-lg"><strong><asp:Literal ID="lHeading" runat="server"></asp:Literal></strong> <asp:Literal ID="lSubHeading" runat="server"></asp:Literal></h3>
                        </div>

                        <div class="col-md-8">

                            <div class="row">
                                <div class="col-sm-4">
                                    <Rock:RockDropDownList ID="ddlServiceArea" runat="server" Label="Service Area" AutoPostBack="true" OnSelectedIndexChanged="ddlServiceArea_SelectedIndexChanged" />
                                </div>
                                <div class="col-sm-5">
                                    <asp:PlaceHolder ID="phDaySelection" runat="server">
                                        <div class="form-group">
                                            <label class="control-label">&nbsp;</label>
                                            <div class="input-group">
                                                <span class="input-group-btn">
                                                    <asp:LinkButton ID="lbPrevDay" runat="server" CssClass="btn btn-default" OnClick="lbPrevDay_Click" ><i class="fa fa-chevron-left"></i></asp:LinkButton>
                                                </span>
                                                <asp:TextBox ID="tbDay" runat="server" Text="Today" CssClass="form-control" Enabled="false"></asp:TextBox>
                                                <span class="input-group-btn">
                                                    <asp:LinkButton ID="lbNextDay" runat="server" CssClass="btn btn-default" OnClick="lbNextDay_Click"><i class="fa fa-chevron-right"></i></asp:LinkButton>
                                                </span>
                                            </div>
                                        </div>
                                    </asp:PlaceHolder>
                                </div>
                                <div class="col-sm-3">
                                    <Rock:Toggle ID="tglView" runat="server" Label="&nbsp;" OnText="<i class='fa fa-calendar-o'></i>" OffText="<i class='fa fa-calendar'></i>" OnCheckedChanged="tglView_CheckedChanged" />
                                </div>
                            </div>

                        </div>

                    </div>

                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" Visible="false" Heading="Warning" NotificationBoxType="Warning" />

                    <asp:Panel ID="pnlDayView" runat="server" Visible="false" >
                        <asp:HiddenField ID="hfDay" runat="server" />

                        <div class="rowlist rowlist-striped">
                        <asp:Repeater ID="rptTimeSlot" runat="server" OnItemDataBound="rptTimeSlot_ItemDataBound">
                            <ItemTemplate>
                                <div class="row">
                                    <div class="col-sm-8">
                                        <h4 class="margin-v-none"><asp:Label ID="lblTime" runat="server"></asp:Label></h4>
                                        <asp:Label ID="lblTimeSlotDetails" runat="server" />
                                    </div>
                                    <div class="col-sm-4">
                                        <asp:LinkButton ID="lbScheduleAppt" runat="server" CssClass="btn btn-default pull-right" Text="Schedule Appointment" CommandName="Schedule" CommandArgument='<%# ((DateTime)Eval("ApptTime")).ToShortDateString() + "|" + Eval("TimeSlot.Id") %>' OnCommand="lbScheduleAppt_Command"></asp:LinkButton>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        </div>

                        <Rock:NotificationBox ID="nbNoAppointments" runat="server" NotificationBoxType="Info" Text="There are not any appointments available for the selected day." Visible="false" />

                        <div class="actions clearfix">
                            <asp:LinkButton ID="lbNextAvailable" runat="server" CssClass="btn btn-default pull-right" Text="Next Available" OnClick="lbNextAvailable_Click"></asp:LinkButton>
                        </div>

                        <div class="margin-t-md rowlist rowlist-striped">
                            <asp:Repeater ID="rptNextAvailable" runat="server" OnItemDataBound="rptTimeSlot_ItemDataBound">
                                <ItemTemplate>
                                    <div class="row">
                                        <div class="col-sm-8">
                                            <h4 class="margin-b-none"><asp:Label ID="lblDateTime" runat="server" CssClass="text-success"></asp:Label></h4>
                                            <asp:Label ID="lblTimeSlotDetails" runat="server" />
                                        </div>
                                        <div class="col-sm-4">
                                            <asp:LinkButton ID="lbScheduleAppt" runat="server" CssClass="btn btn-default pull-right" Text="Schedule Appointment" CommandName="Schedule" CommandArgument='<%# ((DateTime)Eval("ApptTime")).ToShortDateString() + "|" + Eval("TimeSlot.Id") %>' OnCommand="lbScheduleAppt_Command"></asp:LinkButton>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>

                    </asp:Panel>

                    <asp:Panel ID="pnlWeekView" runat="server" Visible="false" >

                        <div class="margin-b-sm clearfix">
                            <asp:LinkButton ID="lbWeekPrev" runat="server" CssClass="btn btn-default pull-right" OnClick="lbWeekPrev_Click"><i class="fa fa-chevron-up"></i></asp:LinkButton>
                        </div>

                        <table class="table table-bordered">
                            <thead>
                                <tr>
                                    <th>Sun</th>
                                    <th>Mon</th>
                                    <th>Tue</th>
                                    <th>Wed</th>
                                    <th>Thu</th>
                                    <th>Fri</th>
                                    <th>Sat</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <asp:PlaceHolder ID="phWeek1" runat="server"></asp:PlaceHolder>
                                </tr>
                                <tr>
                                    <asp:PlaceHolder ID="phWeek2" runat="server"></asp:PlaceHolder>
                                </tr>
                            </tbody>
                        </table>

                        <div class="margin-t-sm clearfix">
                            <asp:LinkButton ID="lbWeekNext" runat="server" CssClass="btn btn-default pull-right" OnClick="lbWeekNext_Click"><i class="fa fa-chevron-down"></i></asp:LinkButton>
                        </div>

                    </asp:Panel>

                </asp:Panel>

                <asp:Panel ID="pnlConfirm" runat="server" Visible="false" >

                    <h3>Confirm Appointment...</h3>

                    <asp:HiddenField ID="hfPreviousView" runat="server" />
                    <asp:HiddenField ID="hfConfirmTimeSlot" runat="server" />
                    <asp:HiddenField ID="hfConfirmDate" runat="server" />

                    <div class="row">
                        <div class="col-sm-4">
                            <Rock:RockLiteral ID="lConfirmServiceArea" runat="server" Label="Service Area" />
                        </div>
                        <div class="col-sm-4">
                            <Rock:RockLiteral ID="lConfirmAppointment" runat="server" Label="Appointment Details" />
                        </div>
                        <div class="col-sm-4">
                            <Rock:RockLiteral ID="lConfirmPerson" runat="server" Label="Person" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbConfirmSchedule" runat="server" CssClass="btn btn-primary" Text="Schedule Appointment" OnClick="lbConfirmSchedule_Click" />
                        <asp:LinkButton ID="lbConfirmCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbConfirmCancel_Click" CausesValidation="false" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlSuccess" runat="server" Visible="false" >

                    <Rock:NotificationBox ID="nbSuccess" runat="server" NotificationBoxType="Success" Title="Appointment Scheduled" />

                    <div class="actions">
                        <asp:LinkButton ID="lbHome" runat="server" CssClass="btn btn-primary" Text="Home" OnClick="lbHome_Click" />
                        <asp:HyperLink ID="hlPrint" runat="server" Target="_blank" CssClass="btn btn-default" ><i class="fa fa-print"></i> Print</asp:HyperLink>
                    </div>

                </asp:Panel>

            </div>

        </asp:Panel>


    </ContentTemplate>
</asp:UpdatePanel>
