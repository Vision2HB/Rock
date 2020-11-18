<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttendanceEntry.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Groups.GroupAttendanceEntry" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>
        <div class="row" style="padding-bottom: 15px;">
            <h2>
                <div class="col-xs-6">
                    <asp:Literal ID="lChurchHeading" runat="server" Text="Church Name" />
                </div>
                <div class="pull-right">
                    <Rock:RockLiteral ID="lOccurrenceDate" runat="server" />
                    <Rock:DatePicker ID="dpOccurrenceDate" runat="server" AllowFutureDateSelection="false" Required="true" />
                </div>
                <div class="pull-right" style="padding-right: 5px;">
                    <asp:Literal ID="lGroupHeading" runat="server" Text="Group Attendance" />
                </div>
            </h2>
        </div>

        <div class="panel panel-block">
            <div class="panel-body">

                <Rock:NotificationBox ID="nbNotice" runat="server" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <asp:CustomValidator ID="cvAttendance" runat="server" Display="None" />

                <asp:Panel ID="pnlDetails" runat="server">
                    <div class="row">
                        <div class="col-xs-6">
                            <Rock:RockTextBox ID="tbSearch" runat="server" AutoPostBack="true" OnTextChanged="tbSearch_TextChanged" />
                            <Rock:RockCheckBox ID="cbDidNotMeet" runat="server" Text="We Did Not Meet" />
                        </div>

                        <div class="col-xs-6 pull-right">
                            <Rock:Toggle ID="tglSort" runat="server" OnText="Last Name" OnCssClass="btn-primary" OffCssClass="btn-outline-primary" ActiveButtonCssClass="btn-primary" ButtonSizeCssClass="btn-xs" OffText="First Name" AutoPostBack="true" OnCheckedChanged="tglSort_CheckedChanged" Checked="true" Label="Sort by" />
                        </div>

                    </div>
                    <div class="js-roster">
                        <div class="panel-labels clearfix">
                            <h4 class="js-members-label">
                                <asp:Literal ID="lMembers" runat="server" />
                            </h4>
                        </div>
                        <asp:ListView ID="lvMembers" runat="server" OnItemDataBound="lvMembers_ItemDataBound">
                            <ItemTemplate>
                                <asp:HiddenField ID="hfMember" runat="server" />

                                <div class="row">
                                    <div class="col-xs-6">
                                        <asp:Literal ID="lMember" runat="server" />
                                    </div>
                                    <div class="pull-right">
                                        <Rock:RockRadioButtonList ID="rblAttendance" runat="server" RepeatDirection="Horizontal" />
                                    </div>
                                    <div class="pull-right" style="padding-right: 5px;">

                                        <asp:LinkButton ID="lbMemberNote" runat="server" OnCommand="lbMemberNote_Command"><i class="fa fa-file"></i></asp:LinkButton>
                                    </div>
                                </div>

                            </ItemTemplate>
                        </asp:ListView>
                        <div class="pull-left margin-b-md margin-r-md">
                            <Rock:PersonPicker ID="ppAddPerson" runat="server" OnSelectPerson="ppAddPerson_SelectPerson" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="dtNotes" runat="server" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" SourceTypeName="Rock.Model.AttendanceOccurrence, Rock" PropertyName="Notes"></Rock:DataTextBox>
                        </div>
                    </div>


                    <Rock:NotificationBox ID="nbPrintRosterWarning" runat="server" NotificationBoxType="Warning" />

                    <div class="actions" style="position: fixed;">
                        <asp:LinkButton ID="lbClearSearch" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Clear Search" CssClass="btn btn-primary" OnClick="lbSave_Click" CausesValidation="false" />
                        <asp:LinkButton ID="lbAddPerson" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Add Person" CssClass="btn btn-primary" OnClick="lbSave_Click" CausesValidation="false" />
                        <asp:LinkButton ID="lbSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" CausesValidation="false" />
                    </div>

                </asp:Panel>

            </div>

        </div>

        <Rock:ModalDialog ID="mdOccurrenceAttendanceType" runat="server" ValidationGroup="Value" CancelLinkVisible="false" CloseLinkVisible="false" OnSaveClick="mdOccurrenceAttendanceType_SaveClick" SaveButtonCssClass="btn btn-primary" SaveButtonText="Start">
            <Content>
                <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Value" />
                <fieldset>
                    <asp:RadioButtonList ID="rblOccurrenceAttendanceType" runat="server">
                        <asp:ListItem Value="In-person" Text="Met In Person" />
                        <asp:ListItem Value="Virtual" Text="Met Online" />
                        <asp:ListItem Value="Mixed" Text="Mixed" />
                        <asp:ListItem Value="DidNotMeet" Text="Did Not Meet" />
                    </asp:RadioButtonList>
                </fieldset>
            </Content>
        </Rock:ModalDialog>

        <script>
            Sys.Application.add_load(function () {
                // toggle all checkboxes
                $('.js-members-label').on('click', function (e) {

                    var container = $(this).parent();
                    var isChecked = container.hasClass('all-checked');

                    container.find('input:checkbox').each(function () {
                        $(this).prop('checked', !isChecked);
                    });

                    if (isChecked) {
                        container.removeClass('all-checked');
                    }
                    else {
                        container.addClass('all-checked');
                    }

                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
