using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using System.Web;

namespace EditorPartTab.ControlTemplates.Maxim.WebParts.ListSearch
{
    public partial class ListSearchEditor : UserControl
    {
        private ListSearchEditorPart parentEditorPart;
        private string filterColumnsList;
        private string displayColumnsList;
        public System.Collections.Generic.List<ListSearchData> TabList;
        public ListSettings MaximList;
        const string ListStorageViewStateId = "ListStorageViewState";
        const string TabStorageViewStateId = "TabStorageViewState";
        protected void Page_Load(object sender, EventArgs e)
        {
            this.parentEditorPart = this.Parent as ListSearchEditorPart;
            // Call Sync Changes on the editor part, to read the tab list from the Web Part.
            this.parentEditorPart.SyncChanges();
            //this.TabList = this.parentEditorPart.TabList;
            this.MaximList = this.parentEditorPart.MSettings;
            //this.listCollection.SelectedValue = this.MaximList.ListName;
            // Check whether this is the first Page_Load of the control.
            if (this.hiddenFieldDetectRequest.Value == "0")
            {
                this.hiddenFieldDetectRequest.Value = "1";
                // Save the original tab list to the control's ViewState.
                this.SaveOriginalTabListToViewState();
                // Bind the tab list to the drop-down.
                PopulateList();
                try
                {
                    this.PopulateConfigures();
                }
                catch { }
            }
        }
        private void SaveOriginalTabListToViewState()
        {
            // Save the tab list that was already retrieved 
            // from the Web Part storage to the view state.
            if (this.TabList != null)
            {
                this.ViewState[TabStorageViewStateId] = this.TabList;
            }
            if (this.MaximList != null)
            {
                this.ViewState[ListStorageViewStateId] = this.MaximList;
            }
        }
        private void PopulateConfigures()
        {
            this.listCollection.SelectedValue = this.MaximList.ListName;
            PopulateFields();
            foreach (ListItem li in this.filterColumns.Items)
            {
                if (this.MaximList.filterFields.Contains(li.Value)) li.Selected = true;
            }
            foreach (ListItem li in this.displayColumns.Items)
            {
                if (this.MaximList.displayFields.Contains(li.Value)) li.Selected = true;
            }
            //this.txtDisplayNames.Text = this.MaximList.displayFieldsNames;
            this.txtDisplayOrders.Text = this.MaximList.displayFieldsOrders;
            this.txtFilterNames.Text = this.MaximList.filterFieldsNames;
            this.txtFilterOrders.Text = this.MaximList.filterFieldsOrders;
        }
        public List<ListSearchData> OriginalTabList
        {
            get
            {
                // Retrieve the original tab list from the ViewState.
                List<ListSearchData> retValue = null;
                retValue = this.ViewState[TabStorageViewStateId] as List<ListSearchData>;
                return retValue;
            }
        }
        public ListSettings OriginalMaximList
        {
            get
            {
                // Retrieve the original tab list from the ViewState.
                ListSettings retValue = null;
                retValue = this.ViewState[ListStorageViewStateId] as ListSettings;
                return retValue;
            }
        }
        protected void sel_SPList_OnTextChanged(object sender, EventArgs e)
        {
            this.PopulateFields();
        }
        private void PopulateList()
        {
            using (SPSite site = new SPSite(HttpContext.Current.Request.Url.ToString()))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    this.listCollection.Items.Clear();
                    foreach (SPList list in web.Lists)
                    {
                        if (!list.Hidden)
                        {
                            this.listCollection.Items.Add(list.Title);
                        }
                    }
                }
            }
        }
        private void PopulateFields()
        {
            using (SPSite site = new SPSite(HttpContext.Current.Request.Url.ToString()))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    this.filterColumns.Items.Clear();
                    this.displayColumns.Items.Clear();
                    SPList list = web.Lists[this.listCollection.SelectedValue];
                    List<string> sortedList = new List<string>();
                    foreach (SPField field in list.Fields)
                    {
                        //if (field.InternalName == "Title")
                        //{
                        //    //this.filterColumns.Items.Add(field.Title);
                        //    //this.displayColumns.Items.Add(field.Title);
                        //    sortedList.Add(field.Title);
                        //}
                        if (!(field.FromBaseType || field.Hidden))
                        {
                            sortedList.Add(field.Title);
                        }
                    }
                    sortedList.Sort();
                    this.filterColumns.DataSource = sortedList;
                    this.filterColumns.DataBind();
                    this.displayColumns.DataSource = sortedList;
                    this.displayColumns.DataBind();
                }
            }
        }
        protected void ButtonSave_Click(object sender, EventArgs e)
        {
            this.SaveTab();
            this.ApplyChanges();
            return;
        }
        private void SaveTab()
        {
            MaximList = new ListSettings();
            this.filterColumnsList = string.Empty;
            this.displayColumnsList = string.Empty;
            for (int num = 0; num <= (this.filterColumns.Items.Count - 1); num++)
            {
                if (this.filterColumns.Items[num].Selected)
                {
                    this.filterColumnsList = this.filterColumnsList + this.filterColumns.Items[num].Text + ";";
                }
            }
            for (int num = 0; num <= (this.displayColumns.Items.Count - 1); num++)
            {
                if (this.displayColumns.Items[num].Selected)
                {
                    this.displayColumnsList = this.displayColumnsList + this.displayColumns.Items[num].Text + ";";
                }
            }
            MaximList.filterFields = this.filterColumnsList;
            MaximList.displayFields = this.displayColumnsList;
            MaximList.ListName = this.listCollection.SelectedValue;
            //MaximList.displayFieldsNames = this.txtDisplayNames.Text;
            MaximList.displayFieldsOrders = this.txtDisplayOrders.Text;
            MaximList.filterFieldsNames = this.txtFilterNames.Text;
            MaximList.filterFieldsOrders = this.txtFilterOrders.Text;
        }
        public void Sync_Changes()
        {
            this.EnsureChildControls();         
        }
        private void ApplyChanges()
        {
            this.SaveTab();
            this.parentEditorPart.MSettings = this.MaximList;
            // Call the ApplyChanges method of the parent editor part class.
            this.parentEditorPart.ApplyChanges();
        }
    }
}