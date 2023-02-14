﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FSParam;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using ImGuiNET;
using SoulsFormats;
using StudioCore;
using StudioCore.TextEditor;
using StudioCore.Editor;
using StudioCore.MsbEditor;
using ActionManager = StudioCore.Editor.ActionManager;
using AddParamsAction = StudioCore.Editor.AddParamsAction;
using CompoundAction = StudioCore.Editor.CompoundAction;
using DeleteParamsAction = StudioCore.Editor.DeleteParamsAction;
using EditorScreen = StudioCore.Editor.EditorScreen;

namespace StudioCore.ParamEditor
{
    class AutoFillSearchEngine<A, B>
    {
        SearchEngine<A, B> engine;
        private string[] _autoFillArgs;
        private static bool _autoFillNotToggle;
        internal AutoFillSearchEngine(SearchEngine<A, B> searchEngine)
        {
            engine = searchEngine;
            _autoFillArgs = Enumerable.Repeat("", engine.AvailableCommands().Sum((x) => x.Item2.Length) + engine.defaultFilter.Item1.Length).ToArray();
            _autoFillNotToggle = false;
        }
        internal string MassEditAutoFillForSearchEngine(bool enableDefault, string suffix, Func<string> subMenu)
        {
            int currentArgIndex = 0;
            string result = null;
            ImGui.Checkbox("Invert selection?##meautoinputnottoggle", ref _autoFillNotToggle);
            foreach (var cmd in enableDefault ? engine.AvailableCommands().Append((null, engine.defaultFilter.Item1)).ToList() : engine.AvailableCommands())
            {
                int[] argIndices = new int[cmd.Item2.Length];
                bool valid = true;
                for (int i = 0; i < argIndices.Length; i++)
                {
                    argIndices[i] = currentArgIndex;
                    currentArgIndex++;
                    if (string.IsNullOrEmpty(_autoFillArgs[argIndices[i]]))
                        valid = false;
                }
                if (subMenu != null)
                {
                    if (ImGui.BeginMenu(cmd.Item1 == null ? "Default filter..." : cmd.Item1, valid))
                    {
                        result = subMenu();
                        ImGui.EndMenu();
                    }
                }
                else
                {
                    result = ImGui.Selectable(cmd.Item1 == null ? "Default filter..." : cmd.Item1, false, valid ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled) ? suffix : null;
                }
                //ImGui.Indent();
                for (int i = 0; i < argIndices.Length; i++)
                {
                    if (i != 0)
                        ImGui.SameLine();
                    ImGui.InputTextWithHint("##meautoinput"+argIndices[i], cmd.Item2[i], ref _autoFillArgs[argIndices[i]], 256);
                }
                //ImGui.Unindent();
                if (result != null && valid)
                {
                    if (cmd.Item1 != null)
                    {
                        string cmdText = _autoFillNotToggle ? '!' + cmd.Item1 : cmd.Item1;
                        for (int i = 0; i < argIndices.Length; i++)
                            cmdText += " " + _autoFillArgs[argIndices[i]];
                        result = cmdText + suffix + result;
                    }
                    else if (argIndices.Length > 0)
                    {
                        string argText = _autoFillArgs[argIndices[0]];
                        for (int i = 1; i < argIndices.Length; i++)
                            argText += " " + _autoFillArgs[argIndices[i]];
                        result = argText + suffix + result;
                    }
                    return result;
                }
            }
            return result;
        }
    }

    class AutoFill
    {
        // Type hell. Can't omit the type.
        static AutoFillSearchEngine<ParamEditorSelectionState, (MassEditRowSource, Param.Row)> autoFillParse = new (ParamAndRowSearchEngine.parse);
        static AutoFillSearchEngine<bool, (ParamBank, Param)> autoFillPse = new (ParamSearchEngine.pse);
        static AutoFillSearchEngine<(ParamBank, Param), Param.Row> autoFillRse = new (RowSearchEngine.rse);
        static AutoFillSearchEngine<(string, Param.Row), (PseudoColumn, Param.Column)> autoFillCse = new (CellSearchEngine.cse);
        private static string[] _autoFillArgsRop = Enumerable.Repeat("", MERowOperation.rowOps.AvailableCommands().Sum((x) => x.Item2.Length)).ToArray();
        private static string[] _autoFillArgsCop = Enumerable.Repeat("", MECellOperation.cellOps.AvailableCommands().Sum((x) => x.Item2.Length)).ToArray();
        private static string[] _autoFillArgsOa = Enumerable.Repeat("", MEOperationArgument.arg.AvailableArguments().Sum((x) => x.Item2.Length)).ToArray();
        
        public static string ParamSearchBarAutoFill()
        {
            ImGui.SameLine();
            ImGui.Button($@"{ForkAwesome.CaretDown}");
            if (ImGui.BeginPopupContextItem("##rsbautoinputoapopup", ImGuiPopupFlags.MouseButtonLeft))
            {
                ImGui.TextUnformatted("Select params...");
                var result = autoFillPse.MassEditAutoFillForSearchEngine(false, "", null);
                ImGui.EndPopup();
                return result;
            }
            return null;
        }

        public static string RowSearchBarAutoFill()
        {
            ImGui.SameLine();
            ImGui.Button($@"{ForkAwesome.CaretDown}");
            if (ImGui.BeginPopupContextItem("##rsbautoinputoapopup", ImGuiPopupFlags.MouseButtonLeft))
            {
                ImGui.TextUnformatted("Select rows...");
                var result = autoFillRse.MassEditAutoFillForSearchEngine(false, "", null);
                ImGui.EndPopup();
                return result;
            }
            return null;
        }

        public static string MassEditAutoFill()
        {
            ImGui.TextUnformatted("Add command...");
            ImGui.SameLine();
            ImGui.Button($@"{ForkAwesome.CaretDown}");
            if (ImGui.BeginPopupContextItem("##meautoinputoapopup", ImGuiPopupFlags.MouseButtonLeft))
            {
                ImGui.TextUnformatted("Select param and rows...");
                string result1 = autoFillParse.MassEditAutoFillForSearchEngine(false, ": ", () => 
                {
                    ImGui.TextUnformatted("Select fields...");
                    string res1 = autoFillCse.MassEditAutoFillForSearchEngine(true, ": ", () => 
                    {
                        ImGui.TextUnformatted("Select field operation...");
                        return MassEditAutoFillForOperation(MECellOperation.cellOps, ref _autoFillArgsCop, ";", null);
                    });
                    ImGui.Separator();
                    ImGui.TextUnformatted("Select row operation...");
                    string res2 = MassEditAutoFillForOperation(MERowOperation.rowOps, ref _autoFillArgsRop, ";", null);
                    if (res1 != null)
                        return res1;
                    return res2;
                });
                ImGui.Separator();
                ImGui.TextUnformatted("Select params...");
                string result2 = autoFillPse.MassEditAutoFillForSearchEngine(false, ": ", () =>
                {
                    ImGui.TextUnformatted("Select rows...");
                    return autoFillRse.MassEditAutoFillForSearchEngine(false, ": ", () => 
                    {
                        ImGui.TextUnformatted("Select fields...");
                        string res1 = autoFillCse.MassEditAutoFillForSearchEngine(true, ": ", () => 
                        {
                            ImGui.TextUnformatted("Select field operation...");
                            return MassEditAutoFillForOperation(MECellOperation.cellOps, ref _autoFillArgsCop, ";", null);
                        });
                        ImGui.Separator();
                        ImGui.TextUnformatted("Select row operation...");
                        string res2 = MassEditAutoFillForOperation(MERowOperation.rowOps, ref _autoFillArgsRop, ";", null);
                        if (res1 != null)
                            return res1;
                        return res2;
                    });
                });
                ImGui.EndPopup();
                if (result1 != null)
                    return result1;
                return result2;
            }
            return null;
        }
        private static string MassEditAutoFillForOperation<A, B> (MEOperation<A, B> ops, ref string[] staticArgs, string suffix, Func<string> subMenu)
        {
            int currentArgIndex = 0;
            string result = null;
            foreach (var cmd in ops.AvailableCommands())
            {
                int[] argIndices = new int[cmd.Item2.Length];
                bool valid = true;
                for (int i = 0; i < argIndices.Length; i++)
                {
                    argIndices[i] = currentArgIndex;
                    currentArgIndex++;
                    if (string.IsNullOrEmpty(staticArgs[argIndices[i]]))
                        valid = false;
                }
                if (subMenu != null)
                {
                    if (ImGui.BeginMenu(cmd.Item1, valid))
                    {
                        result = subMenu();
                        ImGui.EndMenu();
                    }
                }
                else
                {
                    result = ImGui.Selectable(cmd.Item1, false, valid ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled) ? suffix : null;
                }
                ImGui.Indent();
                for (int i = 0; i < argIndices.Length; i++)
                {
                    if (i != 0)
                        ImGui.SameLine();
                    ImGui.InputTextWithHint("##meautoinputop"+argIndices[i], cmd.Item2[i], ref staticArgs[argIndices[i]], 256);
                    ImGui.SameLine();
                    ImGui.Button($@"{ForkAwesome.CaretDown}");
                    if (ImGui.BeginPopupContextItem("##meautoinputoapopup"+argIndices[i], ImGuiPopupFlags.MouseButtonLeft))
                    {
                        string opargResult = MassEditAutoFillForArguments(MEOperationArgument.arg, ref _autoFillArgsOa);
                        if (opargResult != null)
                            staticArgs[argIndices[i]] = opargResult;
                        ImGui.EndPopup();
                    }

                }
                ImGui.Unindent();
                if (result != null && valid)
                {
                    string argText = argIndices.Length > 0 ? staticArgs[argIndices[0]] : null;
                    for (int i = 1; i < argIndices.Length; i++)
                        argText += ":" + staticArgs[argIndices[i]];
                    result = cmd.Item1 + (argText != null ? (" " + argText + result) : result);
                    return result;
                }
            }
            return result;
        }

        private static string MassEditAutoFillForArguments(MEOperationArgument oa, ref string[] staticArgs)
        {
            int currentArgIndex = 0;
            string result = null;
            foreach (var arg in oa.AvailableArguments())
            {
                int[] argIndices = new int[arg.Item2.Length];
                bool valid = true;
                for (int i = 0; i < argIndices.Length; i++)
                {
                    argIndices[i] = currentArgIndex;
                    currentArgIndex++;
                    if (string.IsNullOrEmpty(staticArgs[argIndices[i]]))
                        valid = false;
                }
                bool selected = false;
                if (ImGui.Selectable(arg.Item1, selected, valid ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                {
                    result = arg.Item1;
                    string argText = "";
                    for (int i = 0; i < argIndices.Length; i++)
                        argText += " " + staticArgs[argIndices[i]];
                    return arg.Item1 + argText;
                }
                ImGui.Indent();
                for (int i = 0; i < argIndices.Length; i++)
                {
                    if (i != 0)
                        ImGui.SameLine();
                    ImGui.InputTextWithHint("##meautoinputoa"+argIndices[i], arg.Item2[i], ref staticArgs[argIndices[i]], 256);
                }
                ImGui.Unindent();
            }
            return result;
        }
    }
}
