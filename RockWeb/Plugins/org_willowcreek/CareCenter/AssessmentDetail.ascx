<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssessmentDetail.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.AssessmentDetail" %>

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
            <asp:HiddenField ID="hfAssessmentId" runat="server" />
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i> Assessment Detail</h1>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="nbServerValidation" runat="server" NotificationBoxType="Danger" Visible="false" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppPrimaryContact" runat="server" Label="Primary Contact" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DateTimePicker ID="dtAssessmentDate" runat="server" Label="Assessment Date/Time" />
                            <Rock:PersonPicker ID="ppApprovedBy" runat="server" Label="Approved By" Required="true" />
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
                        <div class="col-sm-4">
                            <Rock:RockLiteral ID="lDate" runat="server" Label="Date" />
                            <Rock:RockLiteral ID="lApprovedBy" runat="server" Label="Approved By" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:Grid ID="gWorkflows" runat="server" AllowSorting="false" DisplayType="Light" RowItemText="Assessment" OnRowSelected="gWorkflows_RowSelected" CssClass="wide">
                                <Columns>
                                    <Rock:RockBoundField DataField="WorkflowType.Name" HeaderText="Assessment" />
                                    <Rock:RockTemplateField HeaderText="Status">
                                        <ItemTemplate><asp:Literal ID="lStatus" runat="server" /></ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField HeaderText="History" ItemStyle-CssClass="col-history">
                                        <ItemTemplate><asp:Literal ID="lServiceHistory" runat="server" /></ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:DeleteField OnClick="gWorkflows_Delete" ButtonCssClass="btn btn-sm btn-warning assessment-cancel-button" IconCssClass="fa fa-minus" HeaderText="Cancel"></Rock:DeleteField>
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>

                    <div class="actions margin-t-lg">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit Assessment" CssClass="btn btn-default" CausesValidation="false" OnClick="lbEdit_Click" />
                    </div>

                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
