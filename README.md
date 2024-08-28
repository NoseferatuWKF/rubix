# Motivation
Improving the navigation between virtual desktops and windows for Windows 10/11

# Credits
- NickoTin: https://www.cyberforum.ru/blogs/105416/blog3671.html (for reverse engineering the virtual desktop manager)
- MScholtes: https://github.com/MScholtes/VirtualDesktop (for the C# version)

# Usage

## EXE
Download the exe from release and run it normally, should work with/without escalated priviledges.
Because the program runs in the background, you can add it to program startups for better experience.
If the program crashes by any chance, just re-run it again, if it is intermittent feel free to raise an issue.

## Build from source
The project is using .NET 8, could work with some previous/latest versions but have not done the full validation yet.
Basically with .NET SDK installed, go to the dotnet root folder, and run `dotnet build -c Release`

## Keymaps
```
- Alt + [1..9] : Switch Virtual Desktop based on index
- Alt + Shift + [1..9] : Move Foreground Window to Virtual Desktop based on index
```
