﻿using System;
using System.Collections.Generic;
using OpenSage.Data;

namespace OpenSage.Mods.Generals
{
    public class GeneralsZeroHourDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.CncGeneralsZeroHour;
        public string DisplayName => "Command & Conquer (tm): Generals - Zero Hour";
        public IGameDefinition BaseGame => GeneralsDefinition.Instance;

        public Type WndCallbackType => typeof(WndCallbacks);

        public string LauncherImagePath => "Install_Final.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer The First Decade", "zh_folder"),
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer Generals Zero Hour", "InstallPath"),
            new RegistryKeyPath(@"SOFTWARE\EA Games\Command and Conquer Generals Zero Hour", "Install Dir", "Command and Conquer Generals Zero Hour\\")
        };

        public static GeneralsZeroHourDefinition Instance { get; } = new GeneralsZeroHourDefinition();
    }
}