<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AppointmentDetail.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.AppointmentDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

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
            <asp:HiddenField ID="hfAppointmentId" runat="server" />
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i> Appointment Detail</h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <asp:Panel id="pnlEditDetails" runat="server" Visible="false">

                    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="nbServerValidation" runat="server" NotificationBoxType="Danger" Visible="false" />

                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:PersonPicker ID="ppPrimaryContact" runat="server" Label="Primary Contact" Required="true" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>

                </asp:Panel>

                <div id="pnlViewDetails" runat="server">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <asp:Panel ID="pnlUpdateMessage" runat="server" CssClass="alert alert-warning" Visible="false">
                        <asp:Label ID="lblUpdateMessage" runat="server" /><br /><br />
                        <asp:HyperLink ID="hlReschedule" runat="server" Text="Re-schedule Appointment" />
                    </asp:Panel>                    

                    <div class="row">
                        <div class="col-sm-4">
                            <asp:Literal ID="lPhoto" runat="server" />
                            <Rock:RockLiteral ID="lPrimaryContact" runat="server" Label="Primary Contact" />
                            <Rock:RockLiteral ID="lServiceArea" runat="server" Label="Service Area" />
                        </div>
                        <div class="col-sm-4">
                            <Rock:RockLiteral ID="lDate" runat="server" Label="Date" />
                            <Rock:RockLiteral ID="lTimeSlot" runat="server" Label="Time" />
                        </div>
                        <div class="col-sm-4">
                            <Rock:RockLiteral ID="lMobilePhone" runat="server" Label="Mobile Phone" />
                            <Rock:RockLiteral ID="lHomePhone" runat="server" Label="Home Phone" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-link" CausesValidation="false" OnClick="lbEdit_Click" />
                        <div class="pull-right">
                            <asp:HyperLink ID="hlPrint" runat="server" Target="_blank" CssClass="btn btn-default" ><i class="fa fa-print"></i> Re-Print Confirmation</asp:HyperLink>
                            <asp:LinkButton ID="lbCancelAppointment" runat="server" CssClass="btn btn-default" Text="Cancel Appointment" CausesValidation="false" OnClick="lbCancelAppointment_Click"></asp:LinkButton>
                            <asp:LinkButton ID="lbNoShow" runat="server" CssClass="btn btn-default" Text="No Show" CausesValidation="false" OnClick="lbNoShow_Click"></asp:LinkButton>
                            <asp:LinkButton ID="lbStartVisit" runat="server" CssClass="btn btn-primary" Text="Start Visit" CausesValidation="false" OnClick="lbStartVisit_Click"></asp:LinkButton>
                        </div>
                    </div>

                </div>

            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgStartVisit" runat="server" Title="Start Visit" OnSaveClick="dlgStartVisit_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="StartVisit">
            <Content>
                <div class="row">
                    <div class="col-md-4 cols-sm-12">
                        <Rock:NumberBox ID="nbPagerNumber" runat="server" Label="Pager Number" Required="true" ValidationGroup="StartVisit" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
