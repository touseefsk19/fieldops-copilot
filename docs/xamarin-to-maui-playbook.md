# Xamarin.Forms → .NET MAUI: a migration playbook from 7 enterprise apps

*Touseef Ahamed S K · Technical Lead · lessons from migrating 7 production apps for SABIC 

## Context
Xamarin support ended in May 2024. We migrated 7 enterprise apps (Android + iOS), and I personally migrated 4. Apps ranged from simple news readers to complex apps with offline data, barcode scanning and certificate-based auth.

## The order that worked
1. **Audit first.** List every NuGet package, custom renderer, effect and platform-specific file. Mark each as *has MAUI version* / *needs replacement* / *needs rewrite*.
2. **Start with the smallest app** to learn the pitfalls, then do the critical ones. 
3. **New single project, then move code in.** Shared code first, then views, then platform code.
4. **Renderers → handlers.** Small tweaks become `Mapper.AppendToMapping`; complex controls become custom handlers.
5. **Replace unsupported libraries** (table below).
6. **Test on real devices**, including the oldest OS versions your users run.
7. **Release through the MDM** with the Xamarin build kept as a fallback until the MAUI build is stable.

## Library replacements we made
| Xamarin package | MAUI replacement |
|---|---|
| SignaturePad | CommunityToolkit.Maui `DrawingView` |
| RG.Plugins.Popup | MAUI's built-in popups (`DisplayAlert`, `DisplayActionSheet`, `DisplayPromptAsync`) |
| Plugin.Connectivity | MAUI Essentials `Connectivity` |
| Plugin.DeviceInfo | MAUI Essentials `DeviceInfo` |
| PushNotification.Plugin | Platform registration (APNs / Android) wired through MAUI lifecycle events |
| Microcharts (Xamarin) | Microcharts.Maui + SkiaSharp, with handlers registered in `MauiProgram` |

## Pitfalls that cost us time
- **Workload/package mismatch on Mac:** MAUI packages required a newer workload (10.0) than the one installed (8.0.82). Fix: install the MAUI workload that matches the SDK (`sudo dotnet workload install maui`) and keep SDK, workload and package versions aligned across the team.
- **Works in the simulator, crashes as a release IPA:** release builds are compiled ahead of time and trimmed by the linker, so code only reached through reflection can be removed. Fix: compare debug and release build settings, keep the affected types from being trimmed, and always smoke-test the actual release build on a real device before distribution.
- **Images:** moved from 1x/2x/3x PNGs to single SVGs in `Resources/Images`; MAUI generates the platform sizes at build time.
- **Navigation and UI differences:** Shell and NavigationPage show titles and navigation bars differently on iOS, so we set navigation-bar visibility and titles explicitly per page. `ListView` kept the selected row highlighted after a tap, so we cleared `SelectedItem` after handling the tap (or used `CollectionView` with an explicit `SelectionMode`).
- **Third-party plugins drive the effort:** most of the time went into packages with no MAUI version, not into our own code. Audit packages first and estimate from that list.

## Result
All 7 apps live on .NET MAUI November 2025, with no major production incidents.