using System;
using System.Collections.Generic;
using UnityEngine;

    public static class LevelDataExtensions
    {
        #region LevelDataSO Operations
        public static SlotLevelData GetSlot(this LevelDataSO levelData, Vector2Int pos)
        {
            if (levelData == null) return null;
            if (levelData.slots == null) levelData.slots = new List<SlotLevelData>();
            return levelData.slots.Find(s => s != null && s.gridPosition == pos);
        }

        public static SlotLevelData GetOrCreateSlot(this LevelDataSO levelData, Vector2Int pos, bool activeByDefault = false)
        {
            if (levelData == null) return null;
            if (levelData.slots == null) levelData.slots = new List<SlotLevelData>();

            var slot = levelData.GetSlot(pos);
            if (slot == null)
            {
                slot = new SlotLevelData(pos, activeByDefault);
                levelData.slots.Add(slot);
            }

            return slot;
        }

        public static bool IsSlotActive(this LevelDataSO levelData, Vector2Int pos)
        {
            var slot = levelData.GetSlot(pos);
            return slot != null && slot.isActive;
        }

        public static void SetSlotActive(this LevelDataSO levelData, Vector2Int pos, bool active)
        {
            var slot = levelData.GetOrCreateSlot(pos, active);
            slot.isActive = active;
        }

        public static List<SlotLevelData> GetActiveSlots(this LevelDataSO levelData)
        {
            if (levelData == null || levelData.slots == null) return new List<SlotLevelData>();
            return levelData.slots.FindAll(s => s != null && s.isActive);
        }

        public static void ResizeGrid(this LevelDataSO levelData, Vector2Int newSize)
        {
            if (levelData == null) return;
            levelData.gridSize = new Vector2Int(Mathf.Max(1, newSize.x), Mathf.Max(1, newSize.y));
            levelData.PruneAndSynchronizeSlots();
        }

        public static void ResetSlots(this LevelDataSO levelData)
        {
            if (levelData == null) return;
            if (levelData.slots == null) levelData.slots = new List<SlotLevelData>();
            levelData.slots.Clear();

            for (int y = 0; y < levelData.gridSize.y; y++)
            {
                for (int x = 0; x < levelData.gridSize.x; x++)
                {
                    levelData.slots.Add(new SlotLevelData(new Vector2Int(x, y), false));
                }
            }
        }

        public static void PruneAndSynchronizeSlots(this LevelDataSO levelData)
        {
            if (levelData == null) return;
            if (levelData.slots == null) levelData.slots = new List<SlotLevelData>();

            // Xóa tất cả các slot null hoặc ngoài phạm vi gridSize
            levelData.slots.RemoveAll(s => s == null ||
                                          s.gridPosition.x < 0 || s.gridPosition.x >= levelData.gridSize.x ||
                                          s.gridPosition.y < 0 || s.gridPosition.y >= levelData.gridSize.y);

            // Bổ sung các slot còn thiếu trong phạm vi gridSize
            for (int y = 0; y < levelData.gridSize.y; y++)
            {
                for (int x = 0; x < levelData.gridSize.x; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (levelData.GetSlot(pos) == null)
                    {
                        levelData.slots.Add(new SlotLevelData(pos, false));
                    }
                }
            }

            // Sắp xếp lại danh sách theo (y, x) để hiển thị trong Inspector tuần tự, sạch sẽ
            levelData.slots.Sort((a, b) =>
            {
                int cmpY = a.gridPosition.y.CompareTo(b.gridPosition.y);
                return cmpY != 0 ? cmpY : a.gridPosition.x.CompareTo(b.gridPosition.x);
            });
        }

        public static void EnsureSlotsInitialized(this LevelDataSO levelData)
        {
            levelData.PruneAndSynchronizeSlots();
        }

        public static void RecalculateBounds(this LevelDataSO levelData)
        {
            if (levelData == null) return;
            var activeSlots = levelData.GetActiveSlots();
            if (activeSlots.Count == 0) return;

            int maxX = 0;
            int maxY = 0;
            foreach (var slot in activeSlots)
            {
                if (slot.gridPosition.x > maxX) maxX = slot.gridPosition.x;
                if (slot.gridPosition.y > maxY) maxY = slot.gridPosition.y;
            }

            levelData.gridSize = new Vector2Int(maxX + 1, maxY + 1);
            levelData.PruneAndSynchronizeSlots();
        }
        #endregion

        #region SlotLevelData Operations
        public static SlotLevelData Clone(this SlotLevelData slot)
        {
            if (slot == null) return null;
            var clone = new SlotLevelData(slot.gridPosition, slot.isActive)
            {
                slotType = slot.slotType,
                lockType = slot.lockType,
                breakHitCount = slot.breakHitCount,
                taskTargetScore = slot.taskTargetScore,
                hasStack = slot.hasStack,
                stackColors = new List<Color>(slot.stackColors)
            };
            return clone;
        }

        public static void AddLayer(this SlotLevelData slot, Color color)
        {
            if (slot == null) return;
            slot.hasStack = true;
            if (slot.stackColors == null) slot.stackColors = new List<Color>();
            slot.stackColors.Add(color);
        }

        public static void RemoveLayer(this SlotLevelData slot, int index)
        {
            if (slot == null || slot.stackColors == null) return;
            if (index >= 0 && index < slot.stackColors.Count)
            {
                slot.stackColors.RemoveAt(index);
                if (slot.stackColors.Count == 0)
                {
                    slot.hasStack = false;
                }
            }
        }

        public static void ClearStack(this SlotLevelData slot)
        {
            if (slot == null) return;
            slot.hasStack = false;
            if (slot.stackColors != null)
            {
                slot.stackColors.Clear();
            }
        }
        #endregion
    }