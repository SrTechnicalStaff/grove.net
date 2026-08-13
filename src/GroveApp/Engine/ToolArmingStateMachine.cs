using System;
using Avalonia;

namespace GroveApp.Engine
{
    /// <summary>
    /// The small interface between keyboard intent and a cell-aligned placement preview.
    /// The canvas supplies collision validation; this module owns only the arming state.
    /// </summary>
    public enum ToolArmingState : byte
    {
        Idle = 0,
        ArmedNote = 1,
        ArmedQuickNote = 2,
        ArmedDocument = 3,
        Placed = 4
    }

    public enum ArmableContentType : byte
    {
        Note = 0,
        QuickNote = 1,
        Document = 2
    }

    public readonly record struct GhostPlacementDescriptor(
        ToolArmingState ArmingState,
        ArmableContentType ContentType,
        CursorDescriptor Cursor,
        int LayerId,
        bool IsValidRegion)
    {
        public CellCoordinate PlacementOriginCell => Cursor.PlacementOriginCell;
        public int WidthCells => Cursor.WidthCells;
        public int HeightCells => Cursor.HeightCells;
        public bool IsVisible => ArmingState is ToolArmingState.ArmedNote
            or ToolArmingState.ArmedQuickNote
            or ToolArmingState.ArmedDocument;
    }

    public readonly record struct PlacementCommitResult(
        bool Succeeded,
        Guid? MemoryId,
        CursorPlacementFootprint Footprint,
        bool ShouldFocusEditor,
        string? RefusalReason);

    public interface IToolArmingService
    {
        ToolArmingState CurrentState { get; }
        GhostPlacementDescriptor ActiveGhostDescriptor { get; }
        bool IsArmed { get; }
        event Action<ToolArmingState>? ArmingStateChanged;
        event Action<GhostPlacementDescriptor>? GhostPreviewUpdated;
        event Action? PlacementRejected;
        void ArmTool(ArmableContentType contentType, CursorDescriptor cursor, int layerId);
        void Disarm();
        void UpdateCursorPosition(CursorDescriptor cursor, int layerId);
        bool TryCommit(out GhostPlacementDescriptor descriptor);
        void CompletePlacement();
    }

    /// <summary>
    /// Deep state machine for ADR-057. It never creates content and never opens a modal;
    /// callers commit the descriptor through their own placement adapter.
    /// </summary>
    public sealed class ToolArmingStateMachine : IToolArmingService
    {
        private readonly Func<CellCoordinate, int, int, int, bool> _isRegionFree;
        private GhostPlacementDescriptor _activeGhost;

        public ToolArmingState CurrentState => _activeGhost.ArmingState;
        public GhostPlacementDescriptor ActiveGhostDescriptor => _activeGhost;
        public bool IsArmed => CurrentState is ToolArmingState.ArmedNote
            or ToolArmingState.ArmedQuickNote
            or ToolArmingState.ArmedDocument;

        public event Action<ToolArmingState>? ArmingStateChanged;
        public event Action<GhostPlacementDescriptor>? GhostPreviewUpdated;
        public event Action? PlacementRejected;

        public ToolArmingStateMachine(Func<CellCoordinate, int, int, int, bool> isRegionFree)
        {
            _isRegionFree = isRegionFree ?? throw new ArgumentNullException(nameof(isRegionFree));
            _activeGhost = new GhostPlacementDescriptor(
                ToolArmingState.Idle,
                ArmableContentType.Note,
                default,
                0,
                false);
        }

        public void ArmTool(ArmableContentType contentType, CursorDescriptor cursor, int layerId)
        {
            var state = contentType switch
            {
                ArmableContentType.Note => ToolArmingState.ArmedNote,
                ArmableContentType.QuickNote => ToolArmingState.ArmedQuickNote,
                ArmableContentType.Document => ToolArmingState.ArmedDocument,
                _ => throw new ArgumentOutOfRangeException(nameof(contentType))
            };

            if (cursor.Kind != CursorFootprintKind.ArmedTool)
            {
                throw new ArgumentException("An armed tool must be represented by an ArmedTool cursor descriptor.", nameof(cursor));
            }

            _activeGhost = new GhostPlacementDescriptor(
                state,
                contentType,
                cursor,
                layerId,
                _isRegionFree(cursor.PlacementOriginCell, cursor.WidthCells, cursor.HeightCells, layerId));

            ArmingStateChanged?.Invoke(state);
            GhostPreviewUpdated?.Invoke(_activeGhost);
        }

        public void Disarm()
        {
            if (CurrentState == ToolArmingState.Idle)
            {
                return;
            }

            _activeGhost = _activeGhost with
            {
                ArmingState = ToolArmingState.Idle,
                Cursor = default,
                IsValidRegion = false
            };
            ArmingStateChanged?.Invoke(ToolArmingState.Idle);
            GhostPreviewUpdated?.Invoke(_activeGhost);
        }

        public void UpdateCursorPosition(CursorDescriptor cursor, int layerId)
        {
            if (!IsArmed)
            {
                return;
            }

            if (cursor.Kind != CursorFootprintKind.ArmedTool)
            {
                throw new ArgumentException("An armed tool must be represented by an ArmedTool cursor descriptor.", nameof(cursor));
            }

            bool isValid = _isRegionFree(
                cursor.PlacementOriginCell,
                cursor.WidthCells,
                cursor.HeightCells,
                layerId);

            var updated = _activeGhost with
            {
                Cursor = cursor,
                LayerId = layerId,
                IsValidRegion = isValid
            };

            if (updated == _activeGhost)
            {
                return;
            }

            _activeGhost = updated;
            GhostPreviewUpdated?.Invoke(_activeGhost);
        }

        public bool TryCommit(out GhostPlacementDescriptor descriptor)
        {
            descriptor = _activeGhost;
            if (!IsArmed || !descriptor.IsValidRegion)
            {
                PlacementRejected?.Invoke();
                return false;
            }

            _activeGhost = descriptor with { ArmingState = ToolArmingState.Placed };
            ArmingStateChanged?.Invoke(ToolArmingState.Placed);
            GhostPreviewUpdated?.Invoke(_activeGhost);
            descriptor = _activeGhost;
            return true;
        }

        public void CompletePlacement()
        {
            if (CurrentState == ToolArmingState.Placed)
            {
                Disarm();
            }
        }
    }
}
