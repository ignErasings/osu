// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Security.Principal;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Desktop.Security
{
    /// <summary>
    /// Checks if the game is running with elevated privileges (as admin in Windows, root in macOS and Linux) and displays a warning notification if so.
    /// </summary>
    public class ElevatedPrivilegesChecker : Component
    {
        [Resolved]
        private INotificationOverlay notifications { get; set; }

        private bool elevated;

        [BackgroundDependencyLoader]
        private void load()
        {
            elevated = checkElevated();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (elevated)
                notifications.Post(new ElevatedPrivilegesNotification());
        }

        private bool checkElevated()
        {
            try
            {
                switch (RuntimeInfo.OS)
                {
                    case RuntimeInfo.Platform.Windows:
                        if (!OperatingSystem.IsWindows()) return true;

                        var windowsIdentity = WindowsIdentity.GetCurrent();
                        var windowsPrincipal = new WindowsPrincipal(windowsIdentity);

                        return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);

                    case RuntimeInfo.Platform.macOS:
                    case RuntimeInfo.Platform.Linux:
                        return Mono.Unix.Native.Syscall.geteuid() == 0;
                }
            }
            catch
            {
            }

            return false;
        }

        private class ElevatedPrivilegesNotification : SimpleNotification
        {
            public override bool IsImportant => true;

            public ElevatedPrivilegesNotification()
            {
                Text = $"Running osu! as {(RuntimeInfo.IsUnix ? "root/administrator" : "administrator/root")} does not improve performance, may break integrations and poses a security risk. Please run the game as a normal user.";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Icon = FontAwesome.Solid.ShieldAlt;
                IconBackground.Colour = colours.YellowDark;
            }
        }
    }
}
