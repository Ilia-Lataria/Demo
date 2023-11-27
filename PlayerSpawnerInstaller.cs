// Unity
using UnityEngine;

// 3D Party
using Zenject;

// Project dependencies
using Infable.DarkWaves.Core.Player;
using Infable.DarkWaves.Core.PlayerSpawner.Services;

namespace Infable.DarkWaves.Core.PlayerSpawner.Installers
{
    public class PlayerSpawnerInstaller : Installer<PlayerSpawnerInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<PlayerSpawnerService>()
                .FromSubContainerResolve()
                .ByInstaller<PlayerSpawnerSubContainerInstaller>()
                .AsSingle();
        }

        private class PlayerSpawnerSubContainerInstaller : Installer
        {
            public override void InstallBindings()
            {
                Container
                    .Bind<PlayerSpawnerService>()
                    .AsSingle();
                
                Container
                    .BindFactory<Object, PlayerBase, PlayerBase.Factory>()
                    .FromFactory<PlayerCustomFactory>();
            }
        }
    }
}
