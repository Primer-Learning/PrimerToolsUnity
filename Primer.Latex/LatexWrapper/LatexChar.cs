using System;
using System.Linq;
using UnityEngine;

namespace Primer.Latex
{
    public sealed record LatexChar(Sprite sprite, Vector3 position)
    {
        public bool IsSameSprite(LatexChar other) => other != null && sprite.Equals(other.sprite);
        public bool IsSamePosition(LatexChar other) => other != null && position == other.position;

        public bool Equals(LatexChar other) => IsSameSprite(other) && IsSamePosition(other);

        public override int GetHashCode() => HashCode.Combine(sprite, position);
    }


    public static class LatexCharArrayExtensions
    {
        /// <summary>
        ///     This is expected to happen when a LatexRenderer is
        ///     (a) initialized from defaults,
        ///     (b) set via a preset in the editor
        ///     (c) and occasionally when undoing/redoing.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The sprite assets are added as subassets of the scene the LatexRenderer is in, and can be
        ///         garbage collected any time if there's no LatexRenderer referencing them. This means when
        ///         redoing/undoing you can arrive in a state where the LatexRenderer is pointing at sprites
        ///         that have been cleaned up. Fortunately Unity notices when deserializing and the sprites
        ///         appear as null values in our list.
        ///     </para>
        ///     <para>
        ///         Presets that refer to a Sprite also don't prevent the sprite from being garbage collected
        ///         (and the preset could be applied to a LatexRenderer in a different scene anyways). So
        ///         presets often cause this same issue. Finally, when editing a preset directly, we actually
        ///         set its stored _sprites value to null directly since we need to make sure we never have a
        ///         mismatch between Latex & Headers text and the stored sprites.
        ///     </para>
        ///     <para>
        ///         I suspect there's a way to handle these situations using hooks into various parts of the
        ///         editor. But a decently thorough dive into the options had me arrive at the conclusion that
        ///         the approach here (just recognizing the mismatch and having the inspector rebuild) is the
        ///         simplest approach. Hopefully as my domain knowledge of the editor improves I'll think of an
        ///         even cleaner way though.
        ///     </para>
        /// </remarks>
        public static bool AreSpritesValid(this LatexChar[] characters)
        {
            // be a special "null unity object". `value == null` or `!value` need to be used to check it.
            // If the Sprite has been garbage collected, it will not be exactly null but will instead
            return characters is not null && characters.All(x => (bool)x.sprite);
        }
    }
}
