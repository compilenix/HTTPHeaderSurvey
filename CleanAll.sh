#!/bin/bash

find ./Source/*/obj/Debug -type d -exec rm -rv {} \; 2>/dev/null &
find ./Source/*/obj/Release -type d -exec rm -rv {} \; 2>/dev/null &
find ./Source/*/bin -type d -exec rm -rv {} \; 2>/dev/null &
find packages/* -type d -exec rm -rv {} \; 2>/dev/null &
find *.obj -type f -exec rm -v {} \; 2>/dev/null &
find *.tmp -type f -exec rm -v {} \; 2>/dev/null &
find *.log -type f -exec rm -v {} \; 2>/dev/null &
find *.vspscc -type f -exec rm -v {} \; 2>/dev/null &
find *.vssscc -type f -exec rm -v {} \; 2>/dev/null &
find *.cache -type f -exec rm -v {} \; 2>/dev/null &
rm -r -v ./.vs &
rm -r -v ./publish &
wait
