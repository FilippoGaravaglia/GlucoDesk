# macOS notarization

GlucoDesk can optionally notarize the macOS DMG when the required Apple Developer credentials are available.

The default preview packaging flow does not notarize the DMG.

## Default preview flow

```bash
./scripts/package-macos-preview.sh 0.2.1-preview
./scripts/verify-macos-preview-artifacts.sh 0.2.1-preview
```

This creates a local preview DMG suitable for manual testing.

The app may still trigger Gatekeeper warnings if it is not signed with a valid Developer ID certificate and notarized by Apple.

## Developer ID signing

To prepare a production-style macOS build, provide a valid Developer ID Application certificate:

```bash
export GLUCODESK_CODESIGN_IDENTITY="Developer ID Application: Your Name (TEAMID)"
```

The packaging script already signs the `.app` bundle when this variable is configured.

## Notarization with a keychain profile

Recommended approach:

```bash
export GLUCODESK_CODESIGN_IDENTITY="Developer ID Application: Your Name (TEAMID)"
export GLUCODESK_NOTARIZE=true
export GLUCODESK_NOTARY_KEYCHAIN_PROFILE="glucodesk-notary"

./scripts/package-macos-preview.sh 0.2.1-preview
```

The script will:

1. create the `.app` bundle;
2. sign the `.app` bundle;
3. create the install-friendly DMG;
4. submit the DMG to Apple notarization;
5. wait for notarization to complete;
6. staple the notarization ticket to the DMG;
7. validate the stapled ticket;
8. generate SHA256 checksums.

## Notarization with Apple ID credentials

Alternative approach:

```bash
export GLUCODESK_CODESIGN_IDENTITY="Developer ID Application: Your Name (TEAMID)"
export GLUCODESK_NOTARIZE=true
export GLUCODESK_NOTARY_APPLE_ID="name@example.com"
export GLUCODESK_NOTARY_TEAM_ID="TEAMID"
export GLUCODESK_NOTARY_PASSWORD="app-specific-password"

./scripts/package-macos-preview.sh 0.2.1-preview
```

Prefer a keychain profile for local development and CI/CD environments.

## Require notarization during verification

To require notarization validation during artifact verification:

```bash
export GLUCODESK_REQUIRE_NOTARIZATION=true

./scripts/verify-macos-preview-artifacts.sh 0.2.1-preview
```

This validates the stapled ticket and, when available, performs a Gatekeeper assessment with `spctl`.

## Notes

- Notarization requires an Apple Developer account.
- Notarization requires a valid Developer ID Application certificate.
- The default preview flow intentionally skips notarization.
- Checksums are generated after notarization, so release checksums match the final stapled DMG.
