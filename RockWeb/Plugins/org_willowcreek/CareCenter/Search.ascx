<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.Search" %>

<script>
    Sys.Application.add_load(function () {
        $('.js-toggle-advanced-search').on('click', function () {
            var $hf = $('.js-advanced-search-selected');
            if ($hf.val() == 'false') {
                 $('.js-advanced-search').slideDown();
                $(this).text("Hide Advanced Search");
                $hf.val('true');
            } else {
                $('.js-advanced-search').slideUp();
                $(this).text("Show Advanced Search");
                $hf.val('false');
            }
        });
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <%-- Search does a full postback so that user can navigate back to search results --%>
        <asp:PostBackTrigger ControlID="btnSearch" />
    </Triggers>
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block" >
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-search"></i> <asp:Literal ID="lSearchHeading" runat="server" Text="Find A Guest"></asp:Literal></h1>
                <div class="panel-labels">
                    <span class="label label-default cursor-pointer js-toggle-advanced-search"><asp:Literal ID="lAdvancedSearchLink" runat="server" /></span>
                </div>
            </div>        

            <div class="panel-body">
                
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
                <div class="row">
                    <div class="col-sm-4">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                    </div>
                    <div class="col-sm-5">
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                    </div>
                    <div class="col-sm-3">
                        <Rock:DatePicker ID="dpDOB" runat="server" Label="Date of Birth" ForceParse="false"  />
                    </div>
                </div>

                <asp:Panel ID="pnlAdvancedSearch" runat="server" class="js-advanced-search" >
                    <hr />
                    <Rock:HiddenFieldWithClass ID="hfAdvancedSearch" runat="server" Value="false" CssClass="js-advanced-search-selected" />
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbStreet" runat="server" Label="Address" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbCity" runat="server" Label="City" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbPostalCode" runat="server" Label="Postal Code" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbPhone" runat="server" Label="Phone" />
                        </div>
                    </div>
                </asp:Panel>

                <div class="actions">
                    <asp:LinkButton ID="btnSearch" runat="server" AccessKey="s" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                    <asp:LinkButton ID="btnClear" runat="server" AccessKey="c" Text="Clear" CssClass="btn btn-link" CausesValidation="false" OnClick="btnClear_Click" />

                    <div class="pull-right">
                        <asp:LinkButton ID="btnAddFamily" runat="server" AccessKey="a" Text="Add Guest" CssClass="btn btn-default" OnClick="btnAddFamily_Click" Visible="false" />
                    </div>
                </div>

            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbOtherMatches" runat="server" NotificationBoxType="Info" Visible="false" />

        <asp:Panel ID="pnlResults" runat="server" CssClass="panel panel-block" Visible="false">
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> <asp:Literal ID="lResultsHeading" runat="server" Text="Results"></asp:Literal></h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gPeople" runat="server" EmptyDataText="No People Found" AllowSorting="true" OnRowSelected="gPeople_RowSelected" >
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField
                                DataField="FullNameReversed"
                                HeaderText="Name"
                                SortExpression="LastName,NickName" />
                            <Rock:DateField 
                                DataField="BirthDate" 
                                HeaderText="Birthdate" 
                                SortExpression="BirthYear desc,BirthMonth desc,BirthDay desc" 
                                ItemStyle-HorizontalAlign="Left" />
                            <Rock:RockTemplateField
                                HeaderText="Home Address" >
                                <ItemTemplate>
                                    <asp:Literal ID="lHomeLocation" runat="server" />
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
