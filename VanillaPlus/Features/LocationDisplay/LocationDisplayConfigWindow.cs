using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace VanillaPlus.Features.LocationDisplay;

public class LocationDisplayConfigWindow(LocationDisplayConfig config, Action onConfigChanged) : Window("位置显示设置窗口", ImGuiWindowFlags.AlwaysAutoResize) {

	public override void Draw() {
		ImGui.Text("在下方文本框中自定义信息栏的显示格式。\n" +
		           "在需要插入下列内容的位置使用占位符 {0} {1} {2} {3} {4}：\n\n" +
		           "{0} - 大区（示例：天极北境）\n" +
		           "{1} - 领地（示例：旧萨雷安）\n" +
		           "{2} - 区域（示例：贤者工坊）\n" +
		           "{3} - 子区域（示例：旧萨雷安以太之环）\n" +
		           "{4} - 房区信息（示例：第 14 区）");

		ImGuiHelpers.ScaledDummy(10.0f);

		FormatInputBox("信息栏显示格式", ref config.FormatString);
        DrawBracesMismatchedCheck(config.FormatString);
        
        FormatInputBox("信息栏提示文本", ref config.TooltipFormatString);
        DrawBracesMismatchedCheck(config.TooltipFormatString);

		ImGuiHelpers.ScaledDummy(10.0f);

		if (ImGui.Checkbox("显示分线编号", ref config.ShowInstanceNumber)) {
            config.Save();
		}
		ImGuiComponents.HelpMarker("在字符串末尾附加当前所在分线编号（例如 #2）");

		if (ImGui.Checkbox("显示精确房区位置", ref config.UsePreciseHousingLocation)) {
            config.Save();
		}
		ImGuiComponents.HelpMarker("将“第 14 区”替换为“第 14 区 分区 23 号地”等完整房区信息");
	}

	private void FormatInputBox(string label, ref string setting) {
        using var table = ImRaii.Table("FormatEntryTable", 3, ImGuiTableFlags.NoClip);
        if (!table) return;
    
		ImGui.TableSetupColumn("##Label", ImGuiTableColumnFlags.WidthStretch);
		ImGui.TableSetupColumn("##Input", ImGuiTableColumnFlags.WidthFixed, 300.0f * ImGuiHelpers.GlobalScale);
		ImGui.TableSetupColumn("##ResetButton", ImGuiTableColumnFlags.WidthStretch);

		ImGui.TableNextColumn();
		ImGui.Text(label);

		ImGui.TableNextColumn();
		ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
		if (ImGui.InputText($"##InputString{label}", ref setting, 2047)) {
            onConfigChanged();
		}

		if (ImGui.IsItemDeactivatedAfterEdit() && !BracesMismatched(setting)) {
			config.Save();
		}

		ImGui.TableNextColumn();
		if (ImGui.Button($"恢复默认##{label}")) {
			setting = "{0}, {1}, {2}, {3}";
            onConfigChanged();
			config.Save();
		}
	}

    private void DrawBracesMismatchedCheck(string setting) {
        if (BracesMismatched(setting)) {
            ImGui.TextColored(new Vector4(1.0f, 0.2f, 0.2f, 1.0f), "格式字符串存在错误，请确认每个左花括号 { 都有对应的右花括号 }");
        }  
    }
    
	private static bool BracesMismatched(string formatString)
		=> formatString.Count(c => c == '{') != formatString.Count(c => c == '}');

    public override void OnClose()
        => config.Save();
}
