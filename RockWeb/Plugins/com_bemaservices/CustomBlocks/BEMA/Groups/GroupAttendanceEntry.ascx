<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttendanceEntry.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Groups.GroupAttendanceEntry" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>
        <div class="row" style="padding-bottom: 15px;">
            <h2>
                <div class="col-xs-6">
                    <asp:Literal ID="lChurchHeading" runat="server" Text="Church Name" />
                </div>
                <div class="pull-right">
                    <rock:rockliteral id="lOccurrenceDate" runat="server" />
                    <rock:datepicker id="dpOccurrenceDate" runat="server" allowfuturedateselection="false" required="true" />
                </div>
                <div class="pull-right" style="padding-right: 5px;">
                    <asp:Literal ID="lGroupHeading" runat="server" Text="Group Attendance" />
                </div>
            </h2>
        </div>

        <div class="panel panel-block">
            <div class="panel-body">

                <rock:notificationbox id="nbNotice" runat="server" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <asp:CustomValidator ID="cvAttendance" runat="server" Display="None" />

                <asp:Panel ID="pnlDetails" runat="server">
                    <div class="row">
                        <div class="col-xs-6">
                            <rock:rocktextbox id="tbSearch" runat="server" autopostback="true" ontextchanged="tbSearch_TextChanged" />
                            <rock:rockcheckbox id="cbDidNotMeet" runat="server" text="We Did Not Meet" />
                        </div>

                        <div class="col-xs-6 pull-right">
                            <rock:toggle id="tglSort" runat="server" ontext="Last Name" oncssclass="btn-primary" offcssclass="btn-outline-primary" activebuttoncssclass="btn-primary" buttonsizecssclass="btn-xs" offtext="First Name" autopostback="true" oncheckedchanged="tglSort_CheckedChanged" checked="true" label="Sort by" />
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
                                <div class="row">
                                    <asp:HiddenField ID="hfMember" runat="server" />
                                    <asp:HiddenField ID="hfMemberName" runat="server" />
                                    <div class="col-xs-6">
                                        <asp:Literal ID="lMember" runat="server" />
                                    </div>
                                    <div class="pull-right">
                                        <rock:rockradiobuttonlist id="rblAttendance" runat="server" repeatdirection="Horizontal" />
                                    </div>
                                    <div class="pull-right" style="padding-right: 5px;">
                                        <asp:LinkButton ID="lbMemberNote" runat="server" OnCommand="lbMemberNote_Command" CommandArgument='<%# Eval("PersonId") %>'><i class="fa fa-file"></i></asp:LinkButton>
                                    </div>
                                </div>

                            </ItemTemplate>
                        </asp:ListView>
                        <div class="pull-left margin-b-md margin-r-md">
                            <rock:personpicker id="ppAddPerson" runat="server" onselectperson="ppAddPerson_SelectPerson" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <rock:datatextbox id="dtNotes" runat="server" textmode="MultiLine" rows="3" validaterequestmode="Disabled" sourcetypename="Rock.Model.AttendanceOccurrence, Rock" propertyname="Notes"></rock:datatextbox>
                        </div>
                    </div>

                    <div class="actions" style="position: fixed;">
                        <asp:LinkButton ID="lbClearSearch" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Clear Search" CssClass="btn btn-primary" OnClick="lbSave_Click" CausesValidation="false" />
                        <asp:LinkButton ID="lbAddPerson" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Add Person" CssClass="btn btn-primary" OnClick="lbSave_Click" CausesValidation="false" />
                        <asp:LinkButton ID="lbSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" CausesValidation="false" />
                    </div>

                </asp:Panel>

            </div>

        </div>

        <rock:modaldialog id="mdOccurrenceAttendanceType" runat="server" validationgroup="Value" cancellinkvisible="false" closelinkvisible="false" onsaveclick="mdOccurrenceAttendanceType_SaveClick" savebuttoncssclass="btn btn-primary" savebuttontext="Start">
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
        </rock:modaldialog>

        <rock:modaldialog id="mdMemberNote" runat="server" validationgroup="Value" onsaveclick="mdMemberNote_SaveClick">
            <Content>
                <asp:ValidationSummary ID="ValNote" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="NoteValue" />
                <fieldset>
                    <asp:HiddenField ID="hfPersonId" runat="server" />
                    <div style="text-align: center;">
                        <Rock:RockRadioButtonList ID="rblNoteType" runat="server" RepeatDirection="Horizontal" Required="true" ValidationGroup="NoteValue"  />
                    </div>
                    <Rock:RockTextBox ID="tbNote" runat="server" TextMode="MultiLine" Rows="3" Placeholder="Start Typing Here" Required="true" ValidationGroup="NoteValue"  />
                </fieldset>
            </Content>
        </rock:modaldialog>

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

                $("[id$='tbSearch']").on('keyup', function (e) {
                    var searchTerm = $(this).val().toLowerCase();
                    $("[id$='hfMemberName']").each(function (index) {
                        var container = $(this).parent();
                        var fullName = $(this).val().toLowerCase();
                        var matchExists = fullName.includes(searchTerm);
                        if (matchExists || searchTerm.length <= 0) {
                            container.show();
                        } else {
                            container.hide();
                        }

                    });
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
