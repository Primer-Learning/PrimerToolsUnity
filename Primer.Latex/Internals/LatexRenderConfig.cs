using System;
using System.Collections.Generic;
using System.Linq;

namespace Primer.Latex
{
    public sealed record LatexRenderConfig(string Latex, IReadOnlyList<string> Headers)
    {
        public bool IsEmpty => string.IsNullOrWhiteSpace(Latex);

        public bool Equals(LatexRenderConfig other) => other != null &&
                                                       Latex == other.Latex &&
                                                       // We compare them with == because we want to know if both of them are null
                                                       // or both o them are the same object, in both cases we don't compare their content
                                                       // ReSharper disable once PossibleUnintendedReferenceComparison
                                                       (Headers == other.Headers || Headers.SequenceEqual(other.Headers));

        public override int GetHashCode() => HashCode.Combine(Latex, Headers);

        public static List<string> GetDefaultHeaders() => new() {
            @"\documentclass[preview]{standalone}",
            @"\usepackage[english]{babel}",
            @"\usepackage[utf8]{inputenc}",
            @"\usepackage[T1]{fontenc}",
            @"\usepackage{amsmath}",
            @"\usepackage{amssymb}",
            @"\usepackage{dsfont}",
            @"\usepackage{setspace}",
            @"\usepackage{tipa}",
            @"\usepackage{relsize}",
            @"\usepackage{textcomp}",
            @"\usepackage{mathrsfs}",
            @"\usepackage{calligra}",
            @"\usepackage{wasysym}",
            @"\usepackage{ragged2e}",
            @"\usepackage{physics}",
            @"\usepackage{xcolor}",
            @"\usepackage{microtype}",
            @"\usepackage{pifont}",
            @"\linespread{1}"
        };
    }
}
