<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ParentLocation.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.RoomCheckIn.ParentLocation" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').click(function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1>
            <Rock:RockLiteral ID="lHeader" runat="server" Text="" />
        </h1>
    </div>
                
    <div class="checkin-body">
        
        <div class="checkin-scroll-panel">
            <div class="scroller">

                <div class="control-group checkin-body-container">
                    <div class="checkin-header">
                        <h1><Rock:RockLiteral ID="lSelectLocation" runat="server" Text="Where Will The Parent/Guardian Be?" /></h1>
                    </div>
                    <div class="controls">
                        <asp:Repeater ID="rSelection" runat="server" OnItemCommand="rSelection_ItemCommand">
                            <ItemTemplate>
                                <Rock:BootstrapButton ID="lbSelect" runat="server" CommandArgument='<%# ((LocationOption)Container.DataItem).LocationToStore %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select"  DataLoadingText="Loading..." ><%# ((LocationOption)Container.DataItem).DisplayLocation %></Rock:BootstrapButton>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>

            </div>
        </div>

    </div>


     <div class="checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-default" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
