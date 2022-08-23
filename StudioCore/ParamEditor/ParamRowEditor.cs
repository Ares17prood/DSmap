﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Numerics;
using SoulsFormats;
using ImGuiNET;
using Veldrid;
using System.Net.Http.Headers;
using System.Security;
using System.Text.RegularExpressions;
using FSParam;
using StudioCore;
using StudioCore.Editor;

namespace StudioCore.ParamEditor
{
    public class PropertyEditor
    {
        public ActionManager ContextActionManager;
        private ParamEditorScreen _paramEditor;

        private Dictionary<string, PropertyInfo[]> _propCache = new Dictionary<string, PropertyInfo[]>();

        public PropertyEditor(ActionManager manager, ParamEditorScreen paramEditorScreen)
        {
            ContextActionManager = manager;
            _paramEditor = paramEditorScreen;
        }

        private object _editedPropCache;

        private unsafe (bool, bool) PropertyRow(Type typ, object oldval, ref object newval, bool isBool)
        {
            bool isChanged = false;
            bool isDeactivatedAfterEdit = false;
            try
            {
                if (isBool)
                {
                    dynamic val = oldval;
                    bool checkVal = val > 0;
                    if (ImGui.Checkbox("##valueBool", ref checkVal))
                    {
                        newval = Convert.ChangeType(checkVal ? 1 : 0, oldval.GetType());
                        _editedPropCache = newval;
                        isChanged = true;
                    }
                    isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit();
                    ImGui.SameLine();
                }
            }
            catch
            {

            }

            if (typ == typeof(long))
            {
                long val = (long)oldval;
                string strval = $@"{val}";
                if (ImGui.InputText("##value", ref strval, 128))
                {
                    var res = long.TryParse(strval, out val);
                    if (res)
                    {
                        newval = val;
                        _editedPropCache = newval;
                        isChanged = true;
                    }
                    else
                    {
                        _editedPropCache = null;
                    }
                }
            }
            else if (typ == typeof(int))
            {
                int val = (int)oldval;
                if (ImGui.InputInt("##value", ref val))
                {
                    newval = val;
                    _editedPropCache = newval;
                    isChanged = true;
                }
            }
            else if (typ == typeof(uint))
            {
                uint val = (uint)oldval;
                string strval = $@"{val}";
                if (ImGui.InputText("##value", ref strval, 16))
                {
                    var res = uint.TryParse(strval, out val);
                    if (res)
                    {
                        newval = val;
                        _editedPropCache = newval;
                        isChanged = true;
                    }
                    else
                    {
                        _editedPropCache = null;
                    }
                }
            }
            else if (typ == typeof(short))
            {
                int val = (short)oldval;
                if (ImGui.InputInt("##value", ref val))
                {
                    newval = (short)val;
                    _editedPropCache = newval;
                    isChanged = true;
                }
            }
            else if (typ == typeof(ushort))
            {
                ushort val = (ushort)oldval;
                string strval = $@"{val}";
                if (ImGui.InputText("##value", ref strval, 5))
                {
                    var res = ushort.TryParse(strval, out val);
                    if (res)
                    {
                        newval = val;
                        _editedPropCache = newval;
                        isChanged = true;
                    }
                    else
                    {
                        _editedPropCache = null;
                    }
                }
            }
            else if (typ == typeof(sbyte))
            {
                int val = (sbyte)oldval;
                if (ImGui.InputInt("##value", ref val))
                {
                    newval = (sbyte)val;
                    _editedPropCache = newval;
                    isChanged = true;
                }
            }
            else if (typ == typeof(byte))
            {
                byte val = (byte)oldval;
                string strval = $@"{val}";
                if (ImGui.InputText("##value", ref strval, 3))
                {
                    var res = byte.TryParse(strval, out val);
                    if (res)
                    {
                        newval = val;
                        _editedPropCache = newval;
                        isChanged = true;
                    }
                    else
                    {
                        _editedPropCache = null;
                    }
                }
            }
            else if (typ == typeof(bool))
            {
                bool val = (bool)oldval;
                if (ImGui.Checkbox("##value", ref val))
                {
                    newval = val;
                    _editedPropCache = newval;
                    isChanged = true;
                }
            }
            else if (typ == typeof(float))
            {
                float val = (float)oldval;
                if (ImGui.DragFloat("##value", ref val, 0.1f))
                {
                    newval = val;
                    _editedPropCache = newval;
                    isChanged = true;
                    // shouldUpdateVisual = true;
                }
            }
            else if (typ == typeof(double))
            {
                double val = (double)oldval;
                if (ImGui.DragScalar("##value", ImGuiDataType.Double, new IntPtr(&val), 0.1f))
                {
                    newval = val;
                    _editedPropCache = newval;
                    return (true, ImGui.IsItemDeactivatedAfterEdit());
                }
            }
            else if (typ == typeof(string))
            {
                string val = (string)oldval;
                if (val == null)
                {
                    val = "";
                }
                if (ImGui.InputText("##value", ref val, 128))
                {
                    newval = val;
                    _editedPropCache = newval;
                    isChanged = true;
                }
            }
            else if (typ == typeof(Vector2))
            {
                Vector2 val = (Vector2)oldval;
                if (ImGui.DragFloat2("##value", ref val, 0.1f))
                {
                    newval = val;
                    _editedPropCache = newval;
                    isChanged = true;
                    // shouldUpdateVisual = true;
                }
            }
            else if (typ == typeof(Vector3))
            {
                Vector3 val = (Vector3)oldval;
                if (ImGui.DragFloat3("##value", ref val, 0.1f))
                {
                    newval = val;
                    _editedPropCache = newval;
                    isChanged = true;
                    // shouldUpdateVisual = true;
                }
            }
            else if (typ == typeof(Byte[]))
            {

                Byte[] bval = (Byte[])oldval;
                string val = ParamUtils.Dummy8Write(bval);
                if (ImGui.InputText("##value", ref val, 128))
                {
                    Byte[] nval = ParamUtils.Dummy8Read(val, bval.Length);
                    if (nval!=null)
                    {
                        newval = nval;
                        _editedPropCache = newval;
                        isChanged = true;
                    }
                    else
                    {
                        _editedPropCache = null;
                    }
                }
            }
            else
            {
                // Using InputText means IsItemDeactivatedAfterEdit doesn't pick up random previous item
                string implMe = "ImplementMe";
                ImGui.InputText(null, ref implMe, 256, ImGuiInputTextFlags.ReadOnly);
            }
            isDeactivatedAfterEdit |= ImGui.IsItemDeactivatedAfterEdit();

            return (isChanged, isDeactivatedAfterEdit);
        }

        private void UpdateProperty(object prop, object obj, object newval,
            bool changed, bool committed, int arrayindex = -1)
        {
            if (changed)
            {
                ChangeProperty(prop, obj, newval, ref committed, arrayindex);
            }
        }

        private void ChangeProperty(object prop, object obj, object newval,
            ref bool committed, int arrayindex = -1)
        {
            if (committed)
            {
                if (newval == null)
                {
                    // Safety check warned to user, should have proper crash handler instead
                    TaskManager.warningList["ParamRowEditorPropertyChangeError"] = "ParamRowEditor: Property changed was null";
                    return;
                }
                PropertiesChangedAction action;
                if (arrayindex != -1)
                {
                    action = new PropertiesChangedAction((PropertyInfo)prop, arrayindex, obj, newval);
                }
                else
                {
                    action = new PropertiesChangedAction((PropertyInfo)prop, obj, newval);
                }
                ContextActionManager.ExecuteAction(action);
            }
        }

        public void PropEditorParamRow(ParamBank bank, Param.Row row, Param.Row vrow, Param.Row crow, ref string propSearchString, string activeParam, bool isActiveView)
        {
            ParamMetaData meta = ParamMetaData.Get(row.Def);
            int id = 0;
            bool showCompare = crow != null;

            if (propSearchString != null)
            {
                if (isActiveView && InputTracker.GetControlShortcut(Key.N))
                    ImGui.SetKeyboardFocusHere();
                ImGui.InputText("Search For Field <Ctrl+N>", ref propSearchString, 255);
                ImGui.Separator();
            }
            Regex propSearchRx = null;
            try
            {
                propSearchRx = new Regex(propSearchString.ToLower());
            }
            catch
            {
            }
            ImGui.BeginChild("Param Fields");
            int columnCount = 2;
            if (ParamEditorScreen.ShowVanillaParamsPreference)
                columnCount++;
            if (showCompare)
                columnCount++;
            ImGui.Columns(columnCount);
            ImGui.NextColumn();
            ImGui.NextColumn();
            if (ParamEditorScreen.ShowVanillaParamsPreference)
                ImGui.NextColumn();
            if (showCompare)
                ImGui.NextColumn();

            // This should be rewritten somehow it's super ugly
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            var nameProp = row.GetType().GetProperty("Name");
            var idProp = row.GetType().GetProperty("ID");
            PropEditorPropInfoRow(bank, row, vrow, crow, showCompare, nameProp, "Name", ref id);
            PropEditorPropInfoRow(bank, row, vrow, crow, showCompare, idProp, "ID", ref id);
            ImGui.PopStyleColor();
            ImGui.Separator();

            List<string> pinnedFields = new List<string>(_paramEditor._projectSettings.PinnedFields.GetValueOrDefault(activeParam, new List<string>()));
            if (pinnedFields.Count > 0)
            {
                foreach (var field in pinnedFields)
                {
                    List<Param.Column> matches = row.Cells.Where(cell => cell.Def.InternalName == field).ToList();
                    List<Param.Column> vmatches = vrow?.Cells.Where(cell => cell.Def.InternalName == field).ToList();
                    List<Param.Column> cmatches = crow?.Cells.Where(cell => cell.Def.InternalName == field).ToList();
                    for (int i = 0; i < matches.Count; i++)
                        PropEditorPropCellRow(bank, row[matches[i]], vrow?[vmatches[i]], crow?[cmatches[i]], showCompare, ref id, propSearchRx, activeParam, true);
                }
                ImGui.Separator();
            }
            List<string> fieldOrder = meta != null && meta.AlternateOrder != null && ParamEditorScreen.AllowFieldReorderPreference ? meta.AlternateOrder : new List<string>();
            foreach (PARAMDEF.Field field in row.Def.Fields)
            {
                if (!fieldOrder.Contains(field.InternalName))
                    fieldOrder.Add(field.InternalName);
            }
            bool lastRowExists = false;
            foreach (var field in fieldOrder)
            {
                if (field.Equals("-") && lastRowExists)
                {
                    ImGui.Separator();
                    lastRowExists = false;
                    continue;
                }
                if (row[field] == null)
                    continue;
                List<Param.Column> matches = row.Cells.Where(cell => cell.Def.InternalName == field).ToList();
                List<Param.Column> vmatches = vrow?.Cells.Where(cell => cell.Def.InternalName == field).ToList();
                List<Param.Column> cmatches = crow?.Cells.Where(cell => cell.Def.InternalName == field).ToList();
                for (int i = 0; i < matches.Count; i++)
                    lastRowExists |= PropEditorPropCellRow(bank, row[matches[i]], vrow?[vmatches[i]], crow?[cmatches[i]], showCompare, ref id, propSearchRx, activeParam, false);
            }
            ImGui.Columns(1);
            ImGui.EndChild();
        }

        // Many parameter options, which may be simplified.
        private void PropEditorPropInfoRow(ParamBank bank, Param.Row row, Param.Row vrow, Param.Row crow, bool showCompare, PropertyInfo prop, string visualName, ref int id)
        {
            PropEditorPropRow(bank, prop.GetValue(row), vrow != null ? prop.GetValue(vrow) : null, crow != null ? prop.GetValue(crow) : null, showCompare, ref id, visualName, null, prop.PropertyType, prop, null, row, null, null, false);
        }
        private bool PropEditorPropCellRow(ParamBank bank, Param.Cell cell, Param.Cell? vcell, Param.Cell? ccell, bool showCompare, ref int id, Regex propSearchRx, string activeParam, bool isPinned)
        {
            return PropEditorPropRow(
                bank,
                cell.Value,
                vcell?.Value,
                ccell?.Value,
                showCompare,
                ref id, cell.Def.InternalName,
                FieldMetaData.Get(cell.Def),
                cell.Value.GetType(),
                cell.GetType().GetProperty("Value"),
                cell, null, propSearchRx, activeParam, isPinned);
        }
        private bool PropEditorPropRow(ParamBank bank, object oldval, object vanillaval, object compareval, bool showCompare, ref int id, string internalName, FieldMetaData cellMeta, Type propType, PropertyInfo proprow, Param.Cell? nullableCell, Param.Row? nullableRow, Regex propSearchRx, string activeParam, bool isPinned)
        {
            List<string> RefTypes = cellMeta?.RefTypes;
            string VirtualRef = cellMeta?.VirtualRef;
            ParamEnum Enum = cellMeta?.EnumType;
            string Wiki = cellMeta?.Wiki;
            bool IsBool = cellMeta?.IsBool ?? false;
            string AltName = cellMeta?.AltName;

            if (propSearchRx != null)
            {
                if (!propSearchRx.IsMatch(internalName.ToLower()) && !(AltName != null && propSearchRx.IsMatch(AltName.ToLower())))
                {
                    return false;
                }
            }

            object newval = null;
            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();
            PropertyRowName(ref internalName, cellMeta);
            PropertyRowNameContextMenu(bank, internalName, cellMeta, activeParam, activeParam != null, isPinned);
            if (Wiki != null)
            {
                if (EditorDecorations.HelpIcon(internalName, ref Wiki, true))
                    cellMeta.Wiki = Wiki;
            }

            EditorDecorations.ParamRefText(RefTypes);
            EditorDecorations.EnumNameText(Enum == null ? null : Enum.name);

            //PropertyRowMetaDefContextMenu();
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            bool changed = false;
            bool committed = false;

            bool diffVanilla = vanillaval != null && !(oldval.Equals(vanillaval) || (propType == typeof(byte[]) && ParamUtils.ByteArrayEquals((byte[])oldval, (byte[])vanillaval)));
            bool diffCompare = compareval != null && !(oldval.Equals(compareval) || (propType == typeof(byte[]) && ParamUtils.ByteArrayEquals((byte[])oldval, (byte[])compareval)));
            //bool diffCompare = vanillaval != null && compareval != null && !(compareval.Equals(vanillaval) || (propType == typeof(byte[]) && ParamUtils.ByteArrayEquals((byte[])compareval, (byte[])vanillaval)));
            //bool diffConclift = diffVanilla && diffCompare;

            bool matchDefault = nullableCell?.Def.Default != null && nullableCell.Value.Def.Default.Equals(oldval);
            bool isRef = (ParamEditorScreen.HideReferenceRowsPreference == false && RefTypes != null) || (ParamEditorScreen.HideEnumsPreference == false && Enum != null) || VirtualRef != null;
            if (diffVanilla)
                ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.22f, 0.2f, 1f));
            if (isRef)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.5f, 1.0f, 1.0f));
            else if (matchDefault)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.75f, 0.75f, 0.75f, 1.0f));
            (changed, committed) = PropertyRow(propType, oldval, ref newval, IsBool);
            //bool committed = true;
            if (isRef || matchDefault) //if diffVanilla, remove styling later
                ImGui.PopStyleColor();

            PropertyRowValueContextMenu(bank, internalName, VirtualRef, oldval);

            if (ParamEditorScreen.HideReferenceRowsPreference == false && RefTypes != null)
                EditorDecorations.ParamRefsSelectables(bank, RefTypes, oldval);
            if (ParamEditorScreen.HideEnumsPreference == false && Enum != null)
                EditorDecorations.EnumValueText(Enum.values, oldval.ToString());

            if (ParamEditorScreen.HideReferenceRowsPreference == false || ParamEditorScreen.HideEnumsPreference == false)
            {
                if (EditorDecorations.ParamRefEnumContextMenu(bank, oldval, ref newval, RefTypes, Enum))
                {
                    changed = true;
                    committed = true;
                    _editedPropCache = newval;
                }
            }
            if (diffVanilla)
                ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.180f, 0.180f, 0.196f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.9f, 0.9f, 0.9f, 1.0f));
            if (ParamEditorScreen.ShowVanillaParamsPreference)
            {
                ImGui.NextColumn();
                AdditionalColumnValue(vanillaval, propType, bank, RefTypes, Enum);
            }
            if (showCompare)
            {
                if(diffCompare)
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.236f, 1f));
                ImGui.NextColumn();
                AdditionalColumnValue(compareval, propType, bank, RefTypes, Enum);
                if (diffCompare)
                    ImGui.PopStyleColor();
            }
            ImGui.PopStyleColor(2);

            if (_editedPropCache != null && _editedPropCache != oldval)
            {
                changed = true;
            }

            UpdateProperty(proprow, nullableCell != null ? (object)nullableCell : nullableRow, _editedPropCache, changed, committed);
            ImGui.NextColumn();
            ImGui.PopID();
            id++;
            return true;
        }

        private static void AdditionalColumnValue(object colVal, Type propType, ParamBank bank, List<string> RefTypes, ParamEnum Enum)
        {
            if (colVal == null)
                    ImGui.TextUnformatted("");
                else
                {
                    string value = "";
                    if (propType == typeof(byte[]))
                        value = ParamUtils.Dummy8Write((byte[])colVal);
                    else
                        value = colVal.ToString();
                    ImGui.InputText("", ref value, 256, ImGuiInputTextFlags.ReadOnly);
                    if (ParamEditorScreen.HideReferenceRowsPreference == false && RefTypes != null)
                        EditorDecorations.ParamRefsSelectables(bank, RefTypes, colVal);
                    if (ParamEditorScreen.HideEnumsPreference == false && Enum != null)
                        EditorDecorations.EnumValueText(Enum.values, colVal.ToString());
                }
        }

        private void PropertyRowName(ref string internalName, FieldMetaData cellMeta)
        {
            string AltName = cellMeta == null ? null : cellMeta.AltName;
            if (cellMeta != null && ParamEditorScreen.EditorMode)
            {
                string EditName = AltName == null ? internalName : AltName;
                ImGui.InputText("", ref EditName, 128);
                if (EditName.Equals(internalName) || EditName.Equals(""))
                    cellMeta.AltName = null;
                else
                    cellMeta.AltName = EditName;
            }
            else
            {
                string printedName = (AltName != null && ParamEditorScreen.ShowAltNamesPreference) ? (ParamEditorScreen.AlwaysShowOriginalNamePreference ? $"{internalName} ({AltName})" : $"{AltName}*") : internalName;
                ImGui.TextUnformatted(printedName);
            }
        }

        private void PropertyRowNameContextMenu(ParamBank bank, string originalName, FieldMetaData cellMeta, string activeParam, bool showPinOptions, bool isPinned)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 10f));
            if (ImGui.BeginPopupContextItem("rowName"))
            {
                if (ParamEditorScreen.ShowAltNamesPreference == true && ParamEditorScreen.AlwaysShowOriginalNamePreference == false)
                {
                    ImGui.TextColored(new Vector4(1f, .7f, .4f, 1f), originalName);
                    ImGui.Separator();
                }
                if (ImGui.MenuItem("Add to Searchbar"))
                {
                    EditorCommandQueue.AddCommand($@"param/search/prop {originalName.Replace(" ", "\\s")} ");
                }
                if (showPinOptions && ImGui.MenuItem((isPinned ? "Unpin " : "Pin " + originalName)))
                {
                    if (!_paramEditor._projectSettings.PinnedFields.ContainsKey(activeParam))
                        _paramEditor._projectSettings.PinnedFields.Add(activeParam, new List<string>());
                    List<string> pinned = _paramEditor._projectSettings.PinnedFields[activeParam];
                    if (isPinned)
                        pinned.Remove(originalName);
                    else if (!pinned.Contains(originalName))
                        pinned.Add(originalName);
                }
                if (ParamEditorScreen.EditorMode && cellMeta != null)
                {
                    if (ImGui.BeginMenu("Add Reference"))
                    {
                        foreach (string p in bank.Params.Keys)
                        {
                            if (ImGui.MenuItem(p+"##add"+p))
                            {
                                if (cellMeta.RefTypes == null)
                                    cellMeta.RefTypes = new List<string>();
                                cellMeta.RefTypes.Add(p);
                            }
                        }
                        ImGui.EndMenu();
                    }
                    if (cellMeta.RefTypes != null && ImGui.BeginMenu("Remove Reference"))
                    {
                        foreach (string p in cellMeta.RefTypes)
                        {
                            if (ImGui.MenuItem(p+"##remove"+p))
                            {
                                cellMeta.RefTypes.Remove(p);
                                if (cellMeta.RefTypes.Count == 0)
                                    cellMeta.RefTypes = null;
                                break;
                            }
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.MenuItem(cellMeta.IsBool ? "Remove bool toggle" : "Add bool toggle"))
                        cellMeta.IsBool = !cellMeta.IsBool;
                    if (cellMeta.Wiki == null && ImGui.MenuItem("Add wiki..."))
                        cellMeta.Wiki = "Empty wiki...";
                    if (cellMeta.Wiki != null && ImGui.MenuItem("Remove wiki"))
                        cellMeta.Wiki = null;
                }
                ImGui.EndPopup();
            }
            ImGui.PopStyleVar();
        }
        private void PropertyRowValueContextMenu(ParamBank bank, string internalName, string VirtualRef, dynamic oldval)
        {
            if (ImGui.BeginPopupContextItem("quickMEdit"))
            {
                if (ImGui.Selectable("Edit all selected..."))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditRegex/selection: {Regex.Escape(internalName)}: ");
                }
                if (VirtualRef != null)
                    EditorDecorations.VirtualParamRefSelectables(bank, VirtualRef, oldval);
                if (ParamEditorScreen.EditorMode && ImGui.BeginMenu("Find rows with this value..."))
                {
                    foreach (KeyValuePair<string, Param> p in bank.Params)
                    {
                        int v = (int)oldval;
                        Param.Row r = p.Value[v];
                        if (r != null && ImGui.Selectable($@"{p.Key}: {Utils.ImGuiEscape(r.Name, "null")}"))
                            EditorCommandQueue.AddCommand($@"param/select/-1/{p.Key}/{v}");
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndPopup();
            }
        }        
    }
}