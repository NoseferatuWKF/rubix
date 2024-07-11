@echo off

set source_path="%USERPROFILE%/utils/native/vdm.cpp"
set build_path="./out"

mkdir %build_path%
pushd %build_path%

REM window manager
REM cl wm.cpp user32.lib
REM cl /c wm_dll.cpp
REM link wm_dll.obj user32.lib /dll

REM keyboard
cl /O2 %source_path% /link user32.lib ole32.lib

popd
