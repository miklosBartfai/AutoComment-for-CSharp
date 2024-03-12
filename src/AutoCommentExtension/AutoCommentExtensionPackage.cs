global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;

namespace AutoCommentExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.AutoCommentExtensionString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "Auto Comment", "General", 0, 0, true, SupportsProfiles = true)]
    public sealed class AutoCommentExtensionPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
        }
    }
}