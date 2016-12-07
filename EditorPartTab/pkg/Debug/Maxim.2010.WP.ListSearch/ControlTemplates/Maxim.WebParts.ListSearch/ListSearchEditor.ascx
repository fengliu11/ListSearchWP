<%@ Assembly Name="Maxim.2010WP.ListSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8f574ecc066b18c6" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListSearchEditor.ascx.cs" Inherits="EditorPartTab.ControlTemplates.Maxim.WebParts.ListSearch.ListSearchEditor" %>


<style type="text/css">
    .style1
    {
        height: 214px;
    }
    .style2
    {
        width: 243px;
    }
</style>
<asp:HiddenField runat="server" ID="hiddenFieldDetectRequest" Value="0" />
<table cellpadding="2px" cellspacing="2px">
    <tr>
        <td class="style2">
            <asp:Panel ID="panelConfiguredTabs" runat="server">
                <table cellpadding="5px" cellspacing="5px">
                    <tr>
                        <td>
                            Select A List:
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:DropDownList ID="listCollection" runat="server" EnableViewState="true" AutoPostBack="true"
                                OnTextChanged="sel_SPList_OnTextChanged" Height="16px" Width="206px">
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Filter Fields:
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:ListBox ID="filterColumns" runat="server" Width="200px" Height="200px" SelectionMode="Multiple">
                            </asp:ListBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Display Fields:
                        </td>
                    </tr>
                    <tr>
                        <td class="style1">
                            <asp:ListBox ID="displayColumns" runat="server" Width="200px" Height="200px" SelectionMode="Multiple">
                            </asp:ListBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Filter Fields Names:(";" as delimiter)
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:TextBox ID="txtFilterNames" runat="server" Width="200px" Height="50px" textmode="multiline" TextWrapping="Wrap">
                            </asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Filter Fields Orders:
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:TextBox ID="txtFilterOrders" runat="server" Width="200px" SelectionMode="Multiple">
                            </asp:TextBox>
                        </td>
                    </tr>
<%--                    <tr>
                        <td>
                            Display Fields Names:
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:TextBox ID="txtDisplayNames" runat="server" Width="200px" Height="50px" SelectionMode="Multiple">
                            </asp:TextBox>
                        </td>
                    </tr>--%>
                    <tr>
                        <td>
                            Display Fields Orders:
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:TextBox ID="txtDisplayOrders" runat="server" Width="200px" SelectionMode="Multiple">
                            </asp:TextBox>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
        </td>
    </tr>
    <tr>
        <td colspan="2" align="right" class="style2">
            <asp:Button runat="server" ID="buttonSave" Text="Save" OnClick="ButtonSave_Click"
                CausesValidation="true" Visible="true" />
<%--            <asp:Button runat="server" ID="buttonCancel" Text="Cancel" OnClick="ButtonCancel_Click"
                CausesValidation="true" />&nbsp;&nbsp;--%>
        </td>
    </tr>
</table>
