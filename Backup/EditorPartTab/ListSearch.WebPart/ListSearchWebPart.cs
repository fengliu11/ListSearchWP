using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;

namespace EditorPartTab.TabEditorWebPart
{
    [ToolboxItemAttribute(false)]
    public class TabEditorWebPart : System.Web.UI.WebControls.WebParts.WebPart, IWebEditable
    {
        private string _displayFields;
        private string _filterFields;
        private string _searchOperator;
        private string _strQueryOperator;
        private string _filterColumnsNamesList;
        private string _displayColumnsNamesList;
        private string _filterColumnsOrdersList;
        private string _displayColumnsOrdersList;
        private Button btnSearch;
        private Button btnReset;
        //private DropDownList sortOption;
        //private CheckBox sortDirection;
        private string[] displayFieldsList;
        private string[] filterFieldsList;
        private string[] filterQueryOpList;
        private string[] filterQueryAndOrList;
        private string[] filterNames;
        private Dictionary<int, string> _dispColumnOrders; //= new Dictionary<int, string>();
        private Panel filterFieldsView;
        private ListViewByQuery listViewByQuery;
        private string queryOperation;
        private UpdatePanel resultsViewPanel;
        private System.Web.UI.ScriptManager scriptManager;
        private DropDownList drpDwnList;
        private TextBox SearchParamTxtBox;
        private DropDownList drpDwnAndOr;
        private PagingButton pbutton = new PagingButton();
        private DateTimeControl dtControl;
        private bool blnValid = true;
        private SPFieldCollection fieldsControl;
        private bool isFound = true;
        private ListSettings _maximList;
        const string MaxSavedQuery = "MaxSavedQuery";
        const string MaxDispFields = "MaxDispFields";
        private bool cleared = false;
        [Personalizable(true)]
        public ListSettings MaximList
        {
            get
            {
                if (this._maximList == null)
                {
                    // Return an empty collection if null.
                    this._maximList = new ListSettings();
                }
                return this._maximList;
            }
            set
            {
                this._maximList = value;
            }
        }
        private SPQuery _maxQuery;
        public SPQuery MaximQuery
        {
            get
            {
                return this._maxQuery;
            }
            set
            {
                this._maxQuery = value;
            }
        }
        public override object WebBrowsableObject
        {
            // Return a reference to the Web Part instance.
            get { return this; }
        }
        public override EditorPartCollection CreateEditorParts()
        {
            //this.CreateChildControls();
            ListSearchEditorPart editorPart = new ListSearchEditorPart();
            // The ID of the editor part should be unique. So prefix it with the ID of the Web Part.
            editorPart.ID = this.ID + "_TabConfigurationEditorPart";
            // Create a collection of editor parts and add them to the EditorPart collection.
            List<EditorPart> editors = new List<EditorPart> { editorPart };
            return new EditorPartCollection(editors);
        }
        private void EnsureUpdatePanelFixups()
        {
            if (this.Page.Form != null)
            {
                string str = this.Page.Form.Attributes["onsubmit"];
                if (str == "return _spFormOnSubmitWrapper();")
                {
                    this.Page.Form.Attributes["onsubmit"] = "_spFormOnSubmitWrapper();";
                }
            }
            System.Web.UI.ScriptManager.RegisterStartupScript(this, typeof(ListSearchEditorPart), "UpdatePanelFixup", "_spOriginalFormAction = document.forms[0].action; _spSuppressFormOnSubmitWrapper=true;", true);
        }
        private void clear_Click(object sender, EventArgs e)
        {
            foreach (Control control in this.filterFieldsView.Controls)
            {
                control.EnableViewState = true;
                if (control is TextBox)
                {
                    TextBox box = control as TextBox;
                    box.Text = "";
                    //if (!string.IsNullOrEmpty(box.Text))
                    //{
                    //    str = str + box.ID + ";";
                    //}
                }
                if (control is DropDownList)
                {
                    DropDownList dropD = control as DropDownList;
                    dropD.SelectedIndex = 0;
                }
                if (control is DateTimeControl)
                {
                    try
                    {
                        DateTimeControl dtbox = control as DateTimeControl;
                        dtbox.ClearSelection();
                    }
                    catch { }
                }
            }
            SPQuery tempQuery = new SPQuery();
            tempQuery.Query = "<Where><IsNull><FieldRef Name=\"ID\" /></IsNull></Where>";
            this.listViewByQuery.Query = tempQuery; 

            //this.listViewByQuery.Query = this.GenerateCAMLQuery(string.Empty);
            this.listViewByQuery.Query.AutoHyperlink = true;
           //CreateChildControls();
            cleared = true;

        }
        private void click_Click(object sender, EventArgs e)
        {
            isFound = false;
            using (SPSite site = new SPSite(HttpContext.Current.Request.Url.ToString()))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    if (!string.IsNullOrEmpty(this.MaximList.ListName))
                    {
                        this.listViewByQuery.List = web.Lists[this.MaximList.ListName];
                        //this.UpdateSearchOperator();//Used to get the search operator
                        this.listViewByQuery.Query = this.GenerateCAMLQuery(string.Empty);
                        this.listViewByQuery.Query.AutoHyperlink = true;
                        //this.listViewByQuery.ch
                    }

                }
            }
        }
        public override void RenderControl(HtmlTextWriter writer)
        {
            int num = 1;
            //if (!string.IsNullOrEmpty(this.Page.Request.QueryString["View"]))
            //{
            //    string strPageQS = "View=" + this.Page.Request.QueryString["View"];
            //    string strPageUrl = this.Page.Request.Url.ToString().Replace("?" + strPageQS, "?a=a").Replace("&" + strPageQS, "");
            //    this.Page.Response.Redirect(strPageUrl, true);
            //}
            if (!string.IsNullOrEmpty(this.MaximList.ListName))
            {
                writer.Write("<BR><TABLE width=\"800px\" align=\"center\" style=\"border-style:solid;border-width:1px;border-spacing:10px 10px;border-color:#DCDCDC\">");
                foreach (Control control in this.filterFieldsView.Controls)
                {
                    control.EnableViewState = true;
                    if (control is Label)
                    {
                        writer.Write("<tr style=height:6px>");
                        writer.Write("<TD style=width:40% align=center>");
                        control.RenderControl(writer);
                        writer.Write("</TD>");
                    }
                    if (control is DropDownList && control.ID.StartsWith("drpDwnList"))
                    {
                        writer.Write("<TD style=width:30%>");
                        control.RenderControl(writer);
                        writer.Write("</TD>");
                    }
                    if (control is TextBox || control is DateTimeControl)// || control is DateTimeControl
                    {
                        writer.Write("<TD style=width:28%>");
                        control.RenderControl(writer);
                        writer.Write("</TD>");
                    }
                    if (control is DropDownList && control.ID.StartsWith("drpDwnAndOr"))
                    {
                        writer.Write("<TD style=width:2%>");
                        control.RenderControl(writer);
                        writer.Write("</TD>");
                        writer.Write("</tr>");
                    }
                    num++;
                }
                writer.WriteLine("<td></td></tr>");
                //writer.WriteLine("<tr><td>");
                //writer.WriteLine("Sort By: </td><td>");
                //this.sortOption.RenderControl(writer);
                //writer.WriteLine("</td><td align=left colspan=2>ASC: ");
                //this.sortDirection.RenderControl(writer);
                //writer.WriteLine("</td></tr>");
                writer.WriteLine("<tr><td align=left>");
                this.btnReset.RenderControl(writer);
                writer.WriteLine("<td></td><td colspan=2 align=right>");
                this.btnSearch.RenderControl(writer);
                writer.WriteLine("</td></tr></TABLE>");
                //if (string.IsNullOrEmpty(this.ViewState["listViewDirection"].ToString())) this.ViewState["listViewDirection"] = "true";




                if (Page.IsPostBack)
                {
                    using (SPSite site = new SPSite(HttpContext.Current.Request.Url.ToString()))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            if (!string.IsNullOrEmpty(this.MaximList.ListName))
                            {
                                this.listViewByQuery.List = web.Lists[this.MaximList.ListName];
                                this.listViewByQuery.Query = this.GenerateCAMLQuery(string.Empty);
                                this.listViewByQuery.Query.AutoHyperlink = true;
                            }
                        }
                    }
                }
                if(!Page.IsPostBack||cleared)
                {
                    using (SPSite site = new SPSite(HttpContext.Current.Request.Url.ToString()))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            if (!string.IsNullOrEmpty(this.MaximList.ListName))
                            {
                                this.listViewByQuery.List = web.Lists[this.MaximList.ListName];
                                SPQuery tempQuery = new SPQuery();
                                tempQuery.Query = "<Where><IsNull><FieldRef Name=\"ID\" /></IsNull></Where>";
                                this.listViewByQuery.Query = tempQuery;
                                this.listViewByQuery.Query.AutoHyperlink = true;
                            }
                        }
                    }
                    cleared = false;
                }
                if (this.listViewByQuery.List != null)
                {
                    if (blnValid == true)
                    {
                        int queryCount = this.listViewByQuery.List.GetItems(this.listViewByQuery.Query).Count;
                        int rowLimit = (int)this.listViewByQuery.Query.RowLimit;

                        if (queryCount.Equals(rowLimit))
                        {
                            writer.WriteLine("<br>");
                            writer.WriteLine("<font color=blue>The search result may contain more items than as shown below. It's suggested to refine your search criteria to get more accurate results.</font>");
                            writer.WriteLine("<br>");
                        }
                        //this.listViewByQuery.Query.ListItemCollectionPosition = this.listViewByQuery.List.GetItems(this.listViewByQuery.Query).ListItemCollectionPosition;
                        //string value = this.listViewByQuery.Query.ListItemCollectionPosition.PagingInfo.ToString();
                        //this.listViewByQuery.Query.ListItemCollectionPosition.PagingInfo = value;
                        ////this.listViewByQuery.Query.ListItemCollectionPosition.PagingInfo
                        writer.WriteLine("<br>");
                        writer.WriteLine("<b><i>Total Count of Search Result Items is:</i> " + queryCount + "</b>");

                        writer.WriteLine("<br>");
                        if (this.ViewState[MaxDispFields] != null)
                        {
                            this.listViewByQuery.Query.ViewFields = this.ViewState[MaxDispFields].ToString();
                        }
                        this.listViewByQuery.RenderControl(writer);
                    }
                }
                else
                {
                    writer.Write("Please modify the search...");
                }
            }
            else
            {
                writer.Write("Please Configure the List and Filters in Tool Pane...");
            }
        }
        private SPQuery GenerateCAMLQuery(string _sortColumn)
        {
            SPFieldCollection fields;
            string str = string.Empty;
            string str2 = string.Empty;
            string onDispFields = string.Empty;
            SPQuery query = new SPQuery();
            string fieldName = string.Empty;
            string strOpArray = string.Empty;
            string strAddOrArray = string.Empty;
            string savedQuery = string.Empty;
            query.QueryThrottleMode = SPQueryThrottleOption.Override;
            query.ViewAttributes = "Scope=\"RecursiveAll\"";//BaseViewID=\"0\" 
            query.AutoHyperlink = true;
            query.IndividualProperties = true;
            string strQueryOperator = string.Empty;
            using (SPSite site = new SPSite(HttpContext.Current.Request.Url.ToString()))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    query = new SPQuery(web.Lists[this.MaximList.ListName].Views[0]);
                }
            }
            query.RowLimit.Equals("10");

            foreach (Control control in this.filterFieldsView.Controls)
            {
                control.EnableViewState = true;
                if (control is DropDownList && control.ID.StartsWith("drpDwnList"))
                {
                    DropDownList drpDwn = control as DropDownList;
                    if (!string.IsNullOrEmpty(drpDwn.SelectedValue))
                    {
                        //[(this.filterFieldsView.Controls.IndexOf(control) + 1)] is TextBox)
                        if (this.filterFieldsView.Controls[(this.filterFieldsView.Controls.IndexOf(control) + 1)] is TextBox)
                        {
                            TextBox box = control as TextBox;
                            box = (TextBox)this.filterFieldsView.Controls[(this.filterFieldsView.Controls.IndexOf(control) + 1)];
                            if (!string.IsNullOrEmpty(box.Text))
                            {
                                strOpArray = strOpArray + drpDwn.ID + ";";
                            }
                        }
                        else if (this.filterFieldsView.Controls[(this.filterFieldsView.Controls.IndexOf(control) + 1)] is DateTimeControl)
                        {
                            try
                            {
                                DateTimeControl dtControl = new DateTimeControl();
                                dtControl = (DateTimeControl)this.filterFieldsView.Controls[(this.filterFieldsView.Controls.IndexOf(control) + 1)];
                                //DateTime dtDateTime = ((DateTimeControl)this.filterFieldsView.Controls[(this.filterFieldsView.Controls.IndexOf(control) + 1)]).SelectedDate;
                                if (!(dtControl.IsValid)) blnValid = false;
                                if (!dtControl.IsDateEmpty)
                                {
                                    strOpArray = strOpArray + drpDwn.ID + ";";
                                }
                            }
                            catch { }
                        }
                    }
                }
                if (control is TextBox)
                {
                    TextBox box = control as TextBox;
                    if (!string.IsNullOrEmpty(box.Text))
                    {
                        str = str + box.ID + ";";
                    }
                }
                if (control is DateTimeControl)
                {
                    try
                    {
                        DateTimeControl dtbox = control as DateTimeControl;
                        if (!dtbox.IsDateEmpty)
                        {
                            str = str + dtbox.ID + ";";
                        }
                    }
                    catch { }
                }
                if (control is DropDownList && control.ID.StartsWith("drpDwnAndOr"))
                {
                    DropDownList drpDwnAO = control as DropDownList;
                    if (!string.IsNullOrEmpty(drpDwnAO.SelectedValue))
                    {
                        if (this.filterFieldsView.Controls[(this.filterFieldsView.Controls.IndexOf(control) - 1)] is TextBox)
                        {
                            TextBox box1 = control as TextBox;
                            box1 = (TextBox)this.filterFieldsView.Controls[(this.filterFieldsView.Controls.IndexOf(control) - 1)];
                            if (!string.IsNullOrEmpty(box1.Text))
                            {
                                strAddOrArray = strAddOrArray + drpDwnAO.ID + ";";
                            }
                        }
                        else if (this.filterFieldsView.Controls[(this.filterFieldsView.Controls.IndexOf(control) - 1)] is DateTimeControl)
                        {
                            try
                            {
                                DateTimeControl dtControl1 = (DateTimeControl)this.filterFieldsView.Controls[(this.filterFieldsView.Controls.IndexOf(control) - 1)];
                                //DateTime dtDateTime = ((DateTimeControl)this.filterFieldsView.Controls[(this.filterFieldsView.Controls.IndexOf(control) + 1)]).SelectedDate;
                                if (!dtControl1.IsDateEmpty)
                                {
                                    strAddOrArray = strAddOrArray + drpDwnAO.ID + ";";
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            this.filterFieldsList = str.Split(new char[] { ';' });
            this.filterQueryOpList = strOpArray.Split(new char[] { ';' });
            this.filterQueryAndOrList = strAddOrArray.Split(new char[] { ';' });
            this.displayFieldsList = this.displayFields.Split(new char[] { ';' });
            using (SPSite site2 = new SPSite(HttpContext.Current.Request.Url.ToString()))
            {
                using (SPWeb web2 = site2.OpenWeb())
                {
                    fields = web2.Lists[this.MaximList.ListName].Fields;
                }
            }
            onDispFields = "<FieldRef Name = 'DocIcon' />"; //<FieldRef Name = 'FileLeafRef' />";

            if (ViewDispColumnOrders == null || ViewDispColumnOrders.Count == 0)
            {
                foreach (string str4 in this.displayFieldsList)
                {
                    if (!string.IsNullOrEmpty(str4))
                    {
                        onDispFields = onDispFields + "<FieldRef Name=\"" + fields.GetField(str4).InternalName + "\"  />";
                    }
                }
            }
            else
            {
                try
                {
                    for (int i = 1; i <= this.ViewDispColumnOrders.Count; i++)
                    {
                        onDispFields = onDispFields + "<FieldRef Name=\"" + fields.GetField(ViewDispColumnOrders[i]).InternalName + "\"  />";
                    }
                }
                catch { }
            }
            //str2 = str2 + BuildFilter();
            for (int i = 0; i < (this.filterFieldsList.Length - 1); i++)
            {
                this.UpdateSearchOperator(((DropDownList)this.FindControl(this.filterQueryOpList[i])).SelectedValue);//Used to get the search operator

                if (fields.GetField(this.filterFieldsList[i]).TypeAsString == "DateTime")
                {
                    //string strForQO = ((TextBox)this.FindControl(this.filterFieldsList[i])).Text;
                    //DateTime dtDateTime = Convert.ToDateTime(strForQO);
                    DateTime dtDateTime = ((DateTimeControl)this.FindControl(this.filterFieldsList[i])).SelectedDate;
                    string iso8601date = dtDateTime.ToString("s") + "Z";
                    str2 = str2 + this.queryOperation + string.Format("<FieldRef Name=\"{0}\" /><Value Type=\"{1}\" IncludeTimeValue=\"TRUE\">{2}</Value>", fields.GetField(this.filterFieldsList[i]).InternalName, fields.GetField(this.filterFieldsList[i]).TypeAsString, iso8601date);//TypeAsString
                }
                else
                {
                    str2 = str2 + this.queryOperation + string.Format("<FieldRef Name=\"{0}\" /><Value Type=\"{1}\">{2}</Value>", fields.GetField(this.filterFieldsList[i]).InternalName, fields.GetField(this.filterFieldsList[i]).TypeAsString, ((TextBox)this.FindControl(this.filterFieldsList[i])).Text);//TypeAsString
                }
                if (i >= 1)
                {
                    if (i < (this.filterFieldsList.Length - 1))
                    {
                        //if (i < (this.filterFieldsList.Length - 2))
                        strQueryOperator = ((DropDownList)this.FindControl(this.filterQueryAndOrList[i - 1])).SelectedValue;
                        // str2 = ("<And>" + str2) + this.queryOperation.Insert(this.queryOperation.IndexOf('<') + 1, "/") + "</And>";
                        str2 = ("<" + strQueryOperator + ">" + str2) + this.queryOperation.Insert(this.queryOperation.IndexOf('<') + 1, "/") + "</" + strQueryOperator + ">";
                    }
                }
                else
                {
                    str2 = str2 + this.queryOperation.Insert(this.queryOperation.IndexOf('<') + 1, "/");
                }
            }
            //if (this.Context.Request[filterfield] != null && this.Context.Request[filtervalue] != null)
            //    {
            //if (!string.IsNullOrEmpty(BuildFilter()))
            //{
            //str2 = BuildFilter();
            if (isFound)
            {
                string strbuildFilter = BuildFilter();
                if (!string.IsNullOrEmpty(strbuildFilter))
                {
                    if (string.IsNullOrEmpty(str2))
                    {
                        //str2 = BuildFilter();
                        str2 = "<Where>" + strbuildFilter + "</Where>";
                    }
                    else
                    {
                        str2 = "<And>" + str2 + strbuildFilter + "</And>";
                    }
                }
            }
            if (this.filterFieldsList.Length != 1)
            {
                str2 = "<Where>" + str2 + "</Where>";
            }
            if (!string.IsNullOrEmpty(onDispFields))
            {
                query.ViewFields = onDispFields;
                query.AutoHyperlink = true;
            }
            // apply filtering and/or sorting
            string sortorder;
            string strSortDir = "false";
            if (this.Context.Request != null && this.Context.Request["SortField"] != null)
            {
                strSortDir = this.ViewState["sortListOrder"].ToString();
                sortorder = "<OrderBy><FieldRef Name='" + fields.GetField(this.Context.Request["SortField"]).InternalName + "' Ascending='" + strSortDir + "' /></OrderBy>";
            }
            else
            {
                sortorder = "<OrderBy><FieldRef Name='" + fields.GetField("Title").InternalName + "' Ascending='" + strSortDir + "' /></OrderBy>";
            }
            str2 = str2 + sortorder;
            query.Query = str2;
            query.ViewFields = onDispFields;
            this.ViewState[MaxSavedQuery] = query.Query;
            return query;
        }
        private string BuildFilter()
        {
            string filterParameter = "";
            string strFilterValueFilled = "";
            int counter = 1;
            while (isFound)
            {
                string filterfield = "FilterField" + counter.ToString();
                string filtervalue = "FilterValue" + counter.ToString();
                if (this.Context.Request[filterfield] != null && this.Context.Request[filtervalue] != null)
                {
                    using (SPSite site = new SPSite(HttpContext.Current.Request.Url.ToString()))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            if (!string.IsNullOrEmpty(this.MaximList.ListName))
                            {
                                //SPList sp_List = web.Lists[this.searchListName];
                                string str_filterField = this.Context.Request[filterfield];
                                int FieldIndex = this.Context.Request[filterfield].IndexOf(this.Context.Request[filterfield]);
                                SPFieldCollection spFldColl = web.Lists[this.MaximList.ListName].Fields;
                                string spFieldType = string.Empty;
                                for (int i = 0; i < spFldColl.Count; i++)
                                {
                                    if (spFldColl[i].StaticName.Equals(str_filterField))
                                    {
                                        SPField field = spFldColl.GetFieldByInternalName(str_filterField);
                                        spFieldType = field.TypeAsString;
                                    }
                                }
                                //if (spFieldType.Equals("DateTime"))
                                //{
                                //    string iso8601FilterDate = this.Context.Request[filtervalue].ToString("s") + "Z";
                                //    strFilterValueFilled = iso8601FilterDate;
                                //}
                                //else
                                //{
                                strFilterValueFilled = this.Context.Request[filtervalue];
                                //}
                                if (counter > 1)
                                {
                                    if (string.IsNullOrEmpty(this.Context.Request[filtervalue]))
                                    {
                                        filterParameter = "<And>" + filterParameter + string.Format("<IsNull><FieldRef Name=\"{0}\" /></IsNull>", this.Context.Request[filterfield]) + "</And>";
                                    }
                                    else
                                    {
                                        filterParameter = "<And>" + filterParameter + string.Format("<Eq><FieldRef Name=\"{0}\" /><Value Type=\"{1}\">{2}</Value></Eq>", this.Context.Request[filterfield], spFieldType, strFilterValueFilled) + "</And>";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(this.Context.Request[filtervalue]))
                                    {
                                        filterParameter = string.Format("<IsNull><FieldRef Name=\"{0}\" /></IsNull>", this.Context.Request[filterfield]);
                                    }
                                    else
                                    {
                                        filterParameter = string.Format("<Eq><FieldRef Name=\"{0}\" /><Value Type=\"{1}\">{2}</Value></Eq>", this.Context.Request[filterfield], spFieldType, strFilterValueFilled);
                                    }
                                }
                                //filterParameter += "<Eq><FieldRef Name=" + this.Context.Request[filterfield] + " />" +
                                //"<Value Type=" + spFieldType + "/>"
                                //+ this.Context.Request[filtervalue] + "</Value></Eq>";
                                //filterParameter = filterParameter + string.Format("<Eq><FieldRef Name=\"{0}\" /><Value Type=\"{1}\">{2}</Value>", this.Context.Request[filterfield], spFieldType, this.Context.Request[filtervalue]);
                                counter++;
                            }
                        }
                    }
                }
                else
                {
                    isFound = false;
                }
            }
            return filterParameter;
        }
        private void UpdateSearchOperator(string searchOperator)
        {
            //string searchOperator = this.drpDwnOperator.SelectedValue;
            if (searchOperator != null)
            {
                if (!(searchOperator == "Equals"))
                {
                    if (searchOperator == "Begins with")
                    {
                        this.queryOperation = "<BeginsWith>";
                    }
                    else if (searchOperator == "Contains")
                    {
                        this.queryOperation = "<Contains>";
                    }
                    else if (searchOperator == "Not Equals")
                    {
                        this.queryOperation = "<Neq>";
                    }
                    else if (searchOperator == "Is Less Than or Equal To")
                    {
                        this.queryOperation = "<Leq>";
                    }
                    else if (searchOperator == "Is Less Than")
                    {
                        this.queryOperation = "<Lt>";
                    }
                    else if (searchOperator == "Is Greater Than or Equal To")
                    {
                        this.queryOperation = "<Geq>";
                    }
                    else if (searchOperator == "Is Greater Than")
                    {
                        this.queryOperation = "<Gt>";
                    }
                    else if (searchOperator == "Is Blank")
                    {
                        this.queryOperation = "<IsNull>";
                    }
                    else if (searchOperator == "Is Not Blank")
                    {
                        this.queryOperation = "<IsNotNull>";
                    }
                }
                else
                {
                    this.queryOperation = "<Eq>";
                }
            }
            else
            {
                this.queryOperation = "<Contains>";
            }
        }
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.EnsureUpdatePanelFixups();
            Dictionary<string, string> listFilters = new Dictionary<string, string>();
            Dictionary<int, string> filterOrders = new Dictionary<int, string>();

            try
            {
                this.filterFields = this.MaximList.filterFields;
                this.displayFields = this.MaximList.displayFields;
                this.filterColumnsNamesList = this.MaximList.filterFieldsNames;
                this.displayColumnsOrdersList = this.MaximList.displayFieldsOrders;
                this.filterColumnsOrdersList = this.MaximList.filterFieldsOrders;




                this.filterFieldsList = this.MaximList.filterFields.Split(new char[] { ';' });
                if (!string.IsNullOrEmpty(filterColumnsNamesList) && !string.IsNullOrEmpty(filterColumnsOrdersList))
                {
                    this.filterNames = this.MaximList.filterFieldsNames.Split(new char[] { ';' });
                    int i = 0;
                    foreach (string fn in this.filterNames)
                    {
                        try
                        {
                            listFilters.Add(filterFieldsList[i], fn);
                            i++;
                        }
                        catch { }
                    }
                }
                else
                {
                    foreach (string str in this.filterFieldsList)
                    {
                        listFilters.Add(str, str);
                    }
                }
                this.filterFieldsList = this.MaximList.filterFields.Split(new char[] { ';' });
                try
                {
                    if (!string.IsNullOrEmpty(filterColumnsOrdersList))
                    {
                        int i = 0;
                        string[] Orders = this.MaximList.filterFieldsOrders.Split(new char[] { ';' });
                        foreach (string fn in this.filterFieldsList)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(fn))
                                {
                                    filterOrders.Add(int.Parse(Orders[i]), fn);
                                    i++;
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
                ViewDispColumnOrders = new Dictionary<int, string>();
                this.displayFieldsList = this.MaximList.displayFields.Split(new char[] { ';' });
                try
                {
                    if (!string.IsNullOrEmpty(displayColumnsOrdersList))
                    {
                        int i = 0;
                        string[] Orders = this.MaximList.displayFieldsOrders.Split(new char[] { ';' });
                        foreach (string fn in this.displayFieldsList)
                        {
                            if (!string.IsNullOrEmpty(fn))
                            {
                                ViewDispColumnOrders.Add(int.Parse(Orders[i]), fn);
                                i++;
                            }
                        }
                    }
                }
                catch { }
                //filter
                using (SPSite site3 = new SPSite(HttpContext.Current.Request.Url.ToString()))
                {
                    using (SPWeb web3 = site3.OpenWeb())
                    {
                        if (!string.IsNullOrEmpty(this.MaximList.ListName))
                        {
                            fieldsControl = web3.Lists[this.MaximList.ListName].Fields;
                        }
                    }
                }
            }
            catch { }
            int intcount = 1;
            this.filterFieldsView = new Panel();
            this.filterFieldsView.EnableViewState = true;
            string strCheck = "|";
            if (this.MaximList != null)
            {
                if (!string.IsNullOrEmpty(this.MaximList.filterFields))
                {
                    this.filterFieldsList = this.MaximList.filterFields.Split(new char[] { ';' });
                    try
                    {
                        if (filterOrders != null && filterOrders.Count != 0)
                        {
                            for (int i = 1; i <= filterOrders.Count; i++)
                            {
                                string str = filterOrders[i];

                                //string tempKey = string.Empty;
                                if (!string.IsNullOrEmpty(str) && !strCheck.Contains("|" + str + "|"))
                                {
                                    this.filterFieldsView.EnableViewState = true;
                                    if (fieldsControl.GetField(str).TypeAsString == "DateTime")
                                    {
                                        dtControl = new DateTimeControl();
                                        dtControl.EnableViewState = true;
                                        dtControl.LocaleId = (int)SPContext.Current.RegionalSettings.LocaleId;
                                        //tempKey = str;
                                        dtControl.ID = str;
                                        dtControl.DatePickerFrameUrl = "/_layouts/iframe.aspx";
                                        dtControl.CalendarImageUrl = "/_layouts/images/Blank.gif";
                                        dtControl.DatePickerJavaScriptUrl = "/_layouts/datepicker.js";
                                        //((DateTimeControl)dtControl.Controls[1]).DatePickerJavaScriptUrl.Equals("http://mosstest02/_layouts/datepicker.js");//Style.Add("display", "none");
                                        dtControl.DateOnly = true;
                                        // dtControl.DatePickerJavaScriptUrl = "http://mosstest02/_layouts/datepicker.js";
                                        //((TextBox)dtControl.Controls[1]).Style.Add("display", "none"); 
                                        //((System.Web.UI.WebControls.Image)dtControl.Controls[1]).Style.Add("display", "none"); 
                                        this.Controls.Add(dtControl);
                                    }
                                    else
                                    {
                                        SearchParamTxtBox = new TextBox();
                                        SearchParamTxtBox.ID = str;
                                        this.Controls.Add(SearchParamTxtBox);
                                    }
                                    Label lbl_SearchParam = new Label();
                                    lbl_SearchParam.Text = listFilters[str] + ": ";
                                    this.filterFieldsView.Controls.Add(lbl_SearchParam);

                                    drpDwnList = new DropDownList();
                                    drpDwnList.ID = "drpDwnList" + listFilters[str];
                                    if (!(fieldsControl.GetField(str).TypeAsString == "DateTime" || fieldsControl.GetField(str).TypeAsString == "Number" || fieldsControl.GetField(str).TypeAsString == "Currency"))
                                    {
                                        drpDwnList.Items.Add("Contains");
                                        drpDwnList.Items.Add("Begins with");
                                        //drpDwnList.Items.Add("Is Blank");
                                        //drpDwnList.Items.Add("Is Not Blank");
                                    }
                                    drpDwnList.Items.Add("Equals");
                                    drpDwnList.Items.Add("Not Equals");
                                    if ((fieldsControl.GetField(str).TypeAsString == "Currency") || (fieldsControl.GetField(str).TypeAsString == "Number") || (fieldsControl.GetField(str).TypeAsString == "DateTime"))
                                    {
                                        drpDwnList.Items.Add("Is Less Than or Equal To");
                                        drpDwnList.Items.Add("Is Less Than");
                                        drpDwnList.Items.Add("Is Greater Than or Equal To");
                                        drpDwnList.Items.Add("Is Greater Than");
                                    }
                                    this.Controls.Add(drpDwnList);
                                    this.filterFieldsView.Controls.Add(drpDwnList);
                                    if (fieldsControl.GetField(str).TypeAsString == "DateTime")
                                    {
                                        this.filterFieldsView.Controls.Add(dtControl);
                                    }
                                    else
                                    {
                                        this.filterFieldsView.Controls.Add(SearchParamTxtBox);
                                    }

                                    if (intcount < (filterFieldsList.Length) - 1)
                                    {
                                        this.drpDwnAndOr = new DropDownList();
                                        this.drpDwnAndOr.Visible = false;
                                        drpDwnAndOr.ID = "drpDwnAndOr" + str;
                                        this.drpDwnAndOr.Items.Add("And");
                                        this.drpDwnAndOr.Items.Add("Or");
                                        this.filterFieldsView.Controls.Add(drpDwnAndOr);
                                        intcount++;
                                    }
                                }
                                strCheck += str + "|";
                                this.filterFieldsView.Controls.Add(new LiteralControl("<br /><br />"));
                            }
                        }
                        else
                        {
                            foreach (string str in filterFieldsList)
                            {
                                if (!string.IsNullOrEmpty(str) && !strCheck.Contains("|" + str + "|"))
                                {
                                    this.filterFieldsView.EnableViewState = true;
                                    if (fieldsControl.GetField(str).TypeAsString == "DateTime")
                                    {
                                        dtControl = new DateTimeControl();
                                        dtControl.EnableViewState = true;
                                        dtControl.LocaleId = (int)SPContext.Current.RegionalSettings.LocaleId;
                                        //tempKey = str;
                                        dtControl.ID = str;
                                        dtControl.DatePickerFrameUrl = "/_layouts/iframe.aspx";
                                        dtControl.CalendarImageUrl = "/_layouts/images/Blank.gif";
                                        dtControl.DatePickerJavaScriptUrl = "/_layouts/datepicker.js";
                                        //((DateTimeControl)dtControl.Controls[1]).DatePickerJavaScriptUrl.Equals("http://mosstest02/_layouts/datepicker.js");//Style.Add("display", "none");
                                        dtControl.DateOnly = true;
                                        // dtControl.DatePickerJavaScriptUrl = "http://mosstest02/_layouts/datepicker.js";
                                        //((TextBox)dtControl.Controls[1]).Style.Add("display", "none"); 
                                        //((System.Web.UI.WebControls.Image)dtControl.Controls[1]).Style.Add("display", "none"); 
                                        this.Controls.Add(dtControl);
                                    }
                                    else
                                    {
                                        SearchParamTxtBox = new TextBox();
                                        SearchParamTxtBox.ID = str;
                                        this.Controls.Add(SearchParamTxtBox);
                                    }
                                    Label lbl_SearchParam = new Label();
                                    lbl_SearchParam.Text = listFilters[str] + ": ";
                                    this.filterFieldsView.Controls.Add(lbl_SearchParam);

                                    drpDwnList = new DropDownList();
                                    drpDwnList.ID = "drpDwnList" + listFilters[str];
                                    if (!(fieldsControl.GetField(str).TypeAsString == "DateTime" || fieldsControl.GetField(str).TypeAsString == "Number" || fieldsControl.GetField(str).TypeAsString == "Currency"))
                                    {
                                        drpDwnList.Items.Add("Contains");
                                        drpDwnList.Items.Add("Begins with");
                                        //drpDwnList.Items.Add("Is Blank");
                                        //drpDwnList.Items.Add("Is Not Blank");
                                    }

                                    drpDwnList.Items.Add("Equals");
                                    drpDwnList.Items.Add("Not Equals");

                                    if ((fieldsControl.GetField(str).TypeAsString == "Currency") || (fieldsControl.GetField(str).TypeAsString == "Number") || (fieldsControl.GetField(str).TypeAsString == "DateTime"))
                                    {
                                        drpDwnList.Items.Add("Is Less Than or Equal To");
                                        drpDwnList.Items.Add("Is Less Than");
                                        drpDwnList.Items.Add("Is Greater Than or Equal To");
                                        drpDwnList.Items.Add("Is Greater Than");
                                    }
                                    this.Controls.Add(drpDwnList);
                                    this.filterFieldsView.Controls.Add(drpDwnList);
                                    if (fieldsControl.GetField(str).TypeAsString == "DateTime")
                                    {
                                        this.filterFieldsView.Controls.Add(dtControl);
                                    }
                                    else
                                    {
                                        this.filterFieldsView.Controls.Add(SearchParamTxtBox);
                                    }

                                    if (intcount < (filterFieldsList.Length) - 1)
                                    {
                                        this.drpDwnAndOr = new DropDownList();
                                        this.drpDwnAndOr.Visible = false;
                                        drpDwnAndOr.ID = "drpDwnAndOr" + str;
                                        this.drpDwnAndOr.Items.Add("And");
                                        this.drpDwnAndOr.Items.Add("Or");
                                        this.filterFieldsView.Controls.Add(drpDwnAndOr);
                                        intcount++;
                                    }
                                }
                                strCheck += str + "|";
                                this.filterFieldsView.Controls.Add(new LiteralControl("<br /><br />"));
                            }
                        }
                    }
                    catch { }
                }
            }
            this.resultsViewPanel = new UpdatePanel();
            this.resultsViewPanel.ID = "ResultsViewPanel";
            this.resultsViewPanel.ChildrenAsTriggers = true;
            //added this line for datecontrol
            this.resultsViewPanel.EnableViewState = true;
            //added above line for datecontrol
            this.resultsViewPanel.UpdateMode = UpdatePanelUpdateMode.Conditional; //Always; //Conditional;
            //this.Controls.Add(this.resultsViewPanel);
            //this.sortOption = new DropDownList();
            //sortOption.ID = "drpSort";
            //sortOption.Text = "Sort By:";
            //if (this.MaximList.displayFields != null)
            //{
            //    string[] disList = this.MaximList.displayFields.Split(new char[] { ';' });
            //    foreach (string dItem in disList)
            //    {
            //        if (!string.IsNullOrEmpty(dItem))
            //        {
            //            this.sortOption.Items.Add(dItem);
            //        }
            //    }
            //}
            //this.resultsViewPanel.ContentTemplateContainer.Controls.Add(this.sortOption);
            //sortDirection = new CheckBox();
            //this.sortDirection.ID = "chkSortDirection";
            //this.resultsViewPanel.ContentTemplateContainer.Controls.Add(this.sortDirection);

            this.btnSearch = new Button();
            this.btnSearch.Text = "Search";
            //this.btnReset.Width = 60;
            this.btnSearch.Click += new EventHandler(click_Click);
            this.resultsViewPanel.ContentTemplateContainer.Controls.Add(this.btnSearch);

            this.btnReset = new Button();
            this.btnReset.Text = "Clear";
            //this.btnReset.Width = 60;
            this.btnReset.Click += new EventHandler(clear_Click);
            this.resultsViewPanel.ContentTemplateContainer.Controls.Add(this.btnReset);

            this.listViewByQuery = new ListViewByQuery();
            this.listViewByQuery.ID = "listViewByQuery";
            this.listViewByQuery.Enabled = true;
            this.listViewByQuery.Visible = true;
            this.listViewByQuery.DisableFilter = true;
            this.listViewByQuery.DisableSort = false;
            this.listViewByQuery.BorderStyle = BorderStyle.Solid;

            if (!string.IsNullOrEmpty(this.MaximList.ListName))
            {
                SPList qryList = SPContext.Current.Web.Lists[this.MaximList.ListName];
                this.listViewByQuery.List = qryList;//SPContext.Current.Web.Lists[this.MaximList.ListName];
                string query = string.Empty;
                string sortColumn = string.Empty;
                if (this.ViewState["sortListOrder"] == null) this.ViewState["sortListOrder"] = "true";
                this.listViewByQuery.Query = GenerateCAMLQuery(string.Empty);
                if (this.ViewState["sortListOrder"].ToString() == "true") this.ViewState["sortListOrder"] = "false";
                else this.ViewState["sortListOrder"] = "true";
                this.listViewByQuery.Query.AutoHyperlink = true;
            }
            //this.Controls.Add(this.listViewByQuery);
            this.resultsViewPanel.ContentTemplateContainer.Controls.Add(this.listViewByQuery);
            this.Controls.Add(this.filterFieldsView);
            this.Controls.Add(this.resultsViewPanel);
        }
        public void SaveChanges()
        {
            // This method sets a flag indicating that the personalization data has changed.
            // This enables the changes to the Web Part properties from outside the Web Part class.
            this.SetPersonalizationDirty();
        }
        [WebBrowsable(true)]
        public string displayFields
        {
            get
            {
                return this._displayFields;
            }
            set
            {
                this._displayFields = value;
            }
        }
        [WebBrowsable(true)]
        public string filterColumnsNamesList
        {
            get
            {
                return this._filterColumnsNamesList;
            }
            set
            {
                this._filterColumnsNamesList = value;
            }
        }
        [WebBrowsable(true)]
        public string displayColumnsNamesList
        {
            get
            {
                return this._displayColumnsNamesList;
            }
            set
            {
                this._displayColumnsNamesList = value;
            }
        }
        [WebBrowsable(true)]
        public string filterColumnsOrdersList
        {
            get
            {
                return this._filterColumnsOrdersList;
            }
            set
            {
                this._filterColumnsOrdersList = value;
            }
        }
        [WebBrowsable(true)]
        public string displayColumnsOrdersList
        {
            get
            {
                return this._displayColumnsOrdersList;
            }
            set
            {
                this._displayColumnsOrdersList = value;
            }
        }
        [WebBrowsable(true)]
        public string filterFields
        {
            get
            {
                return this._filterFields;
            }
            set
            {
                this._filterFields = value;
            }
        }
        [WebBrowsable(true)]
        public System.Web.UI.ScriptManager ScriptManager
        {
            get
            {
                return this.scriptManager;
            }
            set
            {
                this.scriptManager = value;
            }
        }
        [WebBrowsable(true)]
        public string searchOperator
        {
            get
            {
                return this._searchOperator;
            }
            set
            {
                this._searchOperator = value;
            }
        }
        object IWebEditable.WebBrowsableObject
        {
            get
            {
                return this;
            }
        }
        [WebBrowsable(true)]
        public string strQueryOperator
        {
            get
            {
                return this._strQueryOperator;
            }
            set
            {
                this._strQueryOperator = value;
            }
        }
        [WebBrowsable(true)]
        public Dictionary<int, string> ViewDispColumnOrders
        {
            get
            {
                return this._dispColumnOrders;
            }
            set
            {
                this._dispColumnOrders = value;
            }
        }
    }
}