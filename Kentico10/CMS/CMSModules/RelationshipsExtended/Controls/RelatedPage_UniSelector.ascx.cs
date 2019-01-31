﻿using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.Relationships;
using CMS.UIControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class CMSModules_RelationshipExtended_Controls_RelatedPage_UniSelector : CMSAbstractUIWebpart
{

    private Dictionary<string, DataClassInfo> ColumnsToDataClass = new Dictionary<string, DataClassInfo>();
    private Dictionary<string, DataClassInfo> DocumentColumnsToDataClass = new Dictionary<string, DataClassInfo>();
    private Dictionary<string, string> ClassToPrimaryKeyColumn = new Dictionary<string, string>();

    public string AllowedPageTypes
    {
        get
        {
            return SqlHelper.EscapeQuotes(ValidationHelper.GetString(GetValue("AllowedPageTypes"), ""));
        }
        set
        {
            SetValue("AllowedPageTypes", value);
        }
    }

    public string WhereCondition
    {
        get
        {
            return ValidationHelper.GetString(GetValue("WhereCondition"), null);
        }
        set
        {
            SetValue("WhereCondition", value);
        }
    }

    public string RelationshipName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("RelationshipName"), "");
        }
        set
        {
            SetValue("RelationshipName", value);
        }
    }

    public int CurrentNodeID
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("CurrentNodeID"), 0);
        }
        set
        {
            SetValue("CurrentNodeID", value);
        }
    }

    public string DirectionMode
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("DirectionMode"), "LeftNode");
        }
        set
        {
            SetValue("DirectionMode", value);
        }
    }

    public bool AllowSwitchSides
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("AllowSwitchSides"), false);
        }
        set
        {
            SetValue("AllowSwitchSides", value);
        }
    }

    public int MaxRelationships
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("MaxRelationships"), -1);
        }
        set
        {
            SetValue("MaxRelationships", value);
        }
    }

    #region "Uni Selector Config"

    public string ObjectSiteName
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("ObjectSiteName"), "#current");
        }
        set
        {
            SetValue("ObjectSiteName", value);
            CustomUniSelector.ObjectSiteName = ObjectSiteName;
        }
    }

    public int SelectionMode
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("SelectionMode"), 5);
        }
        set
        {
            SetValue("SelectionMode", value);
            CustomUniSelector.SelectionMode = (SelectionModeEnum)SelectionMode;
        }
    }

    public string AdditionalColumns
    {
        get
        {
            return ValidationHelper.GetString(GetValue("AdditionalColumns"), null);
        }
        set
        {
            SetValue("AdditionalColumns", value);
            CustomUniSelector.AdditionalColumns = AdditionalColumns;
        }
    }

    public string AdditionalSearchColumns
    {
        get
        {
            return ValidationHelper.GetString(GetValue("AdditionalSearchColumns"), null);
        }
        set
        {
            SetValue("AdditionalSearchColumns", value);
            CustomUniSelector.AdditionalSearchColumns = AdditionalSearchColumns;
        }
    }

    public string OrderBy
    {
        get
        {
            return ValidationHelper.GetString(GetValue("OrderBy"), null);
        }
        set
        {
            SetValue("OrderBy", value);
            //CustomUniSelector.OrderBy = OrderBy;
        }
    }

    public string EnabledColumnName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("EnabledColumnName"), null);
        }
        set
        {
            SetValue("EnabledColumnName", value);
            CustomUniSelector.EnabledColumnName = EnabledColumnName;
        }
    }

    public int MaxDisplayedTotalItems
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("MaxDisplayedTotalItems"), 50);
        }
        set
        {
            SetValue("MaxDisplayedTotalItems", value);
            CustomUniSelector.MaxDisplayedTotalItems = MaxDisplayedTotalItems;
        }
    }

    public int MaxDisplayedItems
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("MaxDisplayedItems"), 25);
        }
        set
        {
            SetValue("MaxDisplayedItems", value);
            CustomUniSelector.MaxDisplayedItems = MaxDisplayedItems;
        }
    }

    public int ItemsPerPage
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("ItemsPerPage"), 25);
        }
        set
        {
            SetValue("ItemsPerPage", value);
            CustomUniSelector.ItemsPerPage = ItemsPerPage;
        }
    }

    public string FilterControl
    {
        get
        {
            return ValidationHelper.GetString(GetValue("FilterControl"), null);
        }
        set
        {
            SetValue("FilterControl", value);
            CustomUniSelector.FilterControl = FilterControl;
        }
    }

    public bool UseDefaultNameFilter
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("UseDefaultNameFilter"), true);
        }
        set
        {
            SetValue("UseDefaultNameFilter", value);
            CustomUniSelector.UseDefaultNameFilter = UseDefaultNameFilter;
        }
    }

    public string ToolTipFormat
    {
        get
        {
            // Sometimes it tries to processes the Macros instead of just passing the value.
            return DataHelper.GetNotEmpty(ValidationHelper.GetString(UIContext.Data.GetValue("ToolTipFormat"), ""), ValidationHelper.GetString(GetValue("ToolTipFormat"), ""));
        }
        set
        {
            SetValue("ToolTipFormat", value);
            CustomUniSelector.ToolTipFormat = value;
        }
    }

    public string DisplayNameFormat
    {
        get
        {
            // Sometimes it tries to processes the Macros instead of just passing the value.
            return DataHelper.GetNotEmpty(DataHelper.GetNotEmpty(ValidationHelper.GetString(UIContext.Data.GetValue("DisplayNameFormat"), ""), ValidationHelper.GetString(GetValue("DisplayNameFormat"), "")), "{% NodeName %}");
        }
        set
        {
            SetValue("DisplayNameFormat", value);
            CustomUniSelector.DisplayNameFormat = value;
        }
    }

    #endregion

    protected override void OnInit(EventArgs e)
    {
        

        // I believe/hope this is how you can pass the extension class
        CustomUniSelector.ContextResolver.SetNamedSourceData("UIContext", UIContext);
        CustomUniSelector.ObjectSiteName = ObjectSiteName;
        CustomUniSelector.DisplayNameFormat = DisplayNameFormat;
        CustomUniSelector.SelectionMode = (SelectionModeEnum)SelectionMode;
        if (!string.IsNullOrWhiteSpace(AdditionalColumns) || !string.IsNullOrWhiteSpace(AdditionalSearchColumns))
        {
            BuildColumnToClass();
            if(!string.IsNullOrWhiteSpace(AdditionalColumns)) { 
                CustomUniSelector.AdditionalColumns = GetActualAdditionalColumns();
            }
            if(!string.IsNullOrWhiteSpace(AdditionalSearchColumns)) { 
                CustomUniSelector.AdditionalSearchColumns = GetActualAdditionalSearchColumns();
            }
        }
        //CustomUniSelector.OrderBy = OrderBy;
        CustomUniSelector.EnabledColumnName = EnabledColumnName;
        CustomUniSelector.MaxDisplayedTotalItems = MaxDisplayedTotalItems;
        CustomUniSelector.MaxDisplayedItems = MaxDisplayedItems;
        CustomUniSelector.ItemsPerPage = ItemsPerPage;
        CustomUniSelector.FilterControl = FilterControl;
        CustomUniSelector.UseDefaultNameFilter = UseDefaultNameFilter;

        string where = "";

        where = SqlHelper.AddWhereCondition(where, string.Format("NodeID not in (Select LeftNodeID from CMS_Relationship where RightNodeID = {1} and RelationshipNameID in (Select RelationshipNameID from CMS_RelationshipName where RelationshipName = '{0}'))",
            RelationshipName, CurrentNodeID));
        where = SqlHelper.AddWhereCondition(where, string.Format("NodeID not in (Select RightNodeID from CMS_Relationship where LeftNodeID = {1} and RelationshipNameID in (Select RelationshipNameID from CMS_RelationshipName where RelationshipName = '{0}'))",
            RelationshipName, CurrentNodeID));
        where = SqlHelper.AddWhereCondition(where, string.Format("NodeID <> {0}",
            CurrentNodeID));
        if (!string.IsNullOrWhiteSpace(AllowedPageTypes))
        {
            where = SqlHelper.AddWhereCondition(where, string.Format("NodeClassID in (select ClassID from CMS_Class where ClassName in ('{0}'))",
                string.Join("','", AllowedPageTypes.Split(";| ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                ));
        }
        if (!string.IsNullOrWhiteSpace(WhereCondition))
        {
            where = SqlHelper.AddWhereCondition(where, WhereCondition);
        }
        CustomUniSelector.WhereCondition = where;

        // set direction initially
        if (SessionHelper.GetValue("RelatedPageTreeDirection_" + CurrentNodeID) == null)
        {
            SessionHelper.SetValue("RelatedPageTreeDirection_" + CurrentNodeID, DirectionMode);
        }
        ddlCurrentNodeDirection.SelectedValue = ValidationHelper.GetString(SessionHelper.GetValue("RelatedPageTreeDirection_" + CurrentNodeID), "LeftNode");
        ddlCurrentNodeDirection.Visible = AllowSwitchSides;

        // Pass custom tooltip
        CustomUniSelector.ToolTipFormat = ToolTipFormat;

        base.OnInit(e);

    }
    

    public string GetActualAdditionalColumns()
    {
        List<string> Columns = new List<string>();
        string Culture = SqlHelper.EscapeQuotes(DataHelper.GetNotEmpty(URLHelper.GetQueryValue(Request.RawUrl, "culture"), "en-US"));
        foreach (string AdditionalColumn in AdditionalColumns.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
        {
            string CleanAdditionalColumn = AdditionalColumn.ToLower().Trim();
            if (DocumentColumnsToDataClass.ContainsKey(CleanAdditionalColumn))
            {
                Columns.Add(string.Format("(select top 1 {0} from CMS_Document where DocumentNodeID = NodeID order by (case when DocumentCulture = '{1}' then 0 else 1 end)) as {0}", CleanAdditionalColumn, Culture));
            }
            else if (ColumnsToDataClass.ContainsKey(CleanAdditionalColumn))
            {
                DataClassInfo ClassObj = ColumnsToDataClass[CleanAdditionalColumn];
                DataDefinition ClassFields = new DataDefinition(ClassObj.ClassXmlSchema);
                Columns.Add(string.Format("(select top 1 {0} from {1} where {2} = (select top 1 DocumentForeignKeyValue from CMS_Document where DocumentNodeID = NodeID  order by (case when DocumentCulture = '{3}' then 0 else 1 end))) as {0}"
                    , CleanAdditionalColumn, ClassObj.ClassTableName, ClassToPrimaryKeyColumn[ClassObj.ClassName.ToLower()], Culture));
            }
            else
            {
                Columns.Add(AdditionalColumn);
            }
        }
        return string.Join(",", Columns);
    }

    public string GetActualAdditionalSearchColumns()
    {
        List<string> Columns = new List<string>();
        string Culture = SqlHelper.EscapeQuotes(DataHelper.GetNotEmpty(URLHelper.GetQueryValue(Request.RawUrl, "culture"), "en-US"));
        foreach (string AdditionalColumn in AdditionalSearchColumns.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
        {
            string CleanAdditionalColumn = AdditionalColumn.ToLower().Trim();
            if (DocumentColumnsToDataClass.ContainsKey(CleanAdditionalColumn))
            {
                Columns.Add(string.Format("(select top 1 {0} from CMS_Document where DocumentNodeID = NodeID order by (case when DocumentCulture = '{1}' then 0 else 1 end))", CleanAdditionalColumn, Culture));
            }
            else if (ColumnsToDataClass.ContainsKey(CleanAdditionalColumn))
            {
                DataClassInfo ClassObj = ColumnsToDataClass[CleanAdditionalColumn];
                Columns.Add(string.Format("(select top 1 {0} from {1} where {2} = (select top 1 DocumentForeignKeyValue from CMS_Document where DocumentNodeID = NodeID  order by (case when DocumentCulture = '{3}' then 0 else 1 end)))"
                    , CleanAdditionalColumn, ClassObj.ClassTableName, ClassToPrimaryKeyColumn[ClassObj.ClassName.ToLower()], Culture));
            }
            else
            {
                Columns.Add(AdditionalColumn);
            }
        }
        return string.Join(",", Columns);
    }

    private void BuildColumnToClass()
    {
        // Set CMS.Document first
        foreach (string AllowedClass in AllowedPageTypes.Split(";,|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)) {
            DataClassInfo PageTypeClass = DataClassInfoProvider.GetDataClassInfo(AllowedClass.Trim());
            FormInfo PageTypeFormInfo = new FormInfo(PageTypeClass.ClassFormDefinition);
            foreach (string ColumnName in PageTypeFormInfo.ItemsList.Select(x => ((FormFieldInfo)x).Name.ToLower().Trim('[').Trim(']')))
            {
                if (!ColumnsToDataClass.ContainsKey(ColumnName))
                {
                    ColumnsToDataClass.Add(ColumnName, PageTypeClass);
                }
            }
            FormFieldInfo PrimaryKeyField = (FormFieldInfo)PageTypeFormInfo.ItemsList.Where(x => ((FormFieldInfo)x).PrimaryKey).FirstOrDefault();
            if (PrimaryKeyField != null)
            {
                ClassToPrimaryKeyColumn.Add(AllowedClass.ToLower(), PrimaryKeyField.Name);
            }
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {

    }


    protected void CustomUniSelector_OnSelectionChanged(object sender, EventArgs e)
    {
        string SelectedItems = ValidationHelper.GetString(((UniSelector)sender).Value, "");
        int RelationshipNameID = RelationshipNameInfoProvider.GetRelationshipNameInfo(RelationshipName).RelationshipNameId;
        int[] NodeIDs = SelectedItems.Split(";|,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => ValidationHelper.GetInteger(x, -1)).ToArray();
        if (MaxRelationships > -1 && GetRelationshipCount() + NodeIDs.Length > MaxRelationships)
        {
            AddMessage(CMS.Base.Web.UI.MessageTypeEnum.Error, "Too many relationships, max allowed is " + MaxRelationships);
            return;
        }

        foreach (int NodeID in NodeIDs)
        {
            if (NodeID > 0)
            {
                if (ddlCurrentNodeDirection.SelectedValue == "LeftNode")
                {
                    RelationshipInfoProvider.AddRelationship(CurrentNodeID, NodeID, RelationshipNameID);
                }
                else
                {
                    RelationshipInfoProvider.AddRelationship(NodeID, CurrentNodeID, RelationshipNameID);
                }
            }
        }
        // Save direction
        SessionHelper.SetValue("RelatedPageTreeDirection_" + CurrentNodeID, ddlCurrentNodeDirection.SelectedValue);
        URLHelper.RefreshCurrentPage();
    }
    private int GetRelationshipCount()
    {
        int RelationshipNameID = RelationshipNameInfoProvider.GetRelationshipNameInfo(RelationshipName).RelationshipNameId;
        if (AllowSwitchSides)
        {
            return RelationshipInfoProvider.GetRelationships()
                                           .WhereEquals("RelationshipNameID", RelationshipNameID)
                                           .Where(string.Format("(LeftNodeID = {0} or RightNodeID = {0})", CurrentNodeID))
                                           .Count;
        }
        else
        {
            return RelationshipInfoProvider.GetRelationships()
                                           .WhereEquals("RelationshipNameID", RelationshipNameID)
                                           .WhereEquals(DirectionMode == "LeftNode" ? "LeftNodeID" : "RightNodeID", CurrentNodeID)
                                           .Count;
        }
    }
}