# GlucoDesk release smoke test checklist

This checklist must be completed before publishing a public GlucoDesk preview release.

GlucoDesk is not a medical device and must not be used for insulin dosing, treatment, diagnosis, emergency, or safety-critical decisions.

## Release metadata

- [ ] Confirm release version.
- [ ] Confirm Git tag name.
- [ ] Confirm release title.
- [ ] Confirm release notes are generated.
- [ ] Confirm release artifacts are generated from the latest `main`.
- [ ] Confirm no local-only artifact is accidentally used instead of a CI/release artifact.

Suggested format:

```text
Version: 0.2.1-preview
Git tag: v0.2.1-preview
Release title: GlucoDesk 0.2.1 Preview
```

## Git and repository state

- [ ] `main` is up to date with `origin/main`.
- [ ] Working tree is clean.
- [ ] Release branch, if any, is merged.
- [ ] No generated artifacts are staged for commit.
- [ ] No secrets, tokens, credentials, or local configuration files are staged.

Commands:

```bash
git checkout main
git pull origin main
git status -sb
```

## Build and test

Run locally before creating the release:

```bash
dotnet build -c Release
dotnet test -c Release
```

Expected result:

- [ ] Build succeeds.
- [ ] All tests pass.
- [ ] No unexpected warnings or runtime errors are introduced.

## GitHub Actions artifact workflow

Run the manual workflow from GitHub:

```text
Actions -> Preview release artifacts -> Run workflow
```

Use the intended release version, for example:

```text
0.2.1-preview
```

Verify:

- [ ] macOS job succeeds.
- [ ] Windows job succeeds.
- [ ] macOS artifacts are uploaded.
- [ ] Windows artifacts are uploaded.
- [ ] No CI-only failure appears on GitHub runners.
- [ ] Generated artifacts are downloaded from GitHub Actions for final testing.

## macOS artifacts

Expected macOS artifacts:

- [ ] `.dmg`
- [ ] `.zip`
- [ ] SHA256 checksums
- [ ] manifest, if generated
- [ ] release notes, if generated

Verify macOS artifacts:

```bash
./scripts/verify-macos-preview-artifacts.sh 0.2.1-preview
```

Manual macOS smoke test:

- [ ] DMG opens correctly.
- [ ] DMG contains `GlucoDesk.app`.
- [ ] DMG contains `Applications` symlink.
- [ ] DMG contains `README.txt`.
- [ ] DMG contains `SAFETY-NOTICE.txt`.
- [ ] Drag-and-drop installation to `Applications` works.
- [ ] App launches from `Applications`.
- [ ] Dock icon is correct and has no white border.
- [ ] Menu bar icon is visible and visually correct.
- [ ] Desktop presence popover opens correctly.
- [ ] Dashboard loads has no white border.
- [ ] Menu bar icon is visible and visually correct.
- [ ] Desktop presence popover opens correctly.
- [ ] Dashboard loads.
- [ ] Local history card is visible.
- [ ] Safety notice remains visible in the UI.
- [ ] App can be closed and reopened.

Preview-specific expected behavior:

- [ ] Gatekeeper/notarization warning is documented if the build is not notarized.
- [ ] The app does not claim to be a medical device.
- [ ] No urgent or treatment-related claim appears in UI or release notes.

## Windows artifacts

Expected Windows artifacts:

- [ ] Setup installer `.exe`
- [ ] Portable `.zip`
- [ ] SHA256 checksums
- [ ] manifest, if generated
- [ ] release notes

Verify Windows artifacts:

```powershell
.\scripts\verify-windows-preview-artifacts.ps1 -Version "0.2.1-preview"
```

Manual Windows setup smoke test:

- [ ] Previous GlucoDesk installation is removed, if present.
- [ ] Setup installer launches.
- [ ] MIT license page is shown.
- [ ] Safety notice page is shown.
- [ ] App installs without administrator privileges.
- [ ] Start Menu shortcut is created.
- [ ] Optional desktop shortcut works, if selected.
- [ ] App launches after installation.
- [ ] Windows taskbar icon is correct and has no white border.
- [ ] Windows tray icon uses the small gray menu-bar-style icon.
- [ ] Tray popup opens fully inside the screen.
- [ ] Tray popup is not clipped on the right edge.
- [ ] Dashboard loads.
- [ ] Local history card is visible.
- [ ] Safety notice remains visible in the UI.
- [ ] Uninstall works from Windows settings or uninstall entry.

Manual Windows portable ZIP smoke test:

- [ ] ZIP extracts correctly.
- [ ] `GlucoDesk.Desktop.exe` launches.
- [ ] Windows taskbar icon is correct.
- [ ] Windows tray icon is correct.
- [ ] Tray popup opens correctly.
- [ ] No installer-specific assumption breaks portable mode.

Preview-specific expected behavior:

- [ ] Windows SmartScreen warning is documented if the installer is not code-signed.
- [ ] The app does not claim to be a medical device.
- [ ] No urgent or treatment-related claim appears in UI or release notes.

## Checksums

macOS:

```bash
shasum -a 256 -c GlucoDesk-0.2.1-preview-osx-arm64-checksums.sha256
```

Windows:

```powershell
Get-FileHash .\GlucoDesk-0.2.1-preview-win-x64-setup.exe -Algorithm SHA256
Get-FileHash .\GlucoDesk-0.2.1-preview-win-x64-portable.zip -Algorithm SHA256
```

Verify:

- [ ] macOS checksum file matches downloaded macOS artifacts.
- [ ] Windows checksum file matches downloaded Windows artifacts.
- [ ] Checksums are generated after final packaging.
- [ ] Checksums in release notes match the attached artifacts.

## Privacy and safety review

Before release:

- [ ] No Dexcom/Nightscout credentials are logged.
- [ ] No tokens are written to plain text files.
- [ ] No local user data is included in artifacts.
- [ ] No test glucose data is accidentally bundled unless it is intentional mock/demo data.
- [ ] README clearly says GlucoDesk is not a medical device.
- [ ] Release notes include a safety notice.
- [ ] Installer includes a safety notice.
- [ ] DMG includes `SAFETY-NOTICE.txt`.

## Release notes review

Release notes should mention:

- [ ] Current version.
- [ ] Supported platforms.
- [ ] macOS DMG and ZIP assets.
- [ ] Windows setup installer and portable ZIP assets.
- [ ] SHA256 verification.
- [ ] Preview limitations.
- [ ] Not a medical device safety notice.
- [ ] SmartScreen warning for unsigned Windows builds.
- [ ] Gatekeeper/notarization warning for non-notarized macOS builds.
- [ ] Main improvements since previous preview.

Suggested release summary items:

- Windows setup installer.
- Windows portable ZIP.
- Windows taskbar icon improvements.
- Windows tray icon styling.
- Windows tray popup positioning fix.
- macOS DMG drag-and-drop layout.
- macOS Dock icon cleanup.
- macOS optional notarization support.
- Release artifact checksums.
- Preview installation instructions.

## GitHub Release draft

Before publishing:

- [ ] Create GitHub Release as draft.
- [ ] Attach all macOS artifacts.
- [ ] Attach all Windows artifacts.
- [ ] Attach checksum files.
- [ ] Paste reviewed release notes.
- [ ] Confirm download links work.
- [ ] Confirm attached artifact names match release notes.
- [ ] Confirm release is marked as pre-release.
- [ ] Do not publish until both macOS and Windows smoke tests are complete.

## Final publish checklist

Only publish when all are true:

- [ ] GitHub Actions artifact workflow passed.
- [ ] macOS smoke test passed.
- [ ] Windows installer smoke test passed.
- [ ] Windows portable ZIP smoke test passed.
- [ ] Checksums verified.
- [ ] Release notes reviewed.
- [ ] Safety notice reviewed.
- [ ] Git tag created and pushed.
- [ ] GitHub Release draft reviewed.
- [ ] Release marked as pre-release.

Tag command:

```bash
git tag v0.2.1-preview
git push origin v0.2.1-preview
```

## Post-release checks

After publishing:

- [ ] Open GitHub Release page in a clean browser session.
- [ ] Download macOS artifact.
- [ ] Download Windows artifact.
- [ ] Confirm artifacts are not empty.
- [ ] Confirm README installation instructions are still accurate.
- [ ] Confirm release link can be shared publicly.
- [ ] Watch for user feedback and crash reports.
