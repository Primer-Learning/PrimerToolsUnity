SCRIPT_DIR="$(
    cd "$(dirname "${BASH_SOURCE[0]}")" || exit 1
    pwd -P
)"

set -eux

latex -interaction=batchmode -halt-on-error -output-directory=/tmp/ "$1"
dvisvgm "/tmp/${1%.*}.dvi" --no-fonts --verbosity=0 --output="${1%.*}.svg"
