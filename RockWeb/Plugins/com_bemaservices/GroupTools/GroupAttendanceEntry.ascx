<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttendanceEntry.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.GroupTools.GroupAttendanceEntry" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>
        <div class="container attendance-header">
            <div class="d-flex flex-column flex-md-row justify-content-between margin-h-none padding-v-lg">
                <div class="f-col-12 f-col-md-6 d-flex justify-content-center justify-content-md-start">
                    <h2 class="header-left text-white margin-v-md">
                        <asp:Literal ID="lChurchHeading" runat="server" Text="Church Name" />
                    </h2>
                </div>
                <div class="f-col-12 f-col-md-6 header-right d-flex align-items-center justify-content-center justify-content-md-end group-name">
                    <h2 class="margin-all-none text-white">
                        <asp:Literal ID="lGroupHeading" runat="server" Text="Group Attendance" />
                    </h2>
                    <div class="margin-l-md">
                        <Rock:RockLiteral ID="lOccurrenceDate" runat="server" />
                        <Rock:DatePicker class="date-picker" ID="dpOccurrenceDate" runat="server" AllowFutureDateSelection="false" Required="true" OnSelectDate="dpOccurrenceDate_SelectDate" />
                    </div>
                </div>
            </div>
        </div>

        <div class="container attendance-body">
            <div class="panel panel-block">
                <div class="panel-body">

                    <Rock:NotificationBox ID="nbNotice" runat="server" />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:CustomValidator ID="cvAttendance" runat="server" Display="None" />

                    <asp:Panel ID="pnlDetails" runat="server">
                        <div class="d-flex flex-column flex-md-row align-items-center panel-top">
                            <div class="f-col-12 f-col-md-6 padding-all-md">
                                <Rock:RockTextBox class="searchbar" ID="tbSearch" runat="server" placeholder="Search" />
                            </div>

                            <div class="f-col-12 f-col-md-6 btn-sort-wrapper d-flex align-items-center justify-content-center justify-content-md-end padding-v-md padding-r-md padding-l-none">
                                <p class="margin-b-none margin-r-md text-gray">Sort by</p>
                                <Rock:Toggle ID="tglSort" runat="server" OnText="Last Name" OnCssClass="btn-on" OffCssClass="btn-off" ActiveButtonCssClass="btn-active" ButtonSizeCssClass="btn" OffText="First Name" autopostback="true" OnCheckedChanged="tglSort_CheckedChanged" Checked="true" />
                            </div>

                        </div>
                        <div class="js-roster">
                            <div class="panel-labels clearfix">
                                <h4 class="d-none js-members-label">
                                    <asp:Literal ID="lMembers" runat="server" />
                                </h4>
                            </div>
                            <asp:ListView ID="lvMembers" runat="server" OnItemDataBound="lvMembers_ItemDataBound">
                                <ItemTemplate>
                                    <div class="d-flex flex-column flex-md-row padding-all-md bg-alternate">
                                        <div class="f-col-12 f-col-md-7 d-flex flex-row justify-content-around justify-content-md-between align-items-center padding-all-sm">
                                            <div class="d-flex align-items-center member-row">
                                                <asp:HiddenField ID="hfMember" runat="server" />
                                                <asp:HiddenField ID="hfMemberName" runat="server" />
                                                <asp:Literal ID="lMember" runat="server" />
                                            </div>
                                            <div>
                                                <asp:LinkButton ID="lbMemberNote" runat="server" OnCommand="lbMemberNote_Command" CommandArgument='<%# Eval("PersonId") %>'><i class="fas fa-file-medical icon-gray item-border icon-wrap"></i></asp:LinkButton>
                                            </div>
                                        </div>
                                        <div class="f-col-12 f-col-md-5 d-flex flex-row align-items-center padding-all-sm">
                                            <div class="item-border border-small margin-h-lg">
                                                <Rock:RockRadioButtonList ID="rblAttendance" runat="server" RepeatDirection="Horizontal" CssClass="radio-btn-list" />
                                                <Rock:RockCheckBox ID="cbAttendance" runat="server" Text="Attended" CssClass="radio-btn-list" Visible="false" />
                                            </div>
                                        </div>
                                    </div>

                                </ItemTemplate>
                            </asp:ListView>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </div>
        <nav class="navbar navbar-fixed-bottom navbar-light text-center padding-all-md">
            <asp:LinkButton ID="lbClearSearch" runat="server" Text="Clear Search" CssClass="btn btn-clear margin-all-sm" CausesValidation="false" />
            <asp:LinkButton ID="lbAddPerson" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Add Person" CssClass="btn btn-action margin-all-sm btn-add-person" OnClick="lbAddPerson_Click" CausesValidation="false" />
            <asp:LinkButton ID="lbSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-action margin-all-sm" OnClick="lbSave_Click" CausesValidation="false" />
        </nav>

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

        <Rock:ModalDialog ID="mdMemberNote" runat="server" ValidationGroup="Value" OnSaveClick="mdMemberNote_SaveClick">
            <Content>
                <Rock:NotificationBox ID="nbNote" runat="server" NotificationBoxType="Danger" Text="A Note Type and text are required to create a note." />
                <asp:ValidationSummary ID="ValNote" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="NoteValue" />
                <fieldset>
                    <asp:HiddenField ID="hfPersonId" runat="server" />
                    <div style="text-align: center;">
                        <Rock:RockRadioButtonList ID="rblNoteType" runat="server" RepeatDirection="Horizontal" Required="true" ValidationGroup="NoteValue" />
                    </div>
                    <Rock:RockTextBox ID="tbNote" runat="server" TextMode="MultiLine" Rows="3" Placeholder="Start Typing Here" Required="true" ValidationGroup="NoteValue" />
                </fieldset>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdAddPerson" runat="server" ValidationGroup="Value" OnSaveClick="mdAddPerson_SaveClick">
            <Content>
                <Rock:NotificationBox ID="nbPerson" runat="server" NotificationBoxType="Danger" Text="First Name, Last Name, and Email are required to add a new person." />
                <asp:ValidationSummary ID="ValidationSummary2" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="PersonValue" />
                <fieldset>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbFirstName" runat="server" Required="true" Label="First Name" ValidationGroup="PersonValue" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbLastName" runat="server" Required="true" Label="Last Name" ValidationGroup="PersonValue" />
                        </div>
                    </div>
                    <Rock:EmailBox ID="tbEmail" runat="server" Required="true" Label="Email" ValidationGroup="PersonValue" />
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

                $("[id$='tbSearch']").on('keyup', function (e) {
                    var searchTerm = $(this).val().toLowerCase();
                    $("[id$='hfMemberName']").each(function (index) {
                        var container = $(this).parent().parent().parent();
                        var fullName = $(this).val().toLowerCase();
                        var matchExists = fullName.includes(searchTerm);
                        if (matchExists || searchTerm.length <= 0) {
                            container.attr("style", "display: flex !important");
                        } else {
                            container.attr("style", "display: none !important");
                        }

                    });
                });

                $("[id$='lbClearSearch']").on('click', function (e) {
                    $("[id$='tbSearch']").val("")
                    $("[id$='hfMemberName']").each(function (index) {
                        var container = $(this).parent().parent().parent();
                        container.attr("style", "display: flex !important");
                    });
                });
            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
