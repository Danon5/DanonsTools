﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DanonsTools.ModuleSystem;
using DanonsTools.ServiceLayer;
using JetBrains.Annotations;
using UnityEngine;

namespace DanonsTools.BootstrapSystem
{
    public sealed class BootstrapRoot : MonoBehaviour
    {
        [SerializeField] private List<ScriptableBootstrap> _bootstraps = new();

        [UsedImplicitly]
        private void Awake()
        {
            foreach (var bootstrap in _bootstraps)
                bootstrap.InjectDependencies();
        }

        [UsedImplicitly]
        private async UniTaskVoid Start()
        {
            var moduleLoader = ServiceLocator.Retrieve<IModuleService>().ModuleLoader;

            if (moduleLoader == null)
                throw new Exception("Bootstrap module load failed. No module service available.");
            
            foreach (var bootstrap in _bootstraps)
                await bootstrap.Load(moduleLoader);
        }
    }
}