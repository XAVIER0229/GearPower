# History identity and Apple Bluetooth compatibility

## Duplicate history cards

History was keyed only by Windows container/instance ID. Re-enumerating a
receiver can change that ID, leaving the same physical peripheral displayed
both online and as a disconnected history card. This is an identity-matching
defect, not a second connected peripheral.

New records can store an optional hardware identity in the trailing TSV column:
Logitech HID++ feature 0x0003 model/unit ID, or an available HID serial number.
Matching hardware identities reconcile old history and retain current battery
and device classification. Empty, zero or invalid HID++ identifiers are rejected.
The history file is backed up before migration; a current custom display name
takes priority, otherwise the old custom name is inherited.

Older history files lack these identifiers. A compatible disconnected card now
offers **Merge**, asking the user to confirm it is the same physical device.
Two devices with conflicting hardware identities cannot be merged this way.
Names alone never trigger automatic merging. After confirmation, subsequent
records retain hardware identity. If hardware identity is unavailable, future
Windows identity changes may still need explicit merging. Separate same-model
devices remain separate.

## Disconnected Classic Bluetooth devices

Windows PnP battery values can remain available after a device disconnects.
Their presence is not evidence of an active connection. Classic Bluetooth
battery-property readings now require a positive fConnected result from the
Windows Bluetooth API, mapped by exact Bluetooth address and container.
No inquiry, pairing, connection or registry modification is performed.
Disconnected/unknown Classic states do not produce online battery cards;
existing history remains available. This was checked with a disconnected
HUAWEI FreeBuds Pro 2 whose cached 100% previously appeared online.

The Win32 API used here does not enumerate Bluetooth LE; its pre-existing
battery-property path is preserved and was not fully validated by this work.

## Apple Magic Mouse A1296 (05AC:030D)

Generic HID Feature queries use a zero-access handle rather than requiring
read/write access to a mouse collection Windows can hold exclusively.
Feature 0x47 returned [47 64] (100%). No descriptor or driver changes were
necessary for this mouse. Percentage alone no longer fabricates a charging/full
flag: a replaceable-battery mouse at 100% is not "charged".

## Apple Wireless Keyboard 2011 ANSI (05AC:0255)

The model-specific reader queries read-only Feature 0x47 then the declared
Input form. Valid values are percentages, not scaled against the descriptor's
logical maximum of 255. Failed queries retain an unknown card with diagnostics.

**Windows prerequisite:** tested pairings exposed 0x47 as Input only, Feature
length 0 on the battery collection. Direct Feature reads failed with error 1,
Input reads with error 87. Application changes alone did not resolve this.
A separate, explicitly approved per-device cached-descriptor correction added
a Feature Battery Strength declaration while retaining Input. After Bluetooth
reload, Feature length became 2 and the reader returned [47 64] (100%).

This contribution does not patch Windows, install drivers, remove pairing or
include the device-specific repair utility used during local testing. Replacing
the exe alone does not fix an uncorrected stack. If the project adopts a cache
remediation workflow, it needs separate device validation, backup and restoration.

Protocol behavior was independently implemented using the references in
THIRD_PARTY_NOTICES.md; no reference-project source was copied.

## Validation

- Windows .NET Framework/C# 5 build; 123 executable self-tests passed.
- 14 isolated history checks: port changes, restart/load, latest battery/kind,
  names, backups, legacy merging, disconnected history and separate units.
- PRO X Wireless: fresh 79% and nonempty stable hardware identity observed.
- Existing Magic Keyboard with Numeric Keypad reading: 75%.
- Wireless Keyboard 05AC:0255: 100% after the separate cache correction.
- Magic Mouse 05AC:030D: 100%, no invented full/charging signal.
- Full scan excluded disconnected Huawei from current readings.
- The reporter ran the fourth local build and confirmed the UI test passed.
