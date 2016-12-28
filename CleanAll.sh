#!/bin/bash

find ./*/obj/Debug -type d -exec rm -rv {} \; 2>/dev/null &
find ./*/obj/Release -type d -exec rm -rv {} \; 2>/dev/null &
find ./*/bin -type d -exec rm -rv {} \; 2>/dev/null &
find packages/* -type d -exec rm -rv {} \; 2>/dev/null &
wait
