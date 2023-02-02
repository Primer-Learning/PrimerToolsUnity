using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    internal class ScrubbablePlayable : GenericBehaviour
    {
        [SerializeReference]
        [TypeFilter(nameof(GetScrubbables))]
        [Tooltip("Extend Scrubbable class to add more options")]
        public Scrubbable scrubbable;

        [SerializeField]
        [ShowIf(nameof(scrubbable))]
        [MethodOf(nameof(scrubbable), parameters = new[] { typeof(float) })]
        [Tooltip("Method to call when the clip is scrubbed. Only methods with a float parameter are shown here")]
        internal MethodInvocation scrubbableMethod;


        static ScrubbablePlayable() => SetIcon<ScrubbablePlayable>('║');

        public override string playableName => scrubbable?.GetType().Name;


        public override Transform trackTarget {
            get => base.trackTarget;
            internal set {
                base.trackTarget = value;

                if (scrubbable is not null)
                    scrubbable.target = value;
            }
        }


        #region Scrubbable management
        private bool isInitialized = false;

        public void Prepare()
        {
            if (isInitialized || scrubbable is null)
                return;

            scrubbable.Prepare();
            isInitialized = true;
        }

        public void Cleanup()
        {
            if (!isInitialized || scrubbable is null)
                return;

            scrubbable.Cleanup();
            isInitialized = false;
        }

        public void Execute(float time)
        {
            if (scrubbable is null) {
                Debug.LogWarning(
                    $"[{this}] no scrubbable selected.\nYou can create more scrubbables by extending {nameof(Scrubbable)} class"
                );

                return;
            }

            var t = Mathf.Clamp01((time - start) / duration);
            scrubbableMethod.Invoke(scrubbable, t);
        }
        #endregion

        public override string ToString()
        {
            return $"Scrubbable {icon} {scrubbableMethod.ToString(scrubbable)}";
        }

        internal static IEnumerable<Type> GetScrubbables()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(Scrubbable)) && !type.IsAbstract)
                .ToArray();
        }
    }
}
