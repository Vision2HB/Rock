<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VisitDetail.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.VisitDetail" %>

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
            <asp:HiddenField ID="hfVisitId" runat="server" />
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> Visit Detail</h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="nbServerValidation" runat="server" NotificationBoxType="Danger" Visible="false" />

                    <div class="row">
                        <div class="col-sm-6">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:PersonPicker ID="ppPrimaryContact" runat="server" Label="Primary Contact" Required="true" />
                                </div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" AutoPostBack="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlCancelReason" runat="server" Label="Cancel Reason" Visible="false" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:DateTimePicker ID="dtVisitDate" runat="server" Label="Visit Date/Time" />
                            <Rock:NumberBox ID="nbPagerId" runat="server" Label="Pager" NumberType="Integer" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockCheckBox ID="cbPassportPrinted" runat="server" Label="Passport Printed" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>
                </div>

                <div id="pnlViewDetails" runat="server">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    
                    <div class="row">
                        <div class="col-sm-4">
                            <asp:Literal ID="lPhoto" runat="server" />
                            <Rock:RockLiteral ID="lPrimaryContact" runat="server" Label="Primary Contact" />
                        </div>
                        <div class="col-sm-2">
                            <Rock:RockLiteral ID="lPager" runat="server" Label="Pager" />
                        </div>
                        <div class="col-sm-3">
                            <Rock:RockLiteral ID="lDate" runat="server" Label="Date" />
                        </div>
                        <div class="col-sm-3">
                            <Rock:RockLiteral ID="lCreatedBy" runat="server" Label="Created By" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:Grid ID="gServices" runat="server" AllowSorting="false" DisplayType="Light" RowItemText="Service Area" OnRowSelected="gServices_RowSelected" CssClass="wide">
                                <Columns>
                                    <Rock:RockBoundField DataField="WorkflowType.Name" HeaderText="Service" />
                                    <Rock:RockTemplateField HeaderText="Status">
                                        <ItemTemplate><asp:Literal ID="lStatus" runat="server" /></ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField HeaderText="History" ItemStyle-CssClass="col-history">
                                        <ItemTemplate><asp:Literal ID="lServiceHistory" runat="server" /></ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:DeleteField OnClick="gServices_Delete" ButtonCssClass="btn btn-sm btn-warning visit-cancel-button" IconCssClass="fa fa-minus" HeaderText="Cancel"></Rock:DeleteField>
                                </Columns>
                            </Rock:Grid>

                            <Rock:Grid ID="gWorkflows" runat="server" AllowSorting="false" DisplayType="Light" RowItemText="Assessment" OnRowSelected="gServices_RowSelected" CssClass="wide">
                                <Columns>
                                    <Rock:RockBoundField DataField="WorkflowType.Name" HeaderText="Assessment" />
                                    <Rock:RockTemplateField HeaderText="Status">
                                        <ItemTemplate><asp:Literal ID="lStatus" runat="server" /></ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField HeaderText="History" ItemStyle-CssClass="col-history">
                                        <ItemTemplate><asp:Literal ID="lServiceHistory" runat="server" /></ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:DeleteField OnClick="gWorkflows_Delete" ButtonCssClass="btn btn-sm btn-warning visit-cancel-button" IconCssClass="fa fa-minus" HeaderText="Cancel" Tooltip="Cancel"></Rock:DeleteField>
                                </Columns>
                            </Rock:Grid>

                        </div>
                    </div>


                    <div class="actions margin-t-lg">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit Visit" CssClass="btn btn-default" CausesValidation="false" OnClick="lbEdit_Click" />
                        <div class="pull-right">
                            <asp:LinkButton ID="lbCancelVisit" runat="server" CssClass="btn btn-default" Text="Cancel Entire Visit" CausesValidation="false" OnClick="lbCancelVisit_Click"></asp:LinkButton>
                            <asp:HyperLink ID="hlPrintPassport" runat="server" CssClass="btn btn-primary" Text="Print Passport" Target="_blank" CausesValidation="false" />
                        </div>
                    </div>

                </div>

            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgCancelWorkflow" runat="server" Title="Cancel" OnSaveClick="dlgCancelWorkflow_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="CancelWorkflow">
            <Content>
                <asp:HiddenField ID="hfWorkflowId" runat="server" />
                <Rock:RockDropDownList ID="ddlDlgCancelWorkflowReason" runat="server" Label="Cancel Reason" Required="true" ValidationGroup="CancelWorkflow" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgCancel" runat="server" Title="Cancel" OnSaveClick="dlgCancel_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Cancel">
            <Content>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlDlgCancelReason" runat="server" Label="Cancel Reason" Required="true" ValidationGroup="Cancel" />
                    </div>
                    <div class="col-md-8">
                        <Rock:RockTextBox ID="tbCancelNote" runat="server" Label="Note" ValidationGroup="Cancel" TextMode="MultiLine" Rows="3"></Rock:RockTextBox>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
