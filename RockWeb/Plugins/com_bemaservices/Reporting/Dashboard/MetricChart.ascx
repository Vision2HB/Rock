<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricChart.ascx.cs" Inherits="com_bemaservices.Reporting.Dashboard.MetricChart" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

        <asp:Panel ID="pnlDashboardTitle" runat="server" CssClass="dashboard-title">
            <asp:Literal runat="server" ID="lIcon" />
            <asp:Literal runat="server" ID="lDashboardTitle" />
            <asp:Literal runat="server" ID="lDate" />
        </asp:Panel>
        <asp:Panel ID="pnlDashboardSubtitle" runat="server" CssClass="dashboard-subtitle">
            <asp:Literal runat="server" ID="lDashboardSubtitle" />
            <asp:Literal runat="server" ID="lDescription" />
        </asp:Panel>

        <Rock:DynamicPlaceholder ID="phCharts" runat="server" />

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModel" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="mdEdit_SaveClick" Title="Chart Dashboard Widget">
                <Content>

                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>
                            <Rock:MetricCategoryPicker ID="mpMetricCategoryPicker" runat="server" AllowMultiSelect="true" Label="Metric" OnSelectItem="mpMetricCategoryPicker_SelectItem"  />


                            <asp:PlaceHolder ID="phMetricValuePartitions" runat="server" />

                            <Rock:RockCheckBoxList ID="cblOptions" runat="server" Label="Metric Options">
                                <asp:ListItem Text="Show Title" Value="MetricTitle" Selected="True" />
                                <asp:ListItem Text="Show Icon" Value="Icon" Selected="True" />
                                <asp:ListItem Text="Show Subtitle" Value="SubTitle" Selected="True" />
                                <asp:ListItem Text="Show Description" Value="Description" Selected="True" />
                                <asp:ListItem Text="Show Date" Value="ShowDate" Selected="True" />
                                <asp:ListItem Text="Show Goal" Value="ShowGoal" Selected="True" />
                                <asp:ListItem Text="Show Previous Year" Value="PreviousYear" Selected="True" />
                                <asp:ListItem Text="Show Trend Line" Value="ShowTrend" Selected="True" />
                                <asp:ListItem Text="Show 52 Week Moving Average" Value="Show52" Selected="True" />
                                <asp:ListItem Text="Show 26 Week Moving Average" Value="Show26" Selected="True" />
                                <asp:ListItem Text="Show 13 Week Moving Average" Value="Show13" Selected="True" />
                            </Rock:RockCheckBoxList>

                            <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" />

                        </ContentTemplate>
                    </asp:UpdatePanel>

                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
