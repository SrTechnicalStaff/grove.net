# Verification contract

Grove treats a capability as complete only when the user can exercise it through the canonical published executable at `GroveApp-Release/GroveApp.exe`.

`eng/verify-release.ps1` builds the application, runs its unit and integration tests, publishes the canonical executable, runs Windows UI Automation acceptance tests against that executable, and records its commit and SHA-256 hash in `GroveApp-Release/build-manifest.json`.

Source files, build success, and hidden controls are implementation evidence, not product completion evidence. A capability may be marked `VERIFIED` only when its acceptance journey passes through the published executable. `SUPERSEDED` and `REFUSED` require a link to the binding product or architectural decision. No partial or wired status is permitted.
