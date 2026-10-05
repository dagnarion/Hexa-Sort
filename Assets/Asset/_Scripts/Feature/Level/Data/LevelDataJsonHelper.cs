using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class LevelJsonDto
{
    public int levelNumber;
    public string levelName;
    public int gridSizeX;
    public int gridSizeY;
    public string formLayout;
    public List<SlotJsonDto> slots = new List<SlotJsonDto>();
}

[Serializable]
public class SlotJsonDto
{
    public int x;
    public int y;
    public bool isActive;
    public string slotType;
    public string lockType;
    public int breakHitCount;
    public int taskTargetScore;
    public bool hasStack;
    public List<string> stackColors = new List<string>();
}

public static class LevelDataJsonHelper
{
    public static string ColorToHex(Color c)
    {
        return $"#{ColorUtility.ToHtmlStringRGBA(c)}";
    }

    public static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color c))
        {
            return c;
        }

        return Color.white;
    }

    public static LevelJsonDto ToDto(LevelDataSO so)
    {
        if (so == null) return null;

        var dto = new LevelJsonDto
        {
            levelNumber = so.levelNumber,
            levelName = so.levelName,
            gridSizeX = so.gridSize.x,
            gridSizeY = so.gridSize.y,
            slots = new List<SlotJsonDto>()
        };

        foreach (var slot in so.slots)
        {
            if (slot == null) continue;

            var slotDto = new SlotJsonDto
            {
                x = slot.gridPosition.x,
                y = slot.gridPosition.y,
                isActive = slot.isActive,
                slotType = slot.slotType.ToString(),
                lockType = slot.lockType.ToString(),
                breakHitCount = slot.breakHitCount,
                taskTargetScore = slot.taskTargetScore,
                hasStack = slot.hasStack,
                stackColors = new List<string>()
            };

            if (slot.hasStack && slot.stackColors != null)
            {
                foreach (var color in slot.stackColors)
                {
                    slotDto.stackColors.Add(ColorToHex(color));
                }
            }

            dto.slots.Add(slotDto);
        }

        return dto;
    }

    public static void FromDto(LevelJsonDto dto, LevelDataSO targetSO)
    {
        if (dto == null || targetSO == null) return;

        targetSO.levelNumber = dto.levelNumber;
        targetSO.levelName = dto.levelName;
        targetSO.gridSize = new Vector2Int(dto.gridSizeX, dto.gridSizeY);
        

        targetSO.slots = new List<SlotLevelData>();

        if (dto.slots != null)
        {
            foreach (var sDto in dto.slots)
            {
                var slot = new SlotLevelData(new Vector2Int(sDto.x, sDto.y), sDto.isActive);

                if (Enum.TryParse<SlotType>(sDto.slotType, out var st))
                    slot.slotType = st;
                else
                    slot.slotType = SlotType.Nozmal;

                if (Enum.TryParse<LockType>(sDto.lockType, out var lt))
                    slot.lockType = lt;
                else
                    slot.lockType = LockType.None;

                slot.breakHitCount = sDto.breakHitCount;
                slot.taskTargetScore = sDto.taskTargetScore;
                slot.hasStack = sDto.hasStack;
                slot.stackColors = new List<Color>();

                if (sDto.stackColors != null && sDto.stackColors.Count > 0)
                {
                    foreach (var hex in sDto.stackColors)
                    {
                        slot.stackColors.Add(HexToColor(hex));
                    }
                }

                targetSO.slots.Add(slot);
            }
        }
    }

    public static string ToJson(LevelDataSO so, bool prettyPrint = true)
    {
        var dto = ToDto(so);
        return JsonUtility.ToJson(dto, prettyPrint);
    }

    public static void FromJson(string json, LevelDataSO targetSO)
    {
        var dto = JsonUtility.FromJson<LevelJsonDto>(json);
        FromDto(dto, targetSO);
    }

    public static void SaveToFile(string filePath, LevelDataSO so)
    {
        string json = ToJson(so, true);
        string dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(filePath, json);
    }

    public static bool LoadFromFile(string filePath, LevelDataSO targetSO)
    {
        if (!File.Exists(filePath)) return false;
        string json = File.ReadAllText(filePath);
        FromJson(json, targetSO);
        return true;
    }
}