#!/usr/bin/env bash
set -euo pipefail

# Only PlayMode needs a rendered Game view (including WaitForEndOfFrame).
# Activation, EditMode, result combining and license return keep GameCI's
# original batch-mode launcher and arguments.
run_tests=false
test_platform=''
arguments=("$@")
for ((index = 0; index < ${#arguments[@]}; index++)); do
  case "${arguments[index],,}" in
    -runtests) run_tests=true ;;
    -testplatform) test_platform="${arguments[index + 1]:-}" ;;
  esac
done

if [[ "$run_tests" == true && "${test_platform,,}" == playmode ]]; then
  rendering_arguments=()
  for argument in "${arguments[@]}"; do
    case "${argument,,}" in
      -batchmode|-nographics) ;;
      *) rendering_arguments+=("$argument") ;;
    esac
  done
  echo '[H-ONE CI] PlayMode: GameCI Xvfb launcher, graphics enabled, batch mode disabled.'
  export LIBGL_ALWAYS_SOFTWARE=1
  # A stalled GUI must return control to GameCI so it can return the license.
  exec timeout --kill-after=30s 20m /usr/bin/unity-editor-rendering "${rendering_arguments[@]}"
fi

exec /usr/bin/unity-editor-batchmode "${arguments[@]}"
