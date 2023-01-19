using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Primer
{
    public static class PrefabProvider_ChildrenDeclaration_Integration
    {
        public static T NextIsInstanceOf<T>(
            this IChildrenDeclaration declaration,
            PrefabProvider<T> provider,
            ref T cache,
            string name = null,
            Action<T> init = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = -1)
            where T : Component
        {
            if (provider.value == null)
                return null;

            ForceReinitializationIfRequired($"{callerFile}:{callerLine}:{name}", declaration, provider);

            return declaration.NextIsInstanceOf(
                prefab: provider.value,
                ref cache,
                name,
                init: value => {
                    provider.Initialize(value);
                    init?.Invoke(value);
                }
            );
        }

        public static T NextIsInstanceOf<T>(
            this IChildrenDeclaration declaration,
            PrefabProvider<T> provider,
            string name = null,
            Action<T> init = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = -1)
            where T : Component
        {
            if (provider.value == null)
                return null;

            ForceReinitializationIfRequired($"{callerFile}:{callerLine}:{name}", declaration, provider);

            return declaration.NextIsInstanceOf(
                prefab: provider.value,
                name,
                init: value => {
                    provider.Initialize(value);
                    init?.Invoke(value);
                }
            );
        }


        #region Force reinitialization
        private static readonly HashSet<string> initialized = new();

        private static void ForceReinitializationIfRequired<T>(
            string key,
            IChildrenDeclaration declaration,
            PrefabProvider<T> provider
        ) where T : Component
        {
            if (provider.hasChanges) {
                initialized.Clear();
                provider.hasChanges = false;
            }

            if (initialized.Contains(key))
                return;

            initialized.Add(key);
            declaration.ReinitializeNextChild();
        }
        #endregion
    }
}
