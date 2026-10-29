namespace FieldOps.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		#if MACCATALYST
        FlyoutBehavior = FlyoutBehavior.Locked;    // Mac: sidebar always open, like a desktop app
#else
        FlyoutBehavior = FlyoutBehavior.Disabled;  // phones: no sidebar, just the bottom tabs
#endif
	}
}
