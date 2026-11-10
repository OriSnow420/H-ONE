#!/usr/bin/env bash
set -euo pipefail

# Only PlayMode needs a rendered Game view (including WaitForEndOfFrame).
# Activation, EditMode, result combining and license return keep GameCI's
# original batch-mode launcher and arguments.
run_tests=false
test_platform=''
test_log=''
arguments=("$@")
for ((index = 0; index < ${#arguments[@]}; index++)); do
  case "${arguments[index],,}" in
    -runtests) run_tests=true ;;
    -testplatform) test_platform="${arguments[index + 1]:-}" ;;
    -logfile) test_log="${arguments[index + 1]:-}" ;;
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
  # Store file/connection metadata and exit status, never file contents,
  # command arguments or environment values (tools may pass auth tokens).
  trace_log="${test_log:?PlayMode requires -logFile}.startup-trace.log"
  set +e
  # Detach the tracer so a surviving LicensingClient does not delay the
  # launcher's return or hide the Editor's actual exit code.
  timeout --kill-after=30s 20m strace -D -f \
    -e trace=execve,openat,newfstatat,access,readlink,connect,exit_group -s 200 \
    -o >(sed -unE \
      -e 's@^([0-9]+[[:space:]]+)?execve\(("[^"]*"),.*\) = (.*)$@\1execve(\2, [arguments omitted]) = \3@p' \
      -e '/^[0-9]+[[:space:]]+(openat|newfstatat|access|readlink|connect|exit_group)\(/p' > "$trace_log") \
    /usr/bin/unity-editor-rendering "${rendering_arguments[@]}"
  test_exit=$?
  set -e
  tail -n 120 "$trace_log"
  exit "$test_exit"
fi

exec /usr/bin/unity-editor-batchmode "${arguments[@]}"
