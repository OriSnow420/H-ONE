set pagination off
set print frame-arguments none
set startup-with-shell off
set debuginfod enabled off
set follow-fork-mode parent
set detach-on-fork on
handle SIGPIPE nostop noprint pass
break exit
commands
  silent
  echo [H-ONE CI] Native Editor exit call stack (arguments omitted):\n
  bt 24
  continue
end
run
