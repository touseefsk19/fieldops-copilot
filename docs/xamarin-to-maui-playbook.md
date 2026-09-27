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
| RG.Plugins.Popup | MAUI built-in popups [or CommunityToolkit Popup: say which] |
| Plugin.Connectivity | MAUI Essentials `Connectivity` |
| Plugin.DeviceInfo | MAUI Essentials `DeviceInfo` |
| [others you replaced] | [ ] |

## Pitfalls that cost us time
- **Workload/package mismatch on Mac:** MAUI packages required a newer workload (10.0) than installed (8.0.82). Fix: align the workload with `sudo dotnet workload install maui` for the right SDK.
- **Works in the simulator, crashes as a release IPA:** [what the cause was, e.g. trimming/linker removing reflection-used code, and the fix].
- **Images:** moved from 1x/2x/3x PNGs to single SVGs in `Resources/Images`; MAUI generates platform sizes at build.
- **Navigation/UI differences:** [NavigationPage/Shell title and navigation-bar behaviour on iOS, ListView selection staying grey: say how you fixed each].
- [One more real pitfall]

## Result
All 7 apps live on .NET MAUI November 2025, with no major production incidents.