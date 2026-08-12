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
        CellCoordinate OriginCell,
        int WidthCells,
        int HeightCells,
        int LayerId,
        bool IsValidRegion)
    {
        public bool IsVisible => ArmingState != ToolArmingState.Idle;
    }

    /// <summary>
    /// Deep state machine for ADR-057. It never creates content and never opens a modal;
    /// callers commit the descriptor through their own placement adapter.
    /// </summary>
    public sealed class ToolArmingStateMachine
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
                new CellCoordinate(0, 0),
                1,
                1,
                0,
                false);
        }

        public void ArmTool(ArmableContentType contentType, CellCoordinate origin, int layerId)
        {
            var state = contentType switch
            {
                ArmableContentType.Note => ToolArmingState.ArmedNote,
                ArmableContentType.QuickNote => ToolArmingState.ArmedQuickNote,
                ArmableContentType.Document => ToolArmingState.ArmedDocument,
                _ => throw new ArgumentOutOfRangeException(nameof(contentType))
            };

            (int width, int height) = contentType == ArmableContentType.Document ? (2, 2) : (1, 1);
            _activeGhost = new GhostPlacementDescriptor(
                state,
                contentType,
                origin,
                width,
                height,
                layerId,
                _isRegionFree(origin, width, height, layerId));

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
                IsValidRegion = false
            };
            ArmingStateChanged?.Invoke(ToolArmingState.Idle);
            GhostPreviewUpdated?.Invoke(_activeGhost);
        }

        public void UpdateCursorPosition(CellCoordinate origin, int layerId)
        {
            if (!IsArmed)
            {
                return;
            }

            bool isValid = _isRegionFree(
                origin,
                _activeGhost.WidthCells,
                _activeGhost.HeightCells,
                layerId);

            var updated = _activeGhost with
            {
                OriginCell = origin,
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
