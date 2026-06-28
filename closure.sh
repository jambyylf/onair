#!/bin/bash
TMP=$(mktemp)
{
  ldd /ucrt64/bin/uxplay.exe 2>/dev/null
  for f in /ucrt64/lib/gstreamer-1.0/*.dll; do ldd "$f" 2>/dev/null; done
} | tr -s ' \t' '\n' | grep -iE '^/ucrt64/bin/.+\.dll$' | sort -u > "$TMP"
echo "DLL_COUNT=$(wc -l < "$TMP")"
echo "DLL_SIZE=$(du -ch $(cat "$TMP") 2>/dev/null | tail -1 | cut -f1)"
cp "$TMP" /c/Users/FALCON/ONAIR/needed-dlls.txt
echo "SAVED=/c/Users/FALCON/ONAIR/needed-dlls.txt"
