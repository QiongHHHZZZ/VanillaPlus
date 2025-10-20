using System;
using System.Collections.Generic;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.LocationDisplay;

public unsafe class LocationDisplay : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "位置栏信息扩展",
        Description = "在服务器信息栏显示当前所在位置。",
        Authors = [ "MidoriKami" ],
        Type = ModificationType.UserInterface,
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
            new ChangeLogInfo(2, "修复实例编号未及时更新的问题"),
        ],
        CompatibilityModule = new PluginCompatibilityModule("WhereAmIAgain"),
    };

    private IDtrBarEntry? dtrBarEntry;
    private PlaceName? currentContinent;
    private PlaceName? currentRegion;
    private PlaceName? currentSubArea;
    private PlaceName? currentTerritory;
    private string? currentWard;
    private byte lastHousingDivision;
    private sbyte lastHousingPlot;
    private short lastHousingRoom;
    private sbyte lastHousingWard;
    private uint lastInstanceId;
    private uint lastRegion;
    private uint lastSubArea;
    private uint lastTerritory;
    private bool locationChanged;

    private static TerritoryInfo* AreaInfo => TerritoryInfo.Instance();
    private static HousingManager* HousingInfo => HousingManager.Instance();

    private LocationDisplayConfig? config;
    private LocationDisplayConfigWindow? configWindow;

    public override string ImageName => "LocationDisplay.png";

    public override void OnEnable() {
        config = LocationDisplayConfig.Load();
        configWindow = new LocationDisplayConfigWindow(config, UpdateDtrText);
        configWindow.AddToWindowSystem();
        OpenConfigAction = configWindow.Toggle;
        
        dtrBarEntry = Services.DtrBar.Get("VanillaPlus - 位置栏信息扩展");
        dtrBarEntry.OnClick = _ => configWindow.Toggle();

        locationChanged = true;
        
        Services.Framework.Update += OnFrameworkUpdate;
		Services.ClientState.TerritoryChanged += OnZoneChange;
    }

    public override void OnDisable() {
        configWindow?.RemoveFromWindowSystem();
        configWindow = null;
        
        Services.Framework.Update -= OnFrameworkUpdate;
		Services.ClientState.TerritoryChanged -= OnZoneChange;
        
        dtrBarEntry?.Remove();
        dtrBarEntry = null;
        
        config = null;
    }

    private void OnZoneChange(ushort obj)		
        => locationChanged = true;

    private void OnFrameworkUpdate(IFramework framework) {
        if (config is null) return;
		if (Services.ClientState.LocalPlayer is null) return;
        
        UpdateRegion();
        UpdateSubArea();
        UpdateTerritory();
        UpdateInstanceId();

        if (config.UsePreciseHousingLocation) {
            UpdatePreciseHousing();
        }
        else {
            UpdateHousing();
        }

        if (locationChanged) {
            UpdateDtrText();
        }
    }

    private void UpdateDtrText() {
        if (config is null || dtrBarEntry is null) return;
		
        var dtrString = FormatString(config.FormatString);
		var tooltipString = FormatString(config.TooltipFormatString);

        dtrBarEntry.Text = dtrString;
        dtrBarEntry.Tooltip = tooltipString.Replace(@"\n", "\n");
		locationChanged = false;
	}

	private string GetStringForIndex(int index) => index switch {
		0 => currentContinent?.Name.ToString() ?? string.Empty,
		1 => currentTerritory?.Name.ToString() ?? string.Empty,
		2 => currentRegion?.Name.ToString() ?? string.Empty,
		3 => currentSubArea?.Name.ToString() ?? string.Empty,
		4 => currentWard ?? string.Empty,
		_ => string.Empty,
	};

	private string FormatString(string inputFormat) {
        if (config is null) return string.Empty;
		
        try {
			var preTextEnd = inputFormat.IndexOf('{');
			var postTextStart = inputFormat.LastIndexOf('}') + 1;
			var workingSegment = inputFormat[preTextEnd..postTextStart];

			// Get all the segments and the text before them
			// If the segment itself resolves to an empty modifier, we omit the preceding text.
			var splits = workingSegment.Split('}');
			var internalString = string.Empty;
			foreach (var segment in splits) {
				if (segment.IsNullOrEmpty()) continue;

				var separator = segment[..^2];
				var location = GetStringForIndex(int.Parse(segment[^1..]));

				if (location.IsNullOrEmpty()) continue;
				internalString += internalString == string.Empty ? $"{location}" : $"{separator}{location}";
			}

			if (config.ShowInstanceNumber) {
				internalString += GetCharacterForInstanceNumber(UIState.Instance()->PublicInstance.InstanceId);
			}

			return inputFormat[..preTextEnd] + internalString + inputFormat[postTextStart..];
		}
		catch (Exception) {
			// If the format is empty, it'll throw an exception, but some people might still want instance numbers.
			if (config.ShowInstanceNumber) {
				return GetCharacterForInstanceNumber(UIState.Instance()->PublicInstance.InstanceId);
			}

			// Ignore all other exceptions and return empty.
		}

		return string.Empty;
	}

	private static string GetCharacterForInstanceNumber(uint instance) {
		if (instance == 0) return string.Empty;

		return $" {((SeIconChar) ((int) SeIconChar.Instance1 + (instance - 1))).ToIconChar()}";
	}

	private void UpdateTerritory() {
		if (lastTerritory != Services.ClientState.TerritoryType) {
			lastTerritory = Services.ClientState.TerritoryType;
			var territory = GetCurrentTerritory();

			currentTerritory = territory.PlaceName.Value;
			currentContinent = territory.PlaceNameRegion.Value;
			locationChanged = true;
		}
	}
    
    private void UpdateInstanceId() {
        if (lastInstanceId != UIState.Instance()->PublicInstance.InstanceId) {
            lastInstanceId = UIState.Instance()->PublicInstance.InstanceId;
            
            locationChanged = true;
        }
    }

	private void UpdateSubArea() {
		if (lastSubArea != AreaInfo->SubAreaPlaceNameId) {
			lastSubArea = AreaInfo->SubAreaPlaceNameId;
			currentSubArea = GetPlaceName(AreaInfo->SubAreaPlaceNameId);
			locationChanged = true;
		}
	}

	private void UpdateRegion() {
		if (lastRegion != AreaInfo->AreaPlaceNameId) {
			lastRegion = AreaInfo->AreaPlaceNameId;
			currentRegion = GetPlaceName(AreaInfo->AreaPlaceNameId);
			locationChanged = true;
		}
	}

	private void UpdateHousing() {
		if (HousingInfo is null || HousingInfo->CurrentTerritory is null) {
			currentWard = null;
			return;
		}

		var ward = (sbyte) (HousingInfo->GetCurrentWard() + 1);

		if (lastHousingWard != ward) {
			lastHousingWard = ward;
			currentWard = $"第 {ward} 区";
			locationChanged = true;
		}
	}

	private void UpdatePreciseHousing() {
		if (HousingInfo is null) {
			currentWard = null;
			return;
		}

		var ward = HousingInfo->GetCurrentWard();
		var room = HousingInfo->GetCurrentRoom();
		var plot = HousingInfo->GetCurrentPlot();
		var division = HousingInfo->GetCurrentDivision();

		if (ward != lastHousingWard || room != lastHousingRoom || plot != lastHousingPlot || division != lastHousingDivision) {
			lastHousingWard = ward;
			lastHousingRoom = room;
			lastHousingPlot = plot;
			lastHousingDivision = division;
			currentWard = GetCurrentHouseAddress();
			locationChanged = true;
		}
	}

	private string GetCurrentHouseAddress() {
		var housingManager = HousingManager.Instance();
		if (housingManager == null) return string.Empty;
		var strings = new List<string>();

		var ward = housingManager->GetCurrentWard() + 1;
		if (ward == 0) return string.Empty;

		var plot = housingManager->GetCurrentPlot();
		var room = housingManager->GetCurrentRoom();
		var division = housingManager->GetCurrentDivision();

		strings.Add($"第 {ward} 区");
		if (division == 2 || plot is >= 30 or -127) strings.Add("分区");

		switch (plot) {
			case < -1:
				strings.Add(room == 0 ? "公寓大厅" : $"公寓 {room}");
				break;

			case > -1:
				strings.Add($"第 {plot + 1} 号地");
				if (room > 0) {
					strings.Add($"第 {room} 号房");
				}
				break;
		}

		return string.Join(" ", strings);
	}

	private static PlaceName GetPlaceName(uint row)
		=> Services.DataManager.GetExcelSheet<PlaceName>().GetRow(row);

	private static TerritoryType GetCurrentTerritory() {
		if (HousingInfo is not null && HousingInfo->IsInside()) {
			return Services.DataManager.GetExcelSheet<TerritoryType>().GetRow(HousingManager.GetOriginalHouseTerritoryTypeId());
		}
		else {
			return Services.DataManager.GetExcelSheet<TerritoryType>().GetRow(Services.ClientState.TerritoryType);
		}
	}
}


