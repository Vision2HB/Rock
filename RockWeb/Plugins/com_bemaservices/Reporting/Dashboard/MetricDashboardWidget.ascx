<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricDashboardWidget.ascx.cs" Inherits="com_bemaservices.Reporting.Dashboard.MetricDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDashboardTitle" runat="server" CssClass="dashboard-title">
            <asp:Literal runat="server" ID="lDashboardTitle" />
        </asp:Panel>
        <asp:Panel ID="pnlDashboardSubtitle" runat="server" CssClass="dashboard-subtitle">
            <asp:Literal runat="server" ID="lDashboardSubtitle" />
        </asp:Panel>
        <asp:Panel ID="phHtml" runat="server" />

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $.ajax({
                    url: '<%=this.RestUrl %>',
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (resultHtml) {
                    $('#<%=phHtml.ClientID%>').html(resultHtml);
                }).
                fail(function (a, b, c) {
                    debugger
                });
            });

        </script>

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModel" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="mdEdit_SaveClick" Title="Chart Dashboard Widget">
                <Content>

                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>
                            <Rock:MetricCategoryPicker ID="mpMetricCategoryPicker" runat="server" AllowMultiSelect="true" Label="Metric" OnSelectItem="mpMetricCategoryPicker_SelectItem"  />


                            <asp:PlaceHolder ID="phMetricValuePartitions" runat="server" />

                            <Rock:RockCheckBoxList ID="cblOptions" runat="server" Label="Options">
                                <asp:ListItem Text="Show Title" Value="MetricTitle" Selected="True" />
                                <asp:ListItem Text="Show Icon" Value="Icon" Selected="True" />
                                <asp:ListItem Text="Show Subtitle" Value="SubTitle" Selected="True" />
                                <asp:ListItem Text="Show Description" Value="Description" Selected="True" />
                                <asp:ListItem Text="Show Date" Value="ShowDate" Selected="True" />
                                <asp:ListItem Text="Show Current" Value="ShowCurrent" Selected="True" />
                                <asp:ListItem Text="Show Goal" Value="ShowGoal" Selected="True" />
                                <asp:ListItem Text="Show Last Week" Value="LastWeek" Selected="False" />
                                <asp:ListItem Text="Show Previous Year" Value="PreviousYear" Selected="True" />
                                <asp:ListItem Text="Show YTD Total/Average" Value="YTD" Selected="True" />
                                <asp:ListItem Text="Show 52 Week Total/Average" Value="52w" Selected="False" />
                                <asp:ListItem Text="Show Percent Change" Value="Percent" Selected="True" />
                                <asp:ListItem Text="Round Values" Value="Round" Selected="True" />
                                <asp:ListItem Text="Use Date Count for Averages (instead of week count)" Value="DateCount" Selected="False" />
                            </Rock:RockCheckBoxList>

                            <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" />

                        </ContentTemplate>
                    </asp:UpdatePanel>

                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
