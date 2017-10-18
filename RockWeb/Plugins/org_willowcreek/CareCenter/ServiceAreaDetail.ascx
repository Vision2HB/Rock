<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceAreaDetail.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.ServiceAreaDetail" %>

<asp:UpdatePanel ID="pnlServiceAreaUpdatePanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfServiceAreaId" runat="server" />
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIconHtml" runat="server"><i class="fa fa-folder"></i></asp:Literal>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlEditDetails" runat="server">
                    <asp:ValidationSummary ID="valServiceAreaDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server"
                                SourceTypeName="org.willowcreek.CareCenter.Model.ServiceArea, org.willowcreek.CareCenter" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbUsesPassport" runat="server" Label="Uses Passport" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="org.willowcreek.CareCenter.Model.ServiceArea, org.willowcreek.CareCenter" PropertyName="IconCssClass" />
                            <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" RequiredErrorMessage="Category is required" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbWrkFlwSchedule" runat="server" Label="Workflow Allow Scheduling" />
                            <Rock:WorkflowTypePicker ID="wpWorkflowType" runat="server" RequiredErrorMessage="Workflow Type is required" Label="Workflow Type" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="cePassportLava" runat="server" Label="Passport Lava" EditorMode="Html" EditorTheme="Rock" EditorHeight="400" RequiredErrorMessage="Passport Lava is required"
                                Help="Additional HTML content to include with the Service Area." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceIntakeLava" runat="server" Label="Intake Lava" EditorMode="Html" EditorTheme="Rock" EditorHeight="400" RequiredErrorMessage="Intake Lava is required"
                                Help="Additional HTML content to include with the Service Area." />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
                <fieldset id="fieldsetViewSummary" runat="server">
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lDetailsLeft" runat="server" />
                        </div>
                        <%--  <div class="col-md-6">
                                <asp:Literal ID="lDetailsRight" runat="server" />
                            </div>--%>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                        <asp:LinkButton ID="lbTimeSlots" runat="server" CssClass="btn btn-default btn-sm pull-right" CausesValidation="false" ToolTip="Appointment Time Slots" OnClick="lbTimeSlots_Click"><i class="fa fa-calendar"></i></asp:LinkButton>
                    </div>
                </fieldset>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
