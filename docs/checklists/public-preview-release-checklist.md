# GlucoDesk v0.1.0-preview release checklist

This checklist is used before publishing a preview build.

## Release type

This is a public preview release intended for technical validation and early feedback.

GlucoDesk is not a medical device and must not be used for treatment decisions.

## Automated checks

Run:

- dotnet clean
- dotnet restore
- dotnet build -c Release
- dotnet test -c Release

Expected result:

- 0 failed tests

## Manual app checks

Run:

dotnet run --project src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj

Check:

- App starts successfully.
- Dashboard opens correctly.
- Sidebar remains usable at smaller window heights.
- Account page loads saved credentials without showing the password.
- Account page can test Dexcom Share connection.
- Saving the account selects Dexcom Share as live and historical provider.
- Closing and reopening the app reconnects without re-entering credentials.
- Background sync status is visible.
- History continuity status is visible.
- Manual history sync button is visible and clickable.
- Diary page opens.
- Excel diary export works.
- PDF diary export works.
- Settings page opens.
- No credentials are written to Git-tracked files.
- git status -sb does not show generated artifacts.

## Package checks

Create the macOS Apple Silicon preview package:

./scripts/package-preview.sh osx-arm64

Optional Intel macOS package:

./scripts/package-preview.sh osx-x64

Expected output:

artifacts/releases/GlucoDesk-0.1.0-preview-osx-arm64.zip

Check:

- GlucoDesk.app is created.
- Finder shows the app as GlucoDesk.
- The app uses the GlucoDesk icon.
- The app opens locally.
- Dashboard loads correctly from the packaged app.
- Account page still accesses secure credential storage.
- No generated artifact is tracked by Git.

## GitHub release

Suggested tag:

v0.1.0-preview

Suggested release title:

GlucoDesk v0.1.0-preview

Suggested release notes:

Initial public preview of GlucoDesk.

Highlights:
- Desktop dashboard for glucose awareness.
- Dexcom Share account setup with secure local credential storage.
- Automatic reconnect preparation after account save.
- Dexcom Share account connection diagnostics.
- Background sync status.
- History continuity status and manual sync action.
- Local-first glucose history.
- Glycemic diary export to Excel and PDF.

Safety:
GlucoDesk is not a medical device and must not be used for treatment decisions.
Always use official Dexcom and Omnipod apps for therapy decisions.

Known limitations:
- Preview build is intended for technical validation.
- macOS package is not signed or notarized yet.
- The app may require right click > Open on first launch.
- UI and packaging are still evolving.
