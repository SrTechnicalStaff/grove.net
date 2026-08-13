# Grove Domain Context

This file is the canonical vocabulary for Grove's product domain. Domain
relationships and user-visible behavior are defined in the individual concept
documents indexed by [`docs/domain/README.md`](docs/domain/README.md);
implementation tasks, UI documents, review ledgers, and generated plans may
refer to those documents but may not redefine them.

## Authority

- `CONTEXT.md` owns the short form of the ubiquitous language.
- Individual documents under `docs/domain/` own each product concept's rules,
  boundaries, and user journeys.
- Accepted domain decisions under `docs/specs/` may refine the model, but may
  not contradict it. A contradiction requires a new accepted decision.
- Product explainers, UI specifications, tickets, reviews, and code comments
  are derived material. They are not authority for domain meaning.

## Terms

### Semantic records and spatial instances

**Memory**:
An immutable semantic record with a stable identity, payload, and version
lineage. A Memory has no spatial state and does not own Content, Placement, or
Anchor records.

_Avoid_: placed Memory, unplaced Memory, Memory placement, Memory anchor

**Content**:
A placed spatial instance of exactly one Memory. Content owns its grid
footprint and spatial state and references its Memory through `MemoryId`.

_Avoid_: placed Memory, Memory instance when referring to the Grid object

**MemoryId**:
The foreign-key identity on Content that points to its Memory. It is the only
semantic relationship required for Content to refer to a Memory.

**Memory Version**:
An immutable Memory record in a lineage. Semantic editing creates a new
version and rebinds only the edited Content; it never mutates the prior record.

**Placement**:
The grid position, footprint, and Grid Layer state of Content. Placement is
spatial state; it is never a state of Memory.

**Anchor**:
An optional authored label or context record attached to Content. An Anchor
describes why or where that Content is meaningful; it is not owned by Memory
and is not part of the Memory payload.

_Avoid_: Memory anchor, anchor list on Memory

### Spatial recall

**Field Ledger**:
The spatial evidence system that evaluates Content and cells. It is a bridge
from spatial proximity to semantic recall; it does not change Memory identity
or payload.

**Aura**:
The field emitted by Content into nearby cells. Aura overlap and saturation
provide strong spatial-recall evidence.

**Gap**:
A cell without overlapping Content aura that still lies between or near
Content. Gaps contribute a weaker, distance-decaying spatial-recall signal.

**Spatial Recall Weight**:
A derived ranking signal calculated from Content and Field Ledger evidence.
It belongs to a search result or relationship, never to Memory itself.

**Cell**:
An addressable Grid coordinate unit and the membrane between spatial state and
derived recall evidence. Cells never own Memories or Content.

### Grid composition

**Grid Layer**:
A literal depth plane in the spatial Grid. The term Layer is reserved for
this concept.

**Grid**:
The unbounded spatial coordinate field that hosts Content through Grid Layers.
It is not the Memory store.

**Plane**:
A visual composition surface: Spatial Grid Plane, Information Plane, or HUD
Plane.

**Tier**:
A subdivision within a Plane or a representation level for Grid rendering.

**Slate**:
One of the three named surfaces: Writing Slate, Memory Slate, or Gallery
Slate.

**Overlay**:
A transient interface surface that is not a Slate.

## Invariants

- A Memory never references Content, Placement, or Anchor records.
- Every Content instance references exactly one Memory through `MemoryId`.
- One Memory may be referenced by zero, one, or many Content instances.
- Content may have zero or one Anchor; an Anchor is attached to Content.
- Creating a Memory does not create Content.
- Placing a Memory creates Content with a `MemoryId`; it does not mutate the
  Memory into a placed state.
- Moving, resizing, relabelling, or deleting Content changes only Content,
  Placement, and Anchor state. It never changes the Memory payload or identity.
- Editing Content creates a new immutable Memory version and moves that
  Content's `MemoryId` to the new record. Other Content remains bound to its
  prior Memory version.
- Quick Note creates a Memory directly and does not require Content.
- Memory search returns Memory records. It joins outward to Content and
  Anchors for matching context and spatial-recall ranking.
- Search matching precedence is Anchor labels, then Memory titles, then
  Memory payload content. Matching is fuzzy and may match any of those levels.
- Field Ledger evidence can reorder Memory results but cannot create, delete,
  or mutate Memory records.
